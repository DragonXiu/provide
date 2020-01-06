# provide
获取省市区数据信息
思路是： 
   (1)通过统计局网站上找到的各个省市区的ID可以拼接出相关界面的URL 
   (2)通过网络请求可以通过URL获取到当前界面的源码,通过正则表达式截取需要的数据，包括 名称、统计用区划代码、跳转下级界面ID 将获取到的数据拼接成SQL语句，或者拼接为JSON字符串输出 
   工具类ChinaCityNameUtils有四个方法： printProvinceSQL()/printCitySQL ()/ printAreaSQL()/分别在控制台打印 表province/city/area的插入SQL 语句getCitiesJson在控制台打印json字符串，并保持到D盘注意，sql语句是根据我用的数据库的格式拼接的，如果使用的数据库格式和我使用的不通，可以自行修改对应的SQL语句
