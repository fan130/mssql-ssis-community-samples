// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.SSIS.EzAPI;
using Microsoft.SqlServer.Dts.Runtime;

namespace ConsoleApplication1
{
    // DEMO2: Simple data transfer from source to destination. Use EzSrcDestPackage template for this
    public class EzOleDbToFilePackage : EzSrcDestPackage<EzOleDbSource, EzSqlOleDbCM,
        EzFlatFileDestination, EzFlatFileCM>
    {
        public EzOleDbToFilePackage(Package p) : base(p) { }
        public static implicit operator EzOleDbToFilePackage(Package p) { return new EzOleDbToFilePackage(p); }

        public EzOleDbToFilePackage(string srv, string db, string table, string file)
            : base()
        {
            SrcConn.SetConnectionString(srv, db);
            Source.Table = table;
            DestConn.ConnectionString = file;
            Dest.Overwrite = true;
            // This method defines the columns in FlatFile connection manager which have the same
            // datatypes as flat file destination
            Dest.DefineColumnsInCM();
        }

        [STAThread]
        static void Main(string[] args)
        {
            // DEMO 2
            EzOleDbToFilePackage p2 = new EzOleDbToFilePackage("localhost", "AdventureWorks", "Address", "result.txt");
            p2.DataFlow.Disable = true;
            p2.Execute();
            Console.Write(string.Format("Package2 executed with result {0}\n", p2.ExecutionResult));
        }
    }
}