using System;
using System.IO;
using FirebirdSql.Data.FirebirdClient;

namespace FbExternalEmbedded
{
    class Program
    {
        static void Main(string[] args)
        {
            var dbPath = Path.GetFullPath("MyFooDatabase.fdb");

            //go up to solution dir
            //var fbClientDll = Path.GetFullPath("..\\..\\..\\..\\Firebird-4.0.0.2496-1-x64\\fbclient.dll");
            //var conString = GetEmbeddedConnectionString(fbClientDll, dbPath);

            var conString = GetNormalConnectionString(dbPath);

            FbConnection.CreateDatabase(conString, overwrite: true);

            using var fbConn = new FbConnection(conString);
            fbConn.Open();

            //test the connection
            DoStuff(conString);

            using var fbCommand = fbConn.CreateCommand();
            fbCommand.CommandText = @"
create function my_HelloWorldAppender(input varchar(20)) returns varchar(60)
external name 'MyFooAssembly!MyFooAssembly.MyFooFunctions.HelloWorldAppender'
engine FbNetExternalEngine;
";

            fbCommand.ExecuteNonQuery();

            fbCommand.CommandText = "select my_HelloWorldAppender(item1) from foo;";
            using var fbReader = fbCommand.ExecuteReader();
            while (fbReader.Read()) 
                Console.WriteLine(fbReader.GetString(0));

            Console.WriteLine("Done!");
            Console.ReadLine();
        }

        private static void DoStuff(string conString)
        {
            using var fbConn = new FbConnection(conString);
            fbConn.Open();

            using var fbCommand = fbConn.CreateCommand();
            fbCommand.CommandText = "create table foo(item1 varchar(20));";
            fbCommand.ExecuteNonQuery();

            fbCommand.CommandText = "insert into foo values ('abc');";
            fbCommand.ExecuteNonQuery();

            fbCommand.CommandText = "select * from foo;";
            using var fbReader = fbCommand.ExecuteReader();
            while (fbReader.Read()) 
                Console.WriteLine(fbReader.GetString(0));
        }

        public static string GetEmbeddedConnectionString(string fbPath, string dbPath)
        {
            var stringBuilder = new FbConnectionStringBuilder
            {
                Database = dbPath,
                ClientLibrary = fbPath,
                ServerType = FbServerType.Embedded,
                UserID = "sysdba",
                Password = "sysdba",
            };

            return stringBuilder.ToString();
        }
        public static string GetNormalConnectionString(string dbPath)
        {
            var stringBuilder = new FbConnectionStringBuilder
            {
                Database = dbPath,
                DataSource = "localhost",
                Port = 3052,
                UserID = "sysdba",
                Password = "masterkey",
            };

            return stringBuilder.ToString();
        }
    }
}
