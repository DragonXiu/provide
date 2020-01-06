﻿package com.wankun.richduckling.utils;

import com.alibaba.fastjson.JSONArray;
import com.alibaba.fastjson.JSONObject;
import org.thymeleaf.util.StringUtils;

import java.io.ByteArrayOutputStream;
import java.io.FileWriter;
import java.io.IOException;
import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * 获取中国城市相关数据
 *
 * @author wankun
 * @create 2019/4/10
 * @since 1.0.0
 */
public class ChinaCityNameUtils
{
    /**
     * 当前取得是国家统计局2018年的数据
     */
    private static final String WEB_URL = "http://www.stats.gov.cn/tjsj/tjbz/tjyqhdmhcxhfdm/2018/index.html";
    private static final String BASE_URL = "http://www.stats.gov.cn/tjsj/tjbz/tjyqhdmhcxhfdm/2018/";
    /**
     * 省份的对应的id
     * 测试时只取前两个或者前三个，加快测试速度
     */
    private static final int[] province_id = {11, 12, 13, 14, 15, 21, 22, 23, 31, 32, 33, 34, 35, 36, 37, 41, 42, 43, 44, 45, 46, 50, 51, 52,
            53, 54, 61, 62, 63, 64, 65};
    private static final String[] province_name = {"北京市", "天津市", "河北省", "山西省", "内蒙古自治区", "辽宁省", "吉林省", "黑龙江省",
            "上海市", "江苏省", "浙江省", "安徽省", "福建省", "江西省", "山东省", "河南省",
            "湖北省", "湖南省", "广东省", "广西壮族自治区", "海南省", "重庆市", "四川省", "贵州省",
            "云南省", "西藏自治区", "陕西省", "甘肃省", "青海省", "宁夏回族自治区", "新疆维吾尔自治区"
    };
    private static final String RGEX_IDS = "<a href=\\'\\d{2}\\/(.{1,30}).html\\'>(.{1,30})<\\/a><\\/td><\\/tr>";
    private static final String RGEX_NAMES = "<a href=\\'.*?.html\\'>(.{1,30})<\\/a><\\/td><\\/tr>";
    private static final String RGEX_CODES = "<td><a href=\\'.*?.html\\'>(.{1,30})<\\/a><\\/td><td>";
    private static final String RGEX_CODES_NO_A = "<tr class=\\'countytr\\'><td>(.{1,30})<\\/td><td>.*?<\\/td><\\/tr>";
    private static final String RGEX_NAMES_NO_A = "<tr class=\\'countytr\\'><td>.*?<\\/td><td>(.{1,30})<\\/td><\\/tr>";
 
    private static int cityFromIdId = 0;
    private static int provinceFromId = 0;
    private static int areaFormId = 0;

    /**
     * 表结构参考 如下创建表的sql
     */
    private static String CREAT_PROVINCE_SQL = "CREATE TABLE `province`(`pro_id` int(11),`pro_code` varchar(18),`pro_name` varchar(60),`pro_name2` varchar(60));";
    private static String CREAT_CITY_SQL = " CREATE TABLE `city`( `id` int(11),   `province_id` int(10),   `code` varchar(18),   `name` varchar(60),   `province_code` varchar(18));";
    private static String CREAT_AREA_SQL = " CREATE TABLE `area`(`id` int(11),   `city_id` int(10),   `code` varchar(18),   `name` varchar(60),   `city_code` varchar(18));";
    /**
     * 获取省市sql
     */
    public static void printProvinceSQL()
    {
        System.out.println("size:" + province_id.length);
        String sql = "";
        for (int i = 0; i < province_id.length; i++)
        {
            int id = i + 1;
            //注意id补零
            if (!StringUtils.isEmpty(sql))
            {
                sql = String.format("%s,", sql);
            }
            sql = String.format("%s(%s,'%s0000','%s','')", sql, id, province_id[i], province_name[i]);
        }
        sql = String.format("insert into province values %s ;", sql);
        System.out.println(sql);
    }

    /**
     * 获取 城市表sql
     * 注意：1、输出的sql 在控制台中获取
     * 2、清除表数据 delete from  area;
     */
    public static void printCitySQL()
    {
        System.out.println("获取城市数据");
        //城市表中的id
        cityFromIdId = 0;
        provinceFromId = 0;
        for (int i = 0; i < province_id.length; i++)
        {
            String url = BASE_URL + province_id[i] + ".html";
            provinceFromId++;
            String html = getHtml(url);

            try
            {
                //暂停等待获取网页数据
                String provinceCode = province_id[i] + "0000";
                // 匹配的模式
                String sql = getSqlStr(0, html, RGEX_NAMES, RGEX_CODES, provinceCode);
                sql = String.format("insert into city values %s ;", sql);
                System.out.println(sql);
            }
            catch (Exception e)
            {
                e.printStackTrace();
            }
        }
        System.out.println("城市总数：" + cityFromIdId);
    }

