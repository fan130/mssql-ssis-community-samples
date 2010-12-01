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
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

// This demo creates a package with N forloops with dataflow inside executed sequentially
namespace ConsoleApplication1
{
    class EzMyDataFlow : EzDataFlow
    {
        public EzOleDbSource Source;
        // Transforms
        public EzLookup Lookup;
        public EzSortTransform SortMatch;
        public EzSortTransform SortNoMatch;
        // Destinations
        public EzFlatFileDestination MatchDest;
        public EzFlatFileDestination NoMatchDest;
        public EzFlatFileDestination ErrorDest;
        // Connection managers
        public EzSqlOleDbCM RefConn;
        public EzSqlOleDbCM SrcConn;
        public EzFlatFileCM MatchCM;
        public EzFlatFileCM NoMatchCM;
        public EzFlatFileCM ErrorCM;

        public EzMyDataFlow(EzContainer parent, TaskHost pipe) : base(parent, pipe) { }

        public EzMyDataFlow(EzContainer parent) : base(parent)
        {
            // Connection managers
            SrcConn = new EzSqlOleDbCM(Package, "SrcConn");
            SrcConn.SetConnectionString(Environment.MachineName, "AdventureWorks");
            MatchCM = new EzFlatFileCM(Package, "MatchCM");
            MatchCM.ConnectionString = "matchcm.txt";
            NoMatchCM = new EzFlatFileCM(Package, "NoMatchCM");
            NoMatchCM.ConnectionString = "nomatchcm.txt";
            ErrorCM = new EzFlatFileCM(Package, "ErrorCM");
            ErrorCM.ConnectionString = "errorcm.txt";
            RefConn = new EzSqlOleDbCM(Package, "RefConn");
            RefConn.SetConnectionString(Environment.MachineName, "AdventureWorks");

            // Creating Dataflow
            Source = new EzOleDbSource(this);
            Source.Connection = SrcConn;
            Source.SqlCommand = "select * from HumanResources.Employee";

            Lookup = new EzLookup(this);
            Lookup.AttachTo(Source);
            Lookup.OleDbConnection = RefConn;
            Lookup.SqlCommand = "select * from HumanResources.EmployeeAddress";
            Lookup.SetJoinCols("EmployeeID,EmployeeID");
            Lookup.SetPureCopyCols("AddressID");
            Lookup.NoMatchBehavor = NoMatchBehavior.SendToNoMatchOutput;
            Lookup.OutputCol("AddressID").TruncationRowDisposition = DTSRowDisposition.RD_RedirectRow;

            SortMatch = new EzSortTransform(this);
            SortMatch.AttachTo(Lookup, 0, 0);
            SortMatch.SortOrder["EmployeeID"] = 1;      // sort in ascending order
            SortMatch.SortOrder["AddressID"] = -2;     // sort in descending order

            SortNoMatch = new EzSortTransform(this);
            SortNoMatch.AttachTo(Lookup, 1, 0);
            SortNoMatch.SortOrder["EmployeeID"] = 1;      // sort in ascending order

            ErrorDest = new EzFlatFileDestination(this);
            ErrorDest.AttachTo(Lookup, 2, 0);
            ErrorDest.Connection = ErrorCM;
            ErrorDest.DefineColumnsInCM();      // configure connection manager to have all input columns defined in the resulting file

            MatchDest = new EzFlatFileDestination(this);
            MatchDest.AttachTo(SortMatch);
            MatchDest.Connection = MatchCM;
            MatchDest.DefineColumnsInCM();

            NoMatchDest = new EzFlatFileDestination(this);
            NoMatchDest.AttachTo(SortNoMatch);
            NoMatchDest.Connection = NoMatchCM;
            NoMatchDest.DefineColumnsInCM();
        }
    }

    public class EzMyLoopPkg : EzPackage
    {
        EzExecForLoop<EzMyDataFlow>[] Loops;
        public EzMyLoopPkg(int numLoops) : base()
        {
            Variables.Add("i", false, "", 0);
            Loops = new EzExecForLoop<EzMyDataFlow>[numLoops];
            for (int i = 0; i < numLoops; i++)
            {
                Loops[i] = new EzExecForLoop<EzMyDataFlow>(this);
                Loops[i].InitExpression = "@i=0";
                Loops[i].EvalExpression = "@i<10";
                Loops[i].AssignExpression = "@i=@i+1";
            }
            for (int i = 1; i < numLoops; i++)
                Loops[i].AttachTo(Loops[i-1]);
        }

        public static void Main(string[] args)
        {
            EzMyLoopPkg p = new EzMyLoopPkg(5);
            p.SaveToFile("demo7.dtsx");
            p.Execute();
            Console.Write(string.Format("Package executed with result {0}\n", p.ExecutionResult));
        }
    }
}
