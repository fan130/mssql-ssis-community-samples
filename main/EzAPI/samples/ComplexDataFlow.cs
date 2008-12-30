using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Microsoft.SqlServer.SSIS.EzAPI;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace ConsoleApplication1
{
    // For EzAPI public fields means that if we assign SSIS Package to EzPackage this field needs
    // to be assigned to the corresponding object inside SSIS package. The corresponding object is an object that has the same EzName as the field name
    // in the class. If you set it to some incorrect value - package logic won't be affected as internally it stores all the Ez components, tasks and connections
    // and uses that internal list to refresh metadata.
    class EzMyPackage : EzDataFlowPackage
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

        // Provide this constructor only if you want to overload Assignment operator
        public EzMyPackage(Package p) : base(p) { }  
        // Provide assignment operator if you want to be able to Assign SSIS Package to EzPackage
        public static implicit operator EzMyPackage(Package p) { return new EzMyPackage(p); }

        public EzMyPackage() : base()
        {
            // Connection managers
            SrcConn = new EzSqlOleDbCM(this);
            SrcConn.SetConnectionString(Environment.MachineName, "AdventureWorks");
            MatchCM = new EzFlatFileCM(this);
            MatchCM.ConnectionString = "matchcm.txt";
            NoMatchCM = new EzFlatFileCM(this);
            NoMatchCM.ConnectionString = "nomatchcm.txt";
            ErrorCM = new EzFlatFileCM(this);
            ErrorCM.ConnectionString = "errorcm.txt";
            RefConn = new EzSqlOleDbCM(this);
            RefConn.SetConnectionString(Environment.MachineName, "AdventureWorks");

            // Creating Dataflow
            Source = new EzOleDbSource(DataFlow);
            Source.Connection = SrcConn;
            Source.SqlCommand = "select * from HumanResources.Employee";
            
            Lookup = new EzLookup(DataFlow);
            Lookup.AttachTo(Source);
            Lookup.OleDbConnection = RefConn;
            Lookup.SqlCommand = "select * from HumanResources.EmployeeAddress";
            Lookup.SetJoinCols("EmployeeID,EmployeeID");
            Lookup.SetPureCopyCols("AddressID");
            Lookup.NoMatchBehavor = NoMatchBehavior.SendToNoMatchOutput;
            Lookup.OutputCol("AddressID").TruncationRowDisposition = DTSRowDisposition.RD_RedirectRow;

            SortMatch = new EzSortTransform(DataFlow);
            SortMatch.AttachTo(Lookup, 0, 0);
            SortMatch.SortOrder["EmployeeID"] = 1;      // sort in ascending order
            SortMatch.SortOrder["AddressID"] = -2;      // sort in descending order

            SortNoMatch = new EzSortTransform(DataFlow);
            SortNoMatch.AttachTo(Lookup, 1, 0);
            SortNoMatch.SortOrder["EmployeeID"] = 1;      // sort in ascending order

            ErrorDest = new EzFlatFileDestination(DataFlow);
            ErrorDest.AttachTo(Lookup, 2, 0);
            ErrorDest.Connection = ErrorCM;
            ErrorDest.DefineColumnsInCM();      // configure connection manager to have all input columns defined in the resulting file

            MatchDest = new EzFlatFileDestination(DataFlow);
            MatchDest.AttachTo(SortMatch);
            MatchDest.Connection = MatchCM;
            MatchDest.DefineColumnsInCM();

            NoMatchDest = new EzFlatFileDestination(DataFlow);
            NoMatchDest.AttachTo(SortNoMatch);
            NoMatchDest.Connection = NoMatchCM;
            NoMatchDest.DefineColumnsInCM();
        }

        [STAThread]
        static void Main(string[] args)
        {
            EzMyPackage p = new EzMyPackage();
            p.Execute();
            Console.Write(string.Format("Package executed with result {0}\n", p.ExecutionResult));
        }
    }
}