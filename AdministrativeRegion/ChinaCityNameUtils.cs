using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Collections;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.IO;

namespace AdministrativeRegion
{
    class ChinaCityNameUtils
    {
        /*当前得国家统计局*/
        private static string WEB_URL = "http://www.stats.gov.cn/tjsj/tjbz/tjyqhdmhcxhfdm/2018/index.html";
        private static string BASE_URL = "http://www.stats.gov.cn/tjsj/tjbz/tjyqhdmhcxhfdm/2018/";
        /*省份对应的ID,测试时只取两个或者三个加快测试速度*/
        private static int[] province_id = {11, 12, 13, 14, 15, 21, 22, 23, 31, 32, 33, 34, 35, 36, 37, 41, 42, 43, 44, 45, 46, 50, 51, 52,
            53, 54, 61, 62, 63, 64, 65};
        private static string[] province_name = {"北京市", "天津市", "河北省", "山西省", "内蒙古自治区", "辽宁省", "吉林省", "黑龙江省",
            "上海市", "江苏省", "浙江省", "安徽省", "福建省", "江西省", "山东省", "河南省",
            "湖北省", "湖南省", "广东省", "广西壮族自治区", "海南省", "重庆市", "四川省", "贵州省",
            "云南省", "西藏自治区", "陕西省", "甘肃省", "青海省", "宁夏回族自治区", "新疆维吾尔自治区"
             };
        private static String RGEX_IDS = "<a href=\\'\\d{2}\\/(.{1,30}).html\\'>(.{1,30})<\\/a><\\/td><\\/tr>";
        private static String RGEX_NAMES = "<a href=\\'.*?.html\\'>(.{1,30})<\\/a><\\/td><\\/tr>";
        private static String RGEX_CODES = "<td><a href=\\'.*?.html\\'>(.{1,30})<\\/a><\\/td><td>";
        private static String RGEX_CODES_NO_A = "<tr class=\\'countytr\\'><td>(.{1,30})<\\/td><td>.*?<\\/td><\\/tr>";
        private static String RGEX_NAMES_NO_A = "<tr class=\\'countytr\\'><td>.*?<\\/td><td>(.{1,30})<\\/td><\\/tr>";

        private static int cityFromId = 0;
        private static int provinceFromId = 0;
        private static int areaFormId = 0;
        /*表结构参考 如下创建表的sql*/
        public static string CREAT_PROVIINCE_SQL = "CREAT TABLE province( pro_id int,pro_code varchar(18),pro_name varchar(60),pro_name2 varchar(60))";
        public static string CREAT_CITY_SQL = "CREAT TABLE city( id int,province_id int,code varchar(18),name varchar(60),province_code varchar(60))";
        public static string CREAT_AREA_SQL = "CREAT TABLE city( id int,province_id int,code varchar(18),name varchar(60),province_code varchar(60))";
        /*获取省市sql*/
        public static void printProvinceSQL()
        {
            Console.WriteLine("获取省");
            Console.WriteLine("size:" + province_id.Length);
            String sql = "";
            for (int i = 0; i < province_id.Length; i++)
            {
                int id = i + 1;
                //注意id补零
                if (!string.IsNullOrEmpty(sql))
                {
                    sql = string.Format("{0}", sql);
                }
                sql = string.Format("{0}({1},'{2}0000','{3}','')", sql, id, province_id[i], province_name[i]);
            }
            sql = string.Format("insert into province {0};", sql);
            Console.WriteLine(sql);
        }
        /*获取 城市表 SQL*/
        public static void printCitySql()
        {
            Console.WriteLine("获取城市数据");
            //城市表中的id
            cityFromId = 0;
            provinceFromId = 0;
            for (int i = 0; i < province_id.Length; i++)
            {
                string url = BASE_URL + province_id[i] + ".html";
                provinceFromId++; 
                string html = getHtml(url);
                try
                {
                    //暂停等待过去网页数据
                    string provinceCode = province_id[i] + "0000";
                    //匹配的模式
                    string sql = getSqlstr(0,html,RGEX_NAMES,RGEX_CODES, provinceCode);
                    sql = string.Format("insert into city values {0};", sql);
                    Console.WriteLine(sql);
                }
                catch (Exception e)
                {

                    Console.WriteLine(e.Message) ;
                }
            }
        }
        public static void printAreaSQL()
        {
            Console.WriteLine("获取地区数据");
            areaFormId = 0;
            cityFromId = 0;
            for (int i = 0; i < province_id.Length; i++)
            {
                string url = BASE_URL + province_id[i] + ".html";
                string html = getHtml(url);
                //暂停5秒等待获取网页数据
                try
                {
                    //匹配模式
                    List<string> ids = getSubUtil(html, RGEX_IDS);
                    List<string> codes = getSubUtil(html, RGEX_CODES);
                    for (int n = 0; n < ids.Count; n++)
                    {
                        cityFromId++;
                        //获取区级的数据
                        string urlQu = BASE_URL + province_id[i] + "/" + ids[n] + ".html";
                        string htmlQu = getHtml(urlQu);
                        //部分省市的区级不一样，没有跳转时间，所以正则不一样
                        string sql = getSqlstr(1, htmlQu, RGEX_NAMES_NO_A, RGEX_CODES_NO_A, codes[n]);
                        if (string.IsNullOrEmpty(sql))
                        {
                            sql = string.Format("{0}{1}", sql, getSqlstr(1, htmlQu, RGEX_NAMES, RGEX_CODES, codes[n]));
                        }
                        else
                        {
                            sql = string.Format("{0},{1}", sql, getSqlstr(1, htmlQu, RGEX_NAMES, RGEX_CODES, codes[n]));
                        }
                        sql = string.Format("insert into area values {0}", sql);
                        Console.WriteLine(sql);
                    }
                }
                catch (Exception e)
                {

                    Console.WriteLine(e.Message);
                }
            }
            Console.WriteLine("地区总数：" + areaFormId);
        }
        public static string getSqlstr(int type, string html, string regexNames, string regexCodes, string parentsCode)
        {
            string sql = "";
            int formId = 0;
            int parentsFromId = 0;
            List<string> names = getSubUtil(html, regexNames);
            List<string> codes = getSubUtil(html, regexCodes);
            for (int q = 0; q < names.Count; q++)
            {
                if (type==0)
                {
                    cityFromId++;
                    parentsFromId = provinceFromId;
                    formId = cityFromId;
                }
                else
                {
                    areaFormId++;
                    parentsFromId = cityFromId;
                    formId = cityFromId;
                }
                if (!string.IsNullOrEmpty(sql))
                {
                    sql = string.Format("{0},", sql);
                }
                //凭借插入数据
                sql = string.Format("{0}({1},{2},{3},{4},{5})", sql,formId,parentsFromId,codes[q],names[q],parentsCode);
            }
            return sql;
        }
        /// <summary>
        /// 通过url获取网页源码
        /// </summary>
        /// <param name="htmlUrl">地址</param>
        /// <returns></returns>
        public static string getHtml(string htmlUrl)
        {
            try
            {
                WebClient client = new WebClient();
                client.Credentials = CredentialCache.DefaultCredentials;//获取或设置请求的凭据
                Byte[] pageData = client.DownloadData(htmlUrl);//下载数据
                string pageHTML = System.Text.Encoding.GetEncoding("gb2312").GetString(pageData);
                return pageHTML;
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
                return ex.Message;
            }
        }