    /**
     * 获取地区的表sql
     */
    public static void printAreaSQL()
    {
        System.out.println("获取地区数据");
        areaFormId = 0;
        cityFromIdId = 0;
        for (int i = 0; i < province_id.length; i++)
        {
            String url = BASE_URL + province_id[i] + ".html";
            String html = getHtml(url);
            //暂停5秒等待获取网页数据
            try
            {
                // 匹配的模式
                List<String> ids = getSubUtil(html, RGEX_IDS);
                List<String> codes = getSubUtil(html, RGEX_CODES);
                for (int n = 0; n < ids.size(); n++)
                {
                    cityFromIdId++;
                    //获取区级的数据
                    String urlQu = BASE_URL + province_id[i] + "/" + ids.get(n) + ".html";
                    String htmlQu = getHtml(urlQu);
                    //部分省市的区级 不一样，没有跳转事件，所以 正则不一样
                    String sql = getSqlStr(1, htmlQu, RGEX_NAMES_NO_A, RGEX_CODES_NO_A, codes.get(n));
                    sql = String.format(StringUtils.isEmpty(sql) ? "%s %s" : "%s,%s", sql, getSqlStr(1, htmlQu, RGEX_NAMES, RGEX_CODES, codes.get(n)));
                    sql = String.format("insert into area values %s ;", sql);
                    System.out.println(sql);
                }
            }
            catch (Exception e)
            {
                e.printStackTrace();
            }
        }
        System.out.println("地区总数：" + areaFormId);
    }

    /**
     * 通过正则获取 html中的相关数据，并拼接Sql语句
     *
     * @param type 表类型， 0 城市表， 1 地区表
     * @return
     */
    private static String getSqlStr(int type, String html, String rgexNames, String rgexCodes, String parentsCode)
    {
        String sql = "";
        int formId = 0;
        int parentsFromIdId = 0;
        List<String> names = getSubUtil(html, rgexNames);
        List<String> codes = getSubUtil(html, rgexCodes);
        for (int q = 0; q < names.size(); q++)
        {
            if (type == 0)
            {
                cityFromIdId++;
                parentsFromIdId = provinceFromId;
                formId = cityFromIdId;
            }
            else
            {
                areaFormId++;
                parentsFromIdId = cityFromIdId;
                formId = areaFormId;
            }
            if (!StringUtils.isEmpty(sql))
            {
                sql = String.format("%s,", sql);
            }
            //拼接插入数据的SQL语句
            sql = String.format("%s(%s,%s,%s,'%s',%s)", sql, formId, parentsFromIdId, codes.get(q), names.get(q), parentsCode);
        }
        return sql;
    }


