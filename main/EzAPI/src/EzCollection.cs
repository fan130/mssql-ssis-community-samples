// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)


using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.SqlServer.Dts.Runtime;
using RunWrap = Microsoft.SqlServer.Dts.Runtime.Wrapper;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Tasks.ExecutePackageTask;
using System.Reflection;
using System.IO;
using System.Globalization;

namespace Microsoft.SqlServer.SSIS.EzAPI
{
    public class EzDataFlowPackage : EzPackage
    {
        public EzDataFlow DataFlow;

        public EzDataFlowPackage() : base() { DataFlow = new EzDataFlow(this); }
        public EzDataFlowPackage(Package p) : base(p) { }

        public static implicit operator EzDataFlowPackage(Package p) { return new EzDataFlowPackage(p); }
    }

    public class EzForLoopPackage : EzPackage
    {
        public EzForLoop ForLoop;
        public EzForLoopPackage()
            : base()
        {
            ForLoop = new EzForLoop(this);
        }

        public EzForLoopPackage(Package p) : base(p) { }
        public static implicit operator EzForLoopPackage(Package p) { return new EzForLoopPackage(p); }
    }

    public class EzForLoopDFPackage : EzForLoopPackage
    {
        public EzDataFlow DataFlow;
        public EzForLoopDFPackage()
            : base()
        {
            DataFlow = new EzDataFlow(ForLoop);
        }

        public EzForLoopDFPackage(Package p) : base(p) { }
        public static implicit operator EzForLoopDFPackage(Package p) { return new EzForLoopDFPackage(p); }
    }

    public class EzSrcPackage<SrcType, SrcConnType> : EzDataFlowPackage
        where SrcType : EzAdapter
        where SrcConnType : EzConnectionManager
    {
        public SrcConnType SrcConn;
        public SrcType Source;

        public EzSrcPackage()
            : base()
        {
            SrcConn = Activator.CreateInstance(typeof(SrcConnType), new object[] { this }) as SrcConnType;
            Source = Activator.CreateInstance(typeof(SrcType), new object[] { DataFlow }) as SrcType;
            Source.Connection = SrcConn;
        }

        public EzSrcPackage(Package p) : base(p) { }

        public static implicit operator EzSrcPackage<SrcType, SrcConnType>(Package p)
        {
            return new EzSrcPackage<SrcType, SrcConnType>(p);
        }
    }

    public class EzSrcDestMultiStreamPackage<SrcType, SrcConnType, DestType, DestConnType> : EzDataFlowPackage
        where SrcType : EzAdapter
        where SrcConnType : EzConnectionManager
        where DestType : EzAdapter
        where DestConnType : EzConnectionManager
    {
        public DestConnType DestConn;
        public DestType Dest;
        public SrcConnType SourceConn;
        public SrcType Source;
        public List<KeyValuePair<EzAdapter, EzAdapter>> SourceDestPairs = new List<KeyValuePair<EzAdapter, EzAdapter>>();

        public EzSrcDestMultiStreamPackage(int streamCount)
            : base()
        {
            SourceConn = Activator.CreateInstance(typeof(SrcConnType), new object[] { this }) as SrcConnType;
            DestConn = Activator.CreateInstance(typeof(DestConnType), new object[] { this }) as DestConnType;

            for (int i = 0; i < streamCount; i++)
            {
                Source = Activator.CreateInstance(typeof(SrcType), new object[] { DataFlow }) as SrcType;
                Dest = Activator.CreateInstance(typeof(DestType), new object[] { DataFlow }) as DestType;

                KeyValuePair<EzAdapter, EzAdapter> connectionPair = new KeyValuePair<EzAdapter, EzAdapter>(Source, Dest);
                SourceDestPairs.Add(connectionPair);
            }
        }

        public void AttachConnections(SrcConnType sourceConnection, DestConnType destConnection)
        {
            foreach (KeyValuePair<EzAdapter, EzAdapter> connectionPair in SourceDestPairs)
            {
                SrcType source = (SrcType)connectionPair.Key;
                DestType dest = (DestType)connectionPair.Value;

                source.Connection = sourceConnection;
                dest.Connection = destConnection;

                dest.AttachTo(source);
            }
        }

        public EzSrcDestMultiStreamPackage(Package p) : base(p) { }

        public static implicit operator EzSrcDestMultiStreamPackage<SrcType, SrcConnType, DestType, DestConnType>(Package p)
        {
            return new EzSrcDestMultiStreamPackage<SrcType, SrcConnType, DestType, DestConnType>(p);
        }
    }

