// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Microsoft.SqlServer.SSIS.EzAPI;
using Microsoft.SqlServer.Dts.Runtime;

namespace ConsoleApplication1
{
    // DEMO3: Simple package with transform. Use EzTransformPackage for this
    class EzMyPackage<EzTransform> : EzTransformPackage<EzOleDbSource, EzSqlOleDbCM, EzTransform,
        EzFlatFileDestination, EzFlatFileCM> where EzTransform: EzComponent
    {
        // These two methods are not deployed as I am not going to assign SSIS package to EzSortPackage in demo
        public EzMyPackage(Package p) : base(p) { }
        public static implicit operator EzMyPackage<EzTransform>(Package p) { return new EzMyPackage<EzTransform>(p); }

        public EzMyPackage(string srv, string db, string sql, string file) : base()
        {
            SrcConn.SetConnectionString(srv, db);
            Source.SqlCommand = sql;
            DestConn.ConnectionString = file;
            Dest.Overwrite = true;
            Dest.DefineColumnsInCM();
        }

    }
    public class Demo3
    {
        public static void Main(string[] args)
        {
            // DEMO 3
            EzMyPackage<EzSortTransform> p3 = new EzMyPackage<EzSortTransform>("localhost", "AdventureWorks", 
                "select * from Person.Address", "result1.txt");
            p3.Transform.EliminateDuplicates = true;
            p3.Transform.SortOrder["AddressID"] = -1; // sort in descending order
            p3.SaveToFile("demo3.dtsx");
            p3.Execute();
            Console.Write(string.Format("Package3 executed with result {0}\n", p3.ExecutionResult));
            // Assign SSIS package to EzPackage
            p3 = new Application().LoadPackage("demo3.dtsx", null);
            p3.Execute();
            Console.Write(string.Format("Package3 executed with result {0}\n", p3.ExecutionResult));
        }
    }
}