using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Microsoft.SqlServer.SSIS.EzAPI;
using Microsoft.SqlServer.Dts.Runtime;

namespace ConsoleApplication1
{
    // DEMO4: Simple package with transform executed in a loop container. Use EzLoopTransformPackage for this
    public class EzLoopSortPackage : EzLoopTransformPackage<EzOleDbSource, EzSqlOleDbCM, EzSortTransform,
        EzFlatFileDestination, EzFlatFileCM>
    {
        public EzLoopSortPackage(Package p) : base(p) { }
        public static implicit operator EzLoopSortPackage(Package p) { return new EzLoopSortPackage(p); }

        public EzLoopSortPackage(string srv, string db, string table, string file)
            : base()
        {
            SrcConn.SetConnectionString(srv, db);
            Source.Table = table;
            Source.AccessMode = AccessMode.AM_OPENROWSET;
            DestConn.ConnectionString = file;
            Dest.Overwrite = true;
            Dest.DefineColumnsInCM();
        }

        [STAThread]
        static void Main(string[] args)
        {
            // DEMO 4
            EzLoopSortPackage p5 = new EzLoopSortPackage("localhost", "AdventureWorks", "Person.Address", "result1.txt");
            p5.Transform.MaxThreads = -1; // Do not limit number of threads
            p5.Transform.EliminateDuplicates = true;
            p5.Transform.SortOrder["AddressID"] = -1; // sort in descending order
            p5.Variables.Add("LoopCounter", false, "User", 0);
            p5.ForLoop.InitExpression = "@[User::LoopCounter] = 0";
            p5.ForLoop.AssignExpression = "@[User::LoopCounter] = @[User::LoopCounter] + 1";
            p5.ForLoop.EvalExpression = "@[User::LoopCounter] < 3";
            p5.Execute();
            Console.Write(string.Format("Package5 executed with result {0}\n", p5.ExecutionResult));
        }
    }
}