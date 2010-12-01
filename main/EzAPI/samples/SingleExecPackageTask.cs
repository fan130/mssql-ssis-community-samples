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
    // DEMO1: This package contains a single Execute package task
    public class EzExecPkgPackage : EzPackage
    {
        // Provide this constructor only if you want to overload Assignment operator
        public EzExecPkgPackage(Package p) : base(p) { }   // this constructor MUST BE present

        // Provide assignment operator if you want to be able to Assign SSIS Package to EzPackage
        public static implicit operator EzExecPkgPackage(Package p) { return new EzExecPkgPackage(p); }

        // All the tasks, components and connection managers which should be linked to the corresponding
        // SSIS package objects MUST BE PUBLIC MEMBERS if you want to be able to assign SSIS package to EzPackage
        public EzExecPackage ExecPkg;
        public EzFileCM PkgCM;

        public EzExecPkgPackage(string pkgName)
            : base()
        {
            PkgCM = new EzFileCM(this);
            PkgCM.ConnectionString = pkgName;
            PkgCM.Name = "PackageConnection";
            ExecPkg = new EzExecPackage(this);
            ExecPkg.Name = "ExecutePackage";
            ExecPkg.Connection = PkgCM;
        }

        [STAThread]
        static void Main(string[] args)
        {
            // DEMO 1
            EzPackage p = new EzPackage();
            p.SaveToFile("testpkg.dtsx");
            EzExecPkgPackage p1 = new EzExecPkgPackage("testpkg.dtsx");
            p1.SaveToFile("demo1.dtsx");
            p1.Execute();
            Console.Write(string.Format("Package1 executed with result {0}\n", p1.ExecutionResult));
        }
    }
}