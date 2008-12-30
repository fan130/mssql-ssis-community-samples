using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Microsoft.SqlServer.SSIS.EzAPI;
using Microsoft.SqlServer.Dts.Runtime;

namespace ConsoleApplication1
{
    public class EzLookupCachePkg : EzPkgWithExec<EzTransformDF<EzOleDbSource, EzSqlOleDbCM, EzCacheTransform>>
    {
        public EzTransDestConnDF<EzOleDbSource, EzSqlOleDbCM, EzLookup, EzFlatFileDestination, EzFlatFileCM> LookupDF;
        public EzLookupCachePkg(string srv, string srcDb, string refDb, string refSql) : base()
        {
            LookupDF = new EzTransDestConnDF<EzOleDbSource, EzSqlOleDbCM, EzLookup, EzFlatFileDestination, EzFlatFileCM>(this);
            LookupDF.AttachTo(Exec);
            Exec.Transform.Connection = new EzCacheCM(this);
            LookupDF.Transform.CacheConnection = Exec.Transform.Connection;
            Exec.Transform.Connection.Name = "Cache";
            Variables.Add("CheckSum0", false, "", new byte[] { });
            Exec.Name = "CacheDF";
            Exec.SrcConn.Name = "RefDb";
            Exec.SrcConn.SetConnectionString(srv, refDb);
            Exec.Source.SqlCommand = refSql;
            Exec.Transform.ProvideInputToCache();
            LookupDF.SrcConn.Name = "SrcDb";
            LookupDF.SrcConn.SetConnectionString(srv, srcDb);
            LookupDF.Transform.Meta.OutputCollection[0].ErrorRowDisposition = Microsoft.SqlServer.Dts.Pipeline.Wrapper.DTSRowDisposition.RD_IgnoreFailure;
            LookupDF.Name = "LookupDF";
            LookupDF.DestConn.ConnectionString = "demo2.txt";
        }
        public EzLookupCachePkg(Package p) : base(p) { }   
        public static implicit operator EzLookupCachePkg(Package p) { return new EzLookupCachePkg(p); }

        // Field Names in this sample are not very good because we use templates a lot here
        [STAThread]
        static void Main(string[] args)
        {
            EzLookupCachePkg p5 = new EzLookupCachePkg(Environment.MachineName, "AdventureWorks", "AdventureWorks", 
                "select * from HumanResources.EmployeeAddress");
            p5.Exec.Transform.Connection.SetIndexCols("EmployeeID", "AddressID");
            p5.LookupDF.Source.SqlCommand = "select * from HumanResources.Employee";
            p5.LookupDF.Transform.SetJoinCols("EmployeeID,EmployeeID");
            p5.LookupDF.Transform.SetPureCopyCols("AddressID");
            p5.LookupDF.Dest.DefineColumnsInCM();
            p5.Exec.Disable = true;
            p5.LookupDF.Transform.OleDbConnection = p5.Exec.SrcConn;
            p5.LookupDF.Transform.SqlCommand = p5.Exec.Source.SqlCommand;
            p5.SaveToFile("demo5.dtsx");
            p5.Execute();
            Console.Write(string.Format("Package6 executed with result {0}\n", p5.ExecutionResult));
        }

        private static string ArrayToString<T>(T[] arr)
        {
            if (arr == null)
                return "null";
            string res = string.Empty;
            foreach (T el in arr)
                res += el.ToString() + ",";
            return res;
        }
    }
}