    public class EzSrcDestPackage<SrcType, SrcConnType, DestType, DestConnType> : EzSrcPackage<SrcType, SrcConnType>
        where SrcType : EzAdapter
        where SrcConnType : EzConnectionManager
        where DestType : EzAdapter
        where DestConnType : EzConnectionManager
    {
        public DestConnType DestConn;
        public DestType Dest;

        public EzSrcDestPackage()
            : base()
        {
            DestConn = Activator.CreateInstance(typeof(DestConnType), new object[] { this }) as DestConnType;
            Dest = Activator.CreateInstance(typeof(DestType), new object[] { DataFlow }) as DestType;
            Dest.Connection = DestConn;
            Dest.AttachTo(Source);
        }

        public EzSrcDestPackage(Package p) : base(p) { }

        public static implicit operator EzSrcDestPackage<SrcType, SrcConnType, DestType, DestConnType>(Package p)
        {
            return new EzSrcDestPackage<SrcType, SrcConnType, DestType, DestConnType>(p);
        }
    }

    public class EzTransformPackage<SrcType, SrcConnType, TransType, DestType, DestConnType>
        : EzSrcPackage<SrcType, SrcConnType>
        where SrcType : EzAdapter
        where SrcConnType : EzConnectionManager
        where TransType : EzComponent
        where DestType : EzAdapter
        where DestConnType : EzConnectionManager
    {
        public DestConnType DestConn;
        public DestType Dest;
        public TransType Transform;

        public EzTransformPackage()
            : base()
        {
            Transform = Activator.CreateInstance(typeof(TransType), new object[] { DataFlow }) as TransType;
            DestConn = Activator.CreateInstance(typeof(DestConnType), new object[] { this }) as DestConnType;
            Dest = Activator.CreateInstance(typeof(DestType), new object[] { DataFlow }) as DestType;
            Dest.Connection = DestConn;
            Transform.AttachTo(Source);
            Dest.AttachTo(Transform);
        }

        public EzTransformPackage(Package p) : base(p) { }

        public static implicit operator EzTransformPackage<SrcType, SrcConnType, TransType, DestType, DestConnType>(Package p)
        {
            return new EzTransformPackage<SrcType, SrcConnType, TransType, DestType, DestConnType>(p);
        }
    }

    public class EzLoopTransformPackage<SrcType, SrcConnType, TransType, DestType, DestConnType>
        : EzForLoopDFPackage
        where SrcType : EzAdapter
        where SrcConnType : EzConnectionManager
        where TransType : EzComponent
        where DestType : EzAdapter
        where DestConnType : EzConnectionManager
    {
        public SrcType Source;
        public SrcConnType SrcConn;
        public DestConnType DestConn;
        public DestType Dest;
        public TransType Transform;

        public EzLoopTransformPackage()
            : base()
        {
            SrcConn = Activator.CreateInstance(typeof(SrcConnType), new object[] { this }) as SrcConnType;
            Source = Activator.CreateInstance(typeof(SrcType), new object[] { DataFlow }) as SrcType;
            Source.Connection = SrcConn;
            Transform = Activator.CreateInstance(typeof(TransType), new object[] { DataFlow }) as TransType;
            DestConn = Activator.CreateInstance(typeof(DestConnType), new object[] { this }) as DestConnType;
            Dest = Activator.CreateInstance(typeof(DestType), new object[] { DataFlow }) as DestType;
            Dest.Connection = DestConn;
            Transform.AttachTo(Source);
            Dest.AttachTo(Transform);
        }

        public EzLoopTransformPackage(Package p) : base(p) { }

        public static implicit operator EzLoopTransformPackage<SrcType, SrcConnType, TransType, DestType, DestConnType>(Package p)
        {
            return new EzLoopTransformPackage<SrcType, SrcConnType, TransType, DestType, DestConnType>(p);
        }
    }

