// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.SSIS.EzAPI;

namespace UnionAll
{
    class UnionAll
    {
        static void Main(string[] args)
        {
            // Creating the package and data flow task:
            EzPackage package = new EzPackage();
            EzDataFlow dataFlow = new EzDataFlow(package);

            // Creating the first source:
            EzSqlOleDbCM srcConn1 = Activator.CreateInstance(typeof(EzSqlOleDbCM), new object[] { package }) as EzSqlOleDbCM;
            EzOleDbSource source1 = Activator.CreateInstance(typeof(EzOleDbSource), new object[] { dataFlow }) as EzOleDbSource;
            source1.Connection = srcConn1;
            srcConn1.SetConnectionString("localhost", "Northwind");
            source1.Table = "Customers";

            // Creating the second source, which has the same columns as the first source:
            EzSqlOleDbCM srcConn2 = Activator.CreateInstance(typeof(EzSqlOleDbCM), new object[] { package }) as EzSqlOleDbCM;
            EzOleDbSource source2 = Activator.CreateInstance(typeof(EzOleDbSource), new object[] { dataFlow }) as EzOleDbSource;
            source2.Connection = srcConn2;
            srcConn2.SetConnectionString("localhost", "Northwind");
            source2.Table = "Customers3";

            // Creating the third source, which contains different columns to demonstrate how EzUnionAll handles this scenario:
            EzSqlOleDbCM srcConn3 = Activator.CreateInstance(typeof(EzSqlOleDbCM), new object[] { package }) as EzSqlOleDbCM;
            EzOleDbSource source3 = Activator.CreateInstance(typeof(EzOleDbSource), new object[] { dataFlow }) as EzOleDbSource;
            source3.Connection = srcConn3;
            srcConn3.SetConnectionString("localhost", "Northwind");
            source3.Table = "FuzzyOrders";

            /* Creating EzUnionAll and attaching it to the sources.
             * 
             * The input columns of the first input attached define the output columns.
             * 
             * The second input has the same output columns as the first input, so the input columns
             * of the second input are mapped to the corresponding output columns.
             * 
             * The third input shares two of the first input's columns, so these columns are automatically mapped together.
             * The rest of the third input's columns do not match any of the existing output columns, so they are not mapped.
             * No value is inserted into the unmapped output columns for rows that originate from the third input.
             */
            EzUnionAll union = new EzUnionAll(dataFlow);
            union.AttachTo(source1, 0, 0);
            union.AttachTo(source2, 0, 1);
            union.AttachTo(source3, 0, 2);

            // Creating a Flat File Destination:
            EzFlatFileCM output_conn = Activator.CreateInstance(typeof(EzFlatFileCM), new object[] { package }) as EzFlatFileCM;
            EzFlatFileDestination output = Activator.CreateInstance(typeof(EzFlatFileDestination), new object[] { dataFlow }) as EzFlatFileDestination;
            output.Connection = output_conn;
            output_conn.ConnectionString = @"C:\Union All\UnionAllSample.txt";
            output.Overwrite = true;

            // Attaching the Union All component to the destination:
            output.AttachTo(union, 0, 0);
            output.DefineColumnsInCM();

            // Executing the package:
            package.Execute();
        }
    }
}