    /**
     * 获取 城市json
     *
     * @return
     */
    public static JSONArray getCitiesJson()
    {
        System.out.println("获取citiesJson");
        JSONArray jsonArray = new JSONArray();
        //城市id和区id 计算全部的，方便存数据库
        int cityId = 0;
        int areaId = 0;
        int provinceId = 0;
        //第一级为省市
        for (int p = 0; p < province_id.length; p++)
        {
            try
            {
                JSONObject province = new JSONObject();
                provinceId = p + 1;
                province.put("pro_id", provinceId);
                //注意补零
                province.put("pro_code", province_id[p] + "0000");
                province.put("pro_name", province_name[p]);
                //第二级 城市
                System.out.println("开始获取" + province_name[p] + "的城市数据");
                JSONArray cities = new JSONArray();
                String url = BASE_URL + province_id[p] + ".html";
                String html = getHtml(url);
                List<String> ids = getSubUtil(html, RGEX_IDS);
                List<String> names = getSubUtil(html, RGEX_NAMES);
                List<String> codes = getSubUtil(html, RGEX_CODES);
                for (int c = 0; c < ids.size(); c++)
                {
                    int cityID = ++cityId;
                    JSONObject city = new JSONObject();
                    city.put("city_id", cityID);
                    city.put("city_code", codes.get(c));
                    city.put("city_name", names.get(c));
                    //第三级 区级
                    JSONArray areas = new JSONArray();
                    String urlArea = BASE_URL + province_id[p] + "/" + ids.get(c) + ".html";
                    String htmlArea = getHtml(urlArea);
                    List<String> namesQu = new ArrayList<>();
                    List<String> codesQu = new ArrayList<>();
                    //有的市有"直辖区"，正则处理方式不一样
                    namesQu.addAll(getSubUtil(htmlArea, RGEX_NAMES_NO_A));
                    codesQu.addAll(getSubUtil(htmlArea, RGEX_CODES_NO_A));
                    namesQu.addAll(getSubUtil(htmlArea, RGEX_NAMES));
                    codesQu.addAll(getSubUtil(htmlArea, RGEX_CODES));
                    for (int a = 0; a < namesQu.size(); a++)
                    {
                        int areaID = ++areaId;
                        JSONObject area = new JSONObject();
                        area.put("area_id", areaID);
                        area.put("area_code", codesQu.get(a));
                        area.put("area_name", namesQu.get(a));
                        areas.add(area);
                    }
                    city.put("city_areas", areas);
                    cities.add(city);
                }
                province.put("pro_cities", cities);
                jsonArray.add(province);
            }
            catch (Exception e)
            {
                e.printStackTrace();
            }
        }
        //全部数据太长了，控制台打印不全，写入本地文件
        System.out.println(jsonArray.toString());
        writeFile(jsonArray.toString());
        return jsonArray;
    }


    /**
     * 将json字段写入本地D盘
     *
     * @param str
     */
    public static void writeFile(String str)
    {
        FileWriter fw = null;
        //设置日期格式
        SimpleDateFormat df = new SimpleDateFormat("yyyyMMddHHmmss");
        String fileName = "D:\\china_city_name_" + df.format(new Date()) + ".txt";
        try
        {
            //经过测试：FileWriter执行耗时:3,9，5 毫秒
            fw = new FileWriter(fileName);
            fw.write(str);
        }
        catch (Exception e)
        {
            e.printStackTrace();
        }
        finally
        {
            try
            {
                fw.close();
            }
            catch (Exception e)
            {
                e.printStackTrace();
            }
        }

    }


    /**
     * 通过url获取网页的源码
     *
     * @param htmlUrl
     * @return
     */
    public static String getHtml(String htmlUrl)
    {
        HttpURLConnection conn = null;
        try
        {
            URL url = new URL(htmlUrl);
            conn = (HttpURLConnection)url.openConnection();
            //设置输入流采用字节流
            conn.setDoInput(true);
            //设置输出流采用字节流
            conn.setDoOutput(true);
            //设置缓存
            conn.setUseCaches(false);
            conn.setRequestProperty("Content-Type", "application/x-www-form-urlencoded");
            conn.setRequestProperty("Charset", "utf-8");
            //请求方法为GET
            conn.setRequestMethod("GET");
            //设置连接超时为5秒
            conn.setConnectTimeout(5000);
            //服务器返回东西了，先对响应码判断
            if (conn.getResponseCode() == 200)
            {
                //用getInputStream()方法获得服务器返回的输入流
                InputStream in = conn.getInputStream();
                byte[] data = read(in);
                //这里使用gbk，使在控制台打印数据不乱码
                String html = new String(data, "gbk");
                in.close();
                return html;
            }
            else
            {
                return "数据错误";
            }
        }
        catch (IOException e)
        {
            e.printStackTrace();
            return "数据错误";
        }
    }


    /**
     * 流转换为二进制数组，
     *
     * @param inStream
     * @return
     * @throws IOException
     */
    public static final byte[] read(InputStream inStream)
            throws IOException
    {
        ByteArrayOutputStream swapStream = new ByteArrayOutputStream();
    byte[] buff = new byte[100];
    int rc = 0;
        while ((rc = inStream.read(buff, 0, 100)) > 0) {
            swapStream.write(buff, 0, rc);
        }
byte[] in2b = swapStream.toByteArray();
        return in2b;
    }
 
    /**
     * 正则表达式匹配两个指定字符串中间的内容
     *
     * @param soap
     * @return
     */
    public static List<String> getSubUtil(String soap, String rgex)
{
    List<String> list = new ArrayList<String>();
    // 匹配的模式
    // 把规则编译成模式对象
    Pattern pattern = Pattern.compile(rgex);
    Matcher m = pattern.matcher(soap);
    while (m.find())
    {
        int i = 1;
        list.add(m.group(i));
        i++;
    }
    return list;
}
}