    public class EzPkgWithExec<T> : EzPackage where T : EzExecutable
    {
        public T Exec;
        public EzPkgWithExec() : base()
        {
            Exec = Activator.CreateInstance(typeof(T), new object[] { this }) as T;
        }
        public EzPkgWithExec(Package p) : base(p) { }
        public static implicit operator EzPkgWithExec<T>(Package p) { return new EzPkgWithExec<T>(p); }
    }

    public class EzExecForLoop<T> : EzForLoop where T : EzExecutable
    {
        public T Exec;
        public EzExecForLoop(EzPackage pkg, DtsContainer c) : base(pkg, c) { }
        public EzExecForLoop(EzContainer parent)
            : base(parent)
        {
            Exec = Activator.CreateInstance(typeof(T), new object[] { this }) as T;
        }
    }

    public class EzSrcDF<SrcComp> : EzDataFlow where SrcComp : EzComponent
    {
        public SrcComp Source;
        public EzSrcDF(EzContainer parent)
            : base(parent)
        {
            Source = Activator.CreateInstance(typeof(SrcComp), new object[] { this }) as SrcComp;
        }
        public EzSrcDF(EzPackage pkg, TaskHost pipe) : base(pkg, pipe) { }
    }

    public class EzSrcConnDF<SrcComp, SrcCM> : EzSrcDF<SrcComp>
        where SrcComp : EzAdapter
        where SrcCM : EzConnectionManager
    {
        public SrcCM SrcConn;
        public EzSrcConnDF(EzContainer parent)
            : base(parent)
        {
            SrcConn = Activator.CreateInstance(typeof(SrcCM), new object[] { Package }) as SrcCM;
            Source.Connection = SrcConn;
        }
        public EzSrcConnDF(EzPackage pkg, TaskHost pipe) : base(pkg, pipe) { }
    }

    public class EzTransformDF<SrcComp, SrcCM, Trans> : EzSrcConnDF<SrcComp, SrcCM>
        where SrcComp : EzAdapter
        where SrcCM : EzConnectionManager
        where Trans : EzComponent
    {
        public Trans Transform;
        public EzTransformDF(EzContainer parent)
            : base(parent)
        {
            Transform = Activator.CreateInstance(typeof(Trans), new object[] { this }) as Trans;
            Transform.AttachTo(Source);
        }
        public EzTransformDF(EzPackage pkg, TaskHost pipe) : base(pkg, pipe) { }
    }

    public class EzTransDestDF<SrcComp, SrcCM, Trans, DestComp> : EzTransformDF<SrcComp, SrcCM, Trans>
        where SrcComp : EzAdapter
        where SrcCM : EzConnectionManager
        where Trans : EzComponent
        where DestComp : EzComponent
    {
        public DestComp Dest;

        public EzTransDestDF(EzContainer parent)
            : base(parent)
        {
            Dest = Activator.CreateInstance(typeof(DestComp), new object[] { this }) as DestComp;
            Dest.AttachTo(Transform);
        }

        public EzTransDestDF(EzPackage pkg, TaskHost pipe) : base(pkg, pipe) { }
    }

    public class EzTransDestConnDF<SrcComp, SrcCM, Trans, DestComp, DestCM> : EzTransDestDF<SrcComp, SrcCM, Trans, DestComp>
        where SrcComp : EzAdapter
        where SrcCM : EzConnectionManager
        where Trans : EzComponent
        where DestComp : EzAdapter
        where DestCM : EzConnectionManager
    {
        public DestCM DestConn;

        public EzTransDestConnDF(EzContainer parent)
            : base(parent)
        {
            DestConn = Activator.CreateInstance(typeof(DestCM), new object[] { Package }) as DestCM;
            Dest.Connection = DestConn;
        }

        public EzTransDestConnDF(EzPackage pkg, TaskHost pipe) : base(pkg, pipe) { }
    }
}