using System;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Collections.Generic;

namespace AdministrativeRegion
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //Console.WriteLine(ChinaCityNameUtils.CREAT_PROVIINCE_SQL);
            //Console.WriteLine(ChinaCityNameUtils.CREAT_CITY_SQL);
            //Console.WriteLine(ChinaCityNameUtils.CREAT_AREA_SQL);
            //ChinaCityNameUtils.printProvinceSQL();
            //ChinaCityNameUtils.printCitySql();
            //ChinaCityNameUtils.printAreaSQL();

            //ConnectionDatabase();

            var waits = new List<EventWaitHandle>();
            for (int i = 0; i < 10; i++)
            {
                var handler = new ManualResetEvent(false);
                waits.Add(handler);
                new Thread(new ParameterizedThreadStart(Print))
                {
                    Name = "thread" + i.ToString()
                }.Start(new Tuple<string, EventWaitHandle>("test print:" + i, handler));
            }
            WaitHandle.WaitAll(waits.ToArray());
            Console.WriteLine("Completed!");
            Console.Read();





            Console.ReadKey();
        }
  

            private static void Print(object param)
            {
                var p = (Tuple<string, EventWaitHandle>)param;
                Console.WriteLine(Thread.CurrentThread.Name + ": Begin!");
                Console.WriteLine(Thread.CurrentThread.Name + ": Print" + p.Item1);
                Thread.Sleep(300);
                Console.WriteLine(Thread.CurrentThread.Name + ": End!");
                p.Item2.Set();
            }

       







        public static void ConnectionDatabase()
        {
            try
            {
                string sqlcon = "Server = 192.168.0.219;Database=Product;User Id=sa;Password=Rep@2014/p;Trusted_Connection =False";
                SqlConnection con = new SqlConnection(sqlcon);
                con.Open();            
                SqlCommand command = new SqlCommand("HLWGetSerialNo", con);
                command.CommandType =CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@sCode", SqlDbType.VarChar, 50));
                command.Parameters.Add(new SqlParameter("@result",SqlDbType.VarChar,50));
                command.Parameters["@sCode"].Value = "ZX";
                command.Parameters["@result"].Direction = ParameterDirection.Output;
                command.ExecuteNonQuery();
                Console.WriteLine(command.Parameters["@result"].Value);
                con.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
    
}
