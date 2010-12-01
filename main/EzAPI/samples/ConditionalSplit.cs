// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.SSIS.EzAPI;

namespace ConditionalSplit
{
    class ConditionalSplit
    {
        static void Main(string[] args)
        {
            // Creating the package and the Data Flow Task:
            EzPackage package = new EzPackage();
            EzDataFlow dataFlow = new EzDataFlow(package);

            // Creating a connection to the database:
            EzSqlOleDbCM srcConn = Activator.CreateInstance(typeof(EzSqlOleDbCM), new object[] { package }) as EzSqlOleDbCM;
            EzOleDbSource source = Activator.CreateInstance(typeof(EzOleDbSource), new object[] { dataFlow }) as EzOleDbSource;
            source.Connection = srcConn;
            srcConn.SetConnectionString("localhost", "Northwind");
            source.Table = "FuzzyOrders";

            // Creating six Flat File Destinations to write the output from the Conditional Split:
            uint numberCases = 6;
            List<EzFlatFileDestination> destinations = new List<EzFlatFileDestination>();
            for (int i = 1; i <= numberCases; i++)
            {
                EzFlatFileCM conn = Activator.CreateInstance(typeof(EzFlatFileCM), new object[] { package }) as EzFlatFileCM;
                EzFlatFileDestination dest = Activator.CreateInstance(typeof(EzFlatFileDestination), new object[] { dataFlow }) as EzFlatFileDestination;
                dest.Connection = conn;
                conn.ConnectionString = @"C:\CondSplit\output" + i + ".txt";
                dest.Overwrite = true;

                destinations.Add(dest);
            }

            // Attaching the Conditional Split to the source, and mapping input columns to output columns:
            EzConditionalSplit split = new EzConditionalSplit(dataFlow);
            split.AttachTo(source);
            split.LinkAllInputsToOutputs();

            /*
             * Important step - this is where the new EzConditionalSplit functionality comes in.
             * 
             * The Condition property of EzConditionalSplit indexes over outputs using output names.
             * Providing an output name that does not yet exist will create a new output.
             * 
             * The Condition property is set to a string that represents the condition that is evaluated to determine which
             * output a particular row is sent to.
             * 
             * The set method of the Condition property also assigns a string to the Expression property, which contains the actual string
             * that is evaulated during runtime.  In this string, column names are replaced with column IDs.  The Expression property
             * can be set directly, but the Condition property allows the user to provide a string containing column names without
             * worrying about IDs.  In addition, the Condition set method also assigns the new expression to be the next case to be evaluated,
             * automatically setting the Order property, which must be sequential in order to guarantee execution.
             */
            for (int i = 1; i <= numberCases; i++)
            {
                split.Condition["case" + i] = "EmployeeID == " + i;
            }

            // Attaching each destination to an output of Conditional Split:
            for (int i = 1; i <= numberCases; i++)
            {
                destinations[i - 1].AttachTo(split, i + 1, 0);
                destinations[i - 1].DefineColumnsInCM();
            }

            // Creating a destination for the default output:
            EzFlatFileCM conn_default = Activator.CreateInstance(typeof(EzFlatFileCM), new object[] { package }) as EzFlatFileCM;
            EzFlatFileDestination dest_default = Activator.CreateInstance(typeof(EzFlatFileDestination), new object[] { dataFlow }) as EzFlatFileDestination;
            dest_default.Connection = conn_default;
            conn_default.ConnectionString = @"C:\CondSplit\default.txt";
            dest_default.Overwrite = true;

            // Attaching the default destination to the default output.
            dest_default.AttachTo(split, 0, 0);
            dest_default.DefineColumnsInCM();

            // Execute the package.
            package.Execute();
        }
    }
}