        /// <summary>
        /// 正则表达是匹配两个指定字符串中间的内容
        /// </summary>
        /// <param name="soap"></param>
        /// <param name="regex"></param>
        /// <returns></returns>
        public static List<string>getSubUtil(string soap,string regex)
        {
            List<string> list = new List<string>();
            //匹配的模式
            var matches = Regex.Matches(soap,regex);
            foreach (Match match in matches)
            {
                int i = 1;
                list.Add(match.Groups[i].Value);
                i++;
            }
            return list;
        } 
        /// <summary>
        /// 获取城市的json
        /// </summary>
        /// <returns></returns>
        public  static JArray getCitiesJson()
        {
            Console.WriteLine("获取CitiesJson");
            JArray jArray = new JArray();
            //城市id和区id计算全部的，方便存数据库
            int cityId = 0;
            int areaId = 0;
            int provinceId = 0;
            //第一 级为省市
            for (int p = 0; p < province_id.Length; p++)
            {
                try
                {
                    JObject province = new JObject();
                    provinceId = p + 1;
                    province.Add("pro_id", provinceId);
                    //注意补零
                    province.Add("pro_code", province_id[p] + "0000");
                    province.Add("pro_name", province_name[p]);
                    //第二级 城市
                    Console.WriteLine("开始获取" + province_name[p] + "的城市数据");
                    JArray cities = new JArray();
                    string url = BASE_URL + province_id[p] + ".html";
                    string html = getHtml(url);
                    List<string> ids = getSubUtil(html, RGEX_IDS);
                    List<string> names = getSubUtil(html, RGEX_NAMES);
                    List<string> codes = getSubUtil(html, RGEX_CODES);
                    for (int c = 0; c < ids.Count; c++)
                    {
                        int cityID = ++cityId;
                        JObject city = new JObject();
                        city.Add("city_id", cityID);
                        city.Add("city_code", codes[c]);
                        city.Add("city_name", names[c]);
                        //第三季 区域
                        JArray areas = new JArray();
                        string urlArea = BASE_URL + province_id[p] + "/" + ids[c] + ".html";
                        string htmlArea = getHtml(urlArea);
                        List<string> namesQu = new List<string>();
                        List<string> codesQu = new List<string>();
                        //有的市有“直辖区”，正则处理方式不一样\
                        foreach (var item in getSubUtil(htmlArea, RGEX_NAMES_NO_A))
                        {
                            namesQu.Add(item);
                        }
                        foreach (var item in getSubUtil(htmlArea, RGEX_CODES_NO_A))
                        {
                            codesQu.Add(item);
                        }
                        foreach (var item in getSubUtil(htmlArea, RGEX_NAMES))
                        {
                            namesQu.Add(item);
                        }
                        foreach (var item in getSubUtil(htmlArea, RGEX_CODES))
                        {
                            codesQu.Add(item);
                        }
                        for (int a = 0; a < namesQu.Count; a++)
                        {
                            int areaID = ++areaId;
                            JObject area = new JObject();
                            area.Add("area_id", areaID);
                            area.Add("area_code", codesQu[a]);
                            area.Add("area_name", namesQu[a]);
                            areas.Add(area);
                        }
                        city.Add("city_areas", areas);
                        cities.Add(city);
                    }
                    province.Add("pro_cities", cities);
                    jArray.Add(province);
                }
                catch (Exception e)
                {

                    Console.WriteLine(e.Message);
                }
            }
              //全部数据太长了，控制台打印不全，写入本地文件
        Console.WriteLine(jArray);
        writeFile(jArray.ToString());
        return jArray;
        }
        /// <summary>
        /// 将字段写入本地
        /// </summary>
        /// <param name="str"></param>
        public static void writeFile(string str)
        {

            string filepath = Environment.CurrentDirectory+"\\sql.txt";
            if (!File.Exists(filepath))
            {
                File.CreateText(filepath);
            }
            System.IO.StreamWriter file = new StreamWriter(filepath,true);
            file.WriteLine(str);
            

        }

        //public static byte[] read(input)
    }
}
