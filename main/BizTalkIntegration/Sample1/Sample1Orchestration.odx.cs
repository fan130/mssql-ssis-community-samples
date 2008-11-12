
#pragma warning disable 162

namespace Sample1
{

    [Microsoft.XLANGs.BaseTypes.PortTypeOperationAttribute(
        "Operation_1",
        new System.Type[]{
            typeof(Sample1.__messagetype_Sample1_SalesSchema)
        },
        new string[]{
        }
    )]
    [Microsoft.XLANGs.BaseTypes.PortTypeAttribute(Microsoft.XLANGs.BaseTypes.EXLangSAccess.eInternal, "")]
    [System.SerializableAttribute]
    sealed internal class Sample1PortType : Microsoft.BizTalk.XLANGs.BTXEngine.BTXPortBase
    {
        public Sample1PortType(int portInfo, Microsoft.XLANGs.Core.IServiceProxy s)
            : base(portInfo, s)
        { }
        public Sample1PortType(Sample1PortType p)
            : base(p)
        { }

        public override Microsoft.XLANGs.Core.PortBase Clone()
        {
            Sample1PortType p = new Sample1PortType(this);
            return p;
        }

        public static readonly Microsoft.XLANGs.BaseTypes.EXLangSAccess __access = Microsoft.XLANGs.BaseTypes.EXLangSAccess.eInternal;
        #region port reflection support
        static public Microsoft.XLANGs.Core.OperationInfo Operation_1 = new Microsoft.XLANGs.Core.OperationInfo
        (
            "Operation_1",
            System.Web.Services.Description.OperationFlow.OneWay,
            typeof(Sample1PortType),
            typeof(__messagetype_Sample1_SalesSchema),
            null,
            new System.Type[]{},
            new string[]{}
        );
        static public System.Collections.Hashtable OperationsInformation
        {
            get
            {
                System.Collections.Hashtable h = new System.Collections.Hashtable();
                h[ "Operation_1" ] = Operation_1;
                return h;
            }
        }
        #endregion // port reflection support
    }
    //#line 136 "C:\testsamples\Sample1\Sample1Orchestration.odx"
    [Microsoft.XLANGs.BaseTypes.StaticSubscriptionAttribute(
        0, "Sample1Port", "Operation_1", -1, -1, true
    )]
    [Microsoft.XLANGs.BaseTypes.ServicePortsAttribute(
        new Microsoft.XLANGs.BaseTypes.EXLangSParameter[] {
            Microsoft.XLANGs.BaseTypes.EXLangSParameter.ePort|Microsoft.XLANGs.BaseTypes.EXLangSParameter.eImplements
        },
        new System.Type[] {
            typeof(Sample1.Sample1PortType)
        },
        new System.String[] {
            "Sample1Port"
        },
        new System.Type[] {
            null
        }
    )]
    [Microsoft.XLANGs.BaseTypes.ServiceCallTreeAttribute(
        new System.Type[] {
        },
        new System.Type[] {
        },
        new System.Type[] {
        }
    )]
    [Microsoft.XLANGs.BaseTypes.ServiceAttribute(
        Microsoft.XLANGs.BaseTypes.EXLangSAccess.eInternal,
        Microsoft.XLANGs.BaseTypes.EXLangSServiceInfo.eNone|Microsoft.XLANGs.BaseTypes.EXLangSServiceInfo.eAtomic
    )]
    [System.SerializableAttribute]
    [Microsoft.XLANGs.BaseTypes.BPELExportableAttribute(false)]
    [Microsoft.XLANGs.BaseTypes.TransactionAttribute(Retry = true, Batch = true, Timeout = 60, TranIsolationLevel = System.Data.IsolationLevel.Serializable)]
    sealed internal class Sample1Orchestration : Microsoft.BizTalk.XLANGs.BTXEngine.BTXService
    {
        public static readonly Microsoft.XLANGs.BaseTypes.EXLangSAccess __access = Microsoft.XLANGs.BaseTypes.EXLangSAccess.eInternal;
        public static readonly bool __execable = false;
        [Microsoft.XLANGs.BaseTypes.CallCompensationAttribute(
            Microsoft.XLANGs.BaseTypes.EXLangSCallCompensationInfo.eNone,
            new System.String[] {
            },
            new System.String[] {
            }
        )]
        public static void __bodyProxy()
        {
        }
        private static System.Guid _serviceId = Microsoft.XLANGs.Core.HashHelper.HashServiceType(typeof(Sample1Orchestration));
        private static volatile System.Guid[] _activationSubIds;

        private static new object _lockIdentity = new object();

        public static System.Guid UUID { get { return _serviceId; } }
        public override System.Guid ServiceId { get { return UUID; } }

        protected override System.Guid[] ActivationSubGuids
        {
            get { return _activationSubIds; }
            set { _activationSubIds = value; }
        }

        protected override object StaleStateLock
        {
            get { return _lockIdentity; }
        }

        protected override bool HasActivation { get { return true; } }

        internal bool IsExeced = false;

        static Sample1Orchestration()
        {
            Microsoft.BizTalk.XLANGs.BTXEngine.BTXService.CacheStaticState( _serviceId );
        }

        private void ConstructorHelper()
        {
            _segments = new Microsoft.XLANGs.Core.Segment[] {
                new Microsoft.XLANGs.Core.Segment( new Microsoft.XLANGs.Core.Segment.SegmentCode(this.segment0), 0, 0, 0),
                new Microsoft.XLANGs.Core.Segment( new Microsoft.XLANGs.Core.Segment.SegmentCode(this.segment1), 1, 1, 1),
                new Microsoft.XLANGs.Core.Segment( new Microsoft.XLANGs.Core.Segment.SegmentCode(this.segment2), 1, 2, 2)
            };

            _Locks = 0;
            _rootContext = new __Sample1Orchestration_root_0(this);
            _stateMgrs = new Microsoft.XLANGs.Core.IStateManager[3];
            _stateMgrs[0] = _rootContext;
            FinalConstruct();
        }

        public Sample1Orchestration(System.Guid instanceId, Microsoft.BizTalk.XLANGs.BTXEngine.BTXSession session, Microsoft.BizTalk.XLANGs.BTXEngine.BTXEvents tracker)
            : base(instanceId, session, "Sample1Orchestration", tracker)
        {
            ConstructorHelper();
        }

        public Sample1Orchestration(int callIndex, System.Guid instanceId, Microsoft.BizTalk.XLANGs.BTXEngine.BTXService parent)
            : base(callIndex, instanceId, parent, "Sample1Orchestration")
        {
            ConstructorHelper();
        }

        private const string _symInfo = @"
<XsymFile>
<ProcessFlow xmlns:om='http://schemas.microsoft.com/BizTalk/2003/DesignerData'>      <shapeType>RootShape</shapeType>      <ShapeID>ff68cf2a-1ffe-46ff-b326-c682319e9454</ShapeID>      
<children>                          
<ShapeInfo>      <shapeType>ReceiveShape</shapeType>      <ShapeID>ce37850b-3254-4386-816e-d3a2f5e9cb3b</ShapeID>      <ParentLink>ServiceBody_Statement</ParentLink>                <shapeText>Receive_1</shapeText>                  
<children>                </children>
  </ShapeInfo>
                            
<ShapeInfo>      <shapeType>ScopeShape</shapeType>      <ShapeID>433d9806-4cef-42bd-994c-f6b38516f11d</ShapeID>      <ParentLink>ServiceBody_Statement</ParentLink>                <shapeText>Scope_1</shapeText>                  
<children>                          
<ShapeInfo>      <shapeType>VariableAssignmentShape</shapeType>      <ShapeID>55e5b61d-f35d-4a57-830e-e93b1cba4a94</ShapeID>      <ParentLink>ComplexStatement_Statement</ParentLink>                <shapeText>Expression_1</shapeText>                  
<children>                </children>
  </ShapeInfo>
                  </children>
  </ShapeInfo>
                  </children>
  </ProcessFlow>
<Metadata>

<TrkMetadata>
<ActionName>'Sample1Orchestration'</ActionName><IsAtomic>1</IsAtomic><Line>136</Line><Position>14</Position><ShapeID>'e211a116-cb8b-44e7-a052-0de295aa0001'</ShapeID>
</TrkMetadata>

<TrkMetadata>
<Line>147</Line><Position>22</Position><ShapeID>'ce37850b-3254-4386-816e-d3a2f5e9cb3b'</ShapeID>
<Messages>
	<MsgInfo><name>Sample1Message</name><part>part</part><schema>Sample1.SalesSchema</schema><direction>Out</direction></MsgInfo>
</Messages>
</TrkMetadata>

<TrkMetadata>
<ActionName>'??__scope33'</ActionName><IsAtomic>0</IsAtomic><Line>151</Line><Position>13</Position><ShapeID>'433d9806-4cef-42bd-994c-f6b38516f11d'</ShapeID>
<Messages>
</Messages>
</TrkMetadata>

<TrkMetadata>
<Line>156</Line><Position>29</Position><ShapeID>'55e5b61d-f35d-4a57-830e-e93b1cba4a94'</ShapeID>
<Messages>
</Messages>
</TrkMetadata>
</Metadata>
</XsymFile>";

        public override string odXml { get { return _symODXML; } }

        private const string _symODXML = @"
<?xml version='1.0' encoding='utf-8' standalone='yes'?>
<om:MetaModel MajorVersion='1' MinorVersion='3' Core='2b131234-7959-458d-834f-2dc0769ce683' ScheduleModel='66366196-361d-448d-976f-cab5e87496d2' xmlns:om='http://schemas.microsoft.com/BizTalk/2003/DesignerData'>
    <om:Element Type='Module' OID='e83e39e0-eeab-4bae-a6bf-b97ab715f01b' LowerBound='1.1' HigherBound='43.1'>
        <om:Property Name='ReportToAnalyst' Value='True' />
        <om:Property Name='Name' Value='Sample1' />
        <om:Property Name='Signal' Value='False' />
        <om:Element Type='PortType' OID='fee75cea-308f-4005-bd2d-2599455aae50' ParentLink='Module_PortType' LowerBound='4.1' HigherBound='11.1'>
            <om:Property Name='Synchronous' Value='False' />
            <om:Property Name='TypeModifier' Value='Internal' />
            <om:Property Name='ReportToAnalyst' Value='True' />
            <om:Property Name='Name' Value='Sample1PortType' />
            <om:Property Name='Signal' Value='False' />
            <om:Element Type='OperationDeclaration' OID='36764fcd-d5e5-4a19-a3dc-7d261c0ddf22' ParentLink='PortType_OperationDeclaration' LowerBound='6.1' HigherBound='10.1'>
                <om:Property Name='OperationType' Value='OneWay' />
                <om:Property Name='ReportToAnalyst' Value='True' />
                <om:Property Name='Name' Value='Operation_1' />
                <om:Property Name='Signal' Value='False' />
                <om:Element Type='MessageRef' OID='916c0ee1-df58-405f-b2bf-5b87986c3da7' ParentLink='OperationDeclaration_RequestMessageRef' LowerBound='8.13' HigherBound='8.24'>
                    <om:Property Name='Ref' Value='Sample1.SalesSchema' />
                    <om:Property Name='ReportToAnalyst' Value='True' />
                    <om:Property Name='Name' Value='Request' />
                    <om:Property Name='Signal' Value='True' />
                </om:Element>
            </om:Element>
        </om:Element>
        <om:Element Type='ServiceDeclaration' OID='8aa3fe03-25cb-44da-b96d-b0d12cf528f0' ParentLink='Module_ServiceDeclaration' LowerBound='11.1' HigherBound='42.1'>
            <om:Property Name='InitializedTransactionType' Value='True' />
            <om:Property Name='IsInvokable' Value='False' />
            <om:Property Name='TypeModifier' Value='Internal' />
            <om:Property Name='ReportToAnalyst' Value='True' />
            <om:Property Name='Name' Value='Sample1Orchestration' />
            <om:Property Name='Signal' Value='True' />
            <om:Element Type='AtomicTransaction' OID='274e072b-919d-435f-a07e-ad93913bf42c' ParentLink='ServiceDeclaration_Transaction' LowerBound='13.21' HigherBound='13.40'>
                <om:Property Name='ReportToAnalyst' Value='True' />
                <om:Property Name='Name' Value='Transaction_1' />
                <om:Property Name='Signal' Value='False' />
            </om:Element>
            <om:Element Type='VariableDeclaration' OID='20d7e004-6bba-4b2f-9305-0963c4416a22' ParentLink='ServiceDeclaration_VariableDeclaration' LowerBound='18.1' HigherBound='19.1'>
                <om:Property Name='UseDefaultConstructor' Value='True' />
                <om:Property Name='Type' Value='Microsoft.SqlServer.Dts.Runtime.Application' />
                <om:Property Name='ParamDirection' Value='In' />
                <om:Property Name='ReportToAnalyst' Value='True' />
                <om:Property Name='Name' Value='SSISApp' />
                <om:Property Name='Signal' Value='True' />
            </om:Element>
            <om:Element Type='VariableDeclaration' OID='1bd23994-cf01-4556-8f1f-eb79b4794d85' ParentLink='ServiceDeclaration_VariableDeclaration' LowerBound='19.1' HigherBound='20.1'>
                <om:Property Name='UseDefaultConstructor' Value='True' />
                <om:Property Name='Type' Value='Microsoft.SqlServer.Dts.Runtime.Package' />
                <om:Property Name='ParamDirection' Value='In' />
                <om:Property Name='ReportToAnalyst' Value='True' />
                <om:Property Name='Name' Value='SSISPkg' />
                <om:Property Name='Signal' Value='True' />
            </om:Element>
            <om:Element Type='VariableDeclaration' OID='33c98bad-0abd-4089-888b-ffb99c33efb6' ParentLink='ServiceDeclaration_VariableDeclaration' LowerBound='20.1' HigherBound='21.1'>
                <om:Property Name='UseDefaultConstructor' Value='False' />
                <om:Property Name='Type' Value='Microsoft.SqlServer.Dts.Runtime.Variable' />
                <om:Property Name='ParamDirection' Value='In' />
                <om:Property Name='ReportToAnalyst' Value='True' />
                <om:Property Name='Name' Value='SSISVar' />
                <om:Property Name='Signal' Value='True' />
            </om:Element>
            <om:Element Type='TransactionAttribute' OID='aa17216d-cd80-40d0-a913-9874b7764f75' ParentLink='ServiceDeclaration_CLRAttribute' LowerBound='12.1' HigherBound='13.1'>
                <om:Property Name='Batch' Value='True' />
                <om:Property Name='Retry' Value='True' />
                <om:Property Name='Timeout' Value='60' />
                <om:Property Name='Isolation' Value='Serializable' />
                <om:Property Name='Signal' Value='False' />
            </om:Element>
            <om:Element Type='MessageDeclaration' OID='eaa73c86-2aa6-476d-9178-fdf70c78f149' ParentLink='ServiceDeclaration_MessageDeclaration' LowerBound='17.1' HigherBound='18.1'>
                <om:Property Name='Type' Value='Sample1.SalesSchema' />
                <om:Property Name='ParamDirection' Value='In' />
                <om:Property Name='ReportToAnalyst' Value='True' />
                <om:Property Name='Name' Value='Sample1Message' />
                <om:Property Name='Signal' Value='True' />
            </om:Element>
            <om:Element Type='ServiceBody' OID='ff68cf2a-1ffe-46ff-b326-c682319e9454' ParentLink='ServiceDeclaration_ServiceBody'>
                <om:Property Name='Signal' Value='False' />
                <om:Element Type='Receive' OID='ce37850b-3254-4386-816e-d3a2f5e9cb3b' ParentLink='ServiceBody_Statement' LowerBound='23.1' HigherBound='27.1'>
                    <om:Property Name='Activate' Value='True' />
                    <om:Property Name='PortName' Value='Sample1Port' />
                    <om:Property Name='MessageName' Value='Sample1Message' />
                    <om:Property Name='OperationName' Value='Operation_1' />
                    <om:Property Name='OperationMessageName' Value='Request' />
                    <om:Property Name='ReportToAnalyst' Value='True' />
                    <om:Property Name='Name' Value='Receive_1' />
                    <om:Property Name='Signal' Value='True' />
                </om:Element>
                <om:Element Type='Scope' OID='433d9806-4cef-42bd-994c-f6b38516f11d' ParentLink='ServiceBody_Statement' LowerBound='27.1' HigherBound='40.1'>
                    <om:Property Name='InitializedTransactionType' Value='True' />
                    <om:Property Name='IsSynchronized' Value='False' />
                    <om:Property Name='ReportToAnalyst' Value='True' />
                    <om:Property Name='Name' Value='Scope_1' />
                    <om:Property Name='Signal' Value='True' />
                    <om:Element Type='VariableAssignment' OID='55e5b61d-f35d-4a57-830e-e93b1cba4a94' ParentLink='ComplexStatement_Statement' LowerBound='32.1' HigherBound='38.1'>
                        <om:Property Name='Expression' Value='SSISApp = new Microsoft.SqlServer.Dts.Runtime.Application();&#xD;&#xA;SSISPkg = SSISApp.LoadPackage(@&quot;C:\testsamples\Sample1\Sample1.dtsx&quot;,null);&#xD;&#xA;SSISVar = SSISPkg.Variables[&quot;User::orders&quot;];&#xD;&#xA;SSISVar.Value = Sample1Message.SaleSummary.Orders;&#xD;&#xA;SSISPkg.Execute();' />
                        <om:Property Name='ReportToAnalyst' Value='True' />
                        <om:Property Name='Name' Value='Expression_1' />
                        <om:Property Name='Signal' Value='False' />
                    </om:Element>
                </om:Element>
            </om:Element>
            <om:Element Type='PortDeclaration' OID='45b7cc7b-07d8-4027-8f0a-cc01004e97e9' ParentLink='ServiceDeclaration_PortDeclaration' LowerBound='15.1' HigherBound='17.1'>
                <om:Property Name='PortModifier' Value='Implements' />
                <om:Property Name='Orientation' Value='Left' />
                <om:Property Name='PortIndex' Value='-1' />
                <om:Property Name='IsWebPort' Value='False' />
                <om:Property Name='OrderedDelivery' Value='False' />
                <om:Property Name='DeliveryNotification' Value='None' />
                <om:Property Name='Type' Value='Sample1.Sample1PortType' />
                <om:Property Name='ParamDirection' Value='In' />
                <om:Property Name='ReportToAnalyst' Value='True' />
                <om:Property Name='Name' Value='Sample1Port' />
                <om:Property Name='Signal' Value='False' />
                <om:Element Type='LogicalBindingAttribute' OID='4aa40b04-c0c2-4e05-885a-f38eab580764' ParentLink='PortDeclaration_CLRAttribute' LowerBound='15.1' HigherBound='16.1'>
                    <om:Property Name='Signal' Value='False' />
                </om:Element>
            </om:Element>
        </om:Element>
    </om:Element>
</om:MetaModel>
";

        [System.SerializableAttribute]
        public class __Sample1Orchestration_root_0 : Microsoft.XLANGs.Core.ServiceContext
        {
            public __Sample1Orchestration_root_0(Microsoft.XLANGs.Core.Service svc)
                : base(svc, "Sample1Orchestration")
            {
            }

            public override int Index { get { return 0; } }

            public override Microsoft.XLANGs.Core.Segment InitialSegment
            {
                get { return _service._segments[0]; }
            }
            public override Microsoft.XLANGs.Core.Segment FinalSegment
            {
                get { return _service._segments[0]; }
            }

            public override int CompensationSegment { get { return -1; } }
            public override bool OnError()
            {
                Finally();
                return false;
            }

            public override void Finally()
            {
                Sample1Orchestration __svc__ = (Sample1Orchestration)_service;
                __Sample1Orchestration_root_0 __ctx0__ = (__Sample1Orchestration_root_0)(__svc__._stateMgrs[0]);

                if (__svc__.Sample1Port != null)
                {
                    __svc__.Sample1Port.Close(this, null);
                    __svc__.Sample1Port = null;
                }
                base.Finally();
            }

            internal Microsoft.XLANGs.Core.SubscriptionWrapper __subWrapper0;
        }


        [System.SerializableAttribute]
        public class __Sample1Orchestration_1 : Microsoft.XLANGs.Core.AtomicTransaction
        {
            public __Sample1Orchestration_1(Microsoft.XLANGs.Core.Service svc)
                : base(svc, "Sample1Orchestration")
            {
                Retry = true;
                Batch = true;
                Timeout = 60;
                TranIsolationLevel = System.Data.IsolationLevel.Serializable;
            }

            public override int Index { get { return 1; } }

            public override Microsoft.XLANGs.Core.Segment InitialSegment
            {
                get { return _service._segments[1]; }
            }
            public override Microsoft.XLANGs.Core.Segment FinalSegment
            {
                get { return _service._segments[1]; }
            }

            public override int CompensationSegment { get { return -1; } }
            public override bool OnError()
            {
                Finally();
                return false;
            }

            public override void Finally()
            {
                Sample1Orchestration __svc__ = (Sample1Orchestration)_service;
                base.Finally();
            }

            [Microsoft.XLANGs.Core.UserVariableAttribute("Sample1Message")]
            public __messagetype_Sample1_SalesSchema __Sample1Message;
            [System.NonSerializedAttribute]
            [Microsoft.XLANGs.Core.UserVariableAttribute("SSISApp")]
            internal Microsoft.SqlServer.Dts.Runtime.Application __SSISApp;
            [System.NonSerializedAttribute]
            [Microsoft.XLANGs.Core.UserVariableAttribute("SSISPkg")]
            internal Microsoft.SqlServer.Dts.Runtime.Package __SSISPkg;
            [System.NonSerializedAttribute]
            [Microsoft.XLANGs.Core.UserVariableAttribute("SSISVar")]
            internal Microsoft.SqlServer.Dts.Runtime.Variable __SSISVar;
        }


        [System.SerializableAttribute]
        public class ____scope33_2 : Microsoft.XLANGs.Core.ExceptionHandlingContext
        {
            public ____scope33_2(Microsoft.XLANGs.Core.Service svc)
                : base(svc, "??__scope33")
            {
            }

            public override int Index { get { return 2; } }

            public override bool CombineParentCommit { get { return true; } }

            public override Microsoft.XLANGs.Core.Segment InitialSegment
            {
                get { return _service._segments[2]; }
            }
            public override Microsoft.XLANGs.Core.Segment FinalSegment
            {
                get { return _service._segments[2]; }
            }

            public override int CompensationSegment { get { return -1; } }
            public override bool OnError()
            {
                Finally();
                return false;
            }

            public override void Finally()
            {
                Sample1Orchestration __svc__ = (Sample1Orchestration)_service;
                __Sample1Orchestration_1 __ctx1__ = (__Sample1Orchestration_1)(__svc__._stateMgrs[1]);

                if (__ctx1__ != null && __ctx1__.__Sample1Message != null)
                {
                    __ctx1__.UnrefMessage(__ctx1__.__Sample1Message);
                    __ctx1__.__Sample1Message = null;
                }
                if (__ctx1__ != null)
                    __ctx1__.__SSISApp = null;
                if (__ctx1__ != null)
                    __ctx1__.__SSISVar = null;
                if (__ctx1__ != null)
                    __ctx1__.__SSISPkg = null;
                base.Finally();
            }

        }

        private static Microsoft.XLANGs.Core.CorrelationType[] _correlationTypes = null;
        public override Microsoft.XLANGs.Core.CorrelationType[] CorrelationTypes { get { return _correlationTypes; } }

        private static System.Guid[] _convoySetIds;

        public override System.Guid[] ConvoySetGuids
        {
            get { return _convoySetIds; }
            set { _convoySetIds = value; }
        }

        public static object[] StaticConvoySetInformation
        {
            get {
                return null;
            }
        }

        [Microsoft.XLANGs.BaseTypes.LogicalBindingAttribute()]
        [Microsoft.XLANGs.BaseTypes.PortAttribute(
            Microsoft.XLANGs.BaseTypes.EXLangSParameter.eImplements
        )]
        [Microsoft.XLANGs.Core.UserVariableAttribute("Sample1Port")]
        internal Sample1PortType Sample1Port;

        public static Microsoft.XLANGs.Core.PortInfo[] _portInfo = new Microsoft.XLANGs.Core.PortInfo[] {
            new Microsoft.XLANGs.Core.PortInfo(new Microsoft.XLANGs.Core.OperationInfo[] {Sample1PortType.Operation_1},
                                               typeof(Sample1Orchestration).GetField("Sample1Port", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance),
                                               Microsoft.XLANGs.BaseTypes.Polarity.implements,
                                               false,
                                               Microsoft.XLANGs.Core.HashHelper.HashPort(typeof(Sample1Orchestration), "Sample1Port"),
                                               null)
        };

        public override Microsoft.XLANGs.Core.PortInfo[] PortInformation
        {
            get { return _portInfo; }
        }

        static public System.Collections.Hashtable PortsInformation
        {
            get
            {
                System.Collections.Hashtable h = new System.Collections.Hashtable();
                h[_portInfo[0].Name] = _portInfo[0];
                return h;
            }
        }

        public static System.Type[] InvokedServicesTypes
        {
            get
            {
                return new System.Type[] {
                    // type of each service invoked by this service
                };
            }
        }

        public static System.Type[] CalledServicesTypes
        {
            get
            {
                return new System.Type[] {
                };
            }
        }

        public static System.Type[] ExecedServicesTypes
        {
            get
            {
                return new System.Type[] {
                };
            }
        }

        public static object[] StaticSubscriptionsInformation {
            get {
                return new object[1]{
                     new object[5] { _portInfo[0], 0, null , -1, true }
                };
            }
        }

        public static Microsoft.XLANGs.RuntimeTypes.Location[] __eventLocations = new Microsoft.XLANGs.RuntimeTypes.Location[] {
            new Microsoft.XLANGs.RuntimeTypes.Location(0, "00000000-0000-0000-0000-000000000000", 1, true),
            new Microsoft.XLANGs.RuntimeTypes.Location(1, "ce37850b-3254-4386-816e-d3a2f5e9cb3b", 1, true),
            new Microsoft.XLANGs.RuntimeTypes.Location(2, "ce37850b-3254-4386-816e-d3a2f5e9cb3b", 1, false),
            new Microsoft.XLANGs.RuntimeTypes.Location(3, "00000000-0000-0000-0000-000000000000", 1, false),
            new Microsoft.XLANGs.RuntimeTypes.Location(4, "433d9806-4cef-42bd-994c-f6b38516f11d", 1, true),
            new Microsoft.XLANGs.RuntimeTypes.Location(5, "00000000-0000-0000-0000-000000000000", 2, true),
            new Microsoft.XLANGs.RuntimeTypes.Location(6, "55e5b61d-f35d-4a57-830e-e93b1cba4a94", 2, true),
            new Microsoft.XLANGs.RuntimeTypes.Location(7, "55e5b61d-f35d-4a57-830e-e93b1cba4a94", 2, false),
            new Microsoft.XLANGs.RuntimeTypes.Location(8, "00000000-0000-0000-0000-000000000000", 2, false),
            new Microsoft.XLANGs.RuntimeTypes.Location(9, "433d9806-4cef-42bd-994c-f6b38516f11d", 1, false)
        };

        public override Microsoft.XLANGs.RuntimeTypes.Location[] EventLocations
        {
            get { return __eventLocations; }
        }

        public static Microsoft.XLANGs.RuntimeTypes.EventData[] __eventData = new Microsoft.XLANGs.RuntimeTypes.EventData[] {
            new Microsoft.XLANGs.RuntimeTypes.EventData( Microsoft.XLANGs.RuntimeTypes.Operation.Start | Microsoft.XLANGs.RuntimeTypes.Operation.Body),
            new Microsoft.XLANGs.RuntimeTypes.EventData( Microsoft.XLANGs.RuntimeTypes.Operation.Start | Microsoft.XLANGs.RuntimeTypes.Operation.Receive),
            new Microsoft.XLANGs.RuntimeTypes.EventData( Microsoft.XLANGs.RuntimeTypes.Operation.Start | Microsoft.XLANGs.RuntimeTypes.Operation.Scope),
            new Microsoft.XLANGs.RuntimeTypes.EventData( Microsoft.XLANGs.RuntimeTypes.Operation.Start | Microsoft.XLANGs.RuntimeTypes.Operation.Expression),
            new Microsoft.XLANGs.RuntimeTypes.EventData( Microsoft.XLANGs.RuntimeTypes.Operation.End | Microsoft.XLANGs.RuntimeTypes.Operation.Expression),
            new Microsoft.XLANGs.RuntimeTypes.EventData( Microsoft.XLANGs.RuntimeTypes.Operation.End | Microsoft.XLANGs.RuntimeTypes.Operation.Scope),
            new Microsoft.XLANGs.RuntimeTypes.EventData( Microsoft.XLANGs.RuntimeTypes.Operation.End | Microsoft.XLANGs.RuntimeTypes.Operation.Body)
        };

        public static int[] __progressLocation0 = new int[] { 0,0,0,3,3,};
        public static int[] __progressLocation1 = new int[] { 0,0,1,1,2,2,2,4,4,4,9,3,3,3,3,};
        public static int[] __progressLocation2 = new int[] { 6,6,6,7,7,7,7,7,7,7,7,};

        public static int[][] __progressLocations = new int[3] [] {__progressLocation0,__progressLocation1,__progressLocation2};
        public override int[][] ProgressLocations {get {return __progressLocations;} }

        public Microsoft.XLANGs.Core.StopConditions segment0(Microsoft.XLANGs.Core.StopConditions stopOn)
        {
            Microsoft.XLANGs.Core.Segment __seg__ = _segments[0];
            Microsoft.XLANGs.Core.Context __ctx__ = (Microsoft.XLANGs.Core.Context)_stateMgrs[0];
            __Sample1Orchestration_1 __ctx1__ = (__Sample1Orchestration_1)_stateMgrs[1];
            __Sample1Orchestration_root_0 __ctx0__ = (__Sample1Orchestration_root_0)_stateMgrs[0];

            switch (__seg__.Progress)
            {
            case 0:
                Sample1Port = new Sample1PortType(0, this);
                __ctx__.PrologueCompleted = true;
                __ctx0__.__subWrapper0 = new Microsoft.XLANGs.Core.SubscriptionWrapper(ActivationSubGuids[0], Sample1Port, this);
                if ( !PostProgressInc( __seg__, __ctx__, 1 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                if ((stopOn & Microsoft.XLANGs.Core.StopConditions.Initialized) != 0)
                    return Microsoft.XLANGs.Core.StopConditions.Initialized;
                goto case 1;
            case 1:
                __ctx1__ = new __Sample1Orchestration_1(this);
                _stateMgrs[1] = __ctx1__;
                if ( !PostProgressInc( __seg__, __ctx__, 2 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                goto case 2;
            case 2:
                __ctx0__.StartContext(__seg__, __ctx1__);
                if ( !PostProgressInc( __seg__, __ctx__, 3 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                return Microsoft.XLANGs.Core.StopConditions.Blocked;
            case 3:
                if (Sample1Port != null)
                {
                    Sample1Port.Close(__ctx0__, __seg__);
                    Sample1Port = null;
                }
                if (!__ctx0__.CleanupAndPrepareToCommit(__seg__))
                    return Microsoft.XLANGs.Core.StopConditions.Blocked;
                if ( !PostProgressInc( __seg__, __ctx__, 4 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                goto case 4;
            case 4:
                __ctx1__.Finally();
                ServiceDone(__seg__, (Microsoft.XLANGs.Core.Context)_stateMgrs[0]);
                __ctx0__.OnCommit();
                break;
            }
            return Microsoft.XLANGs.Core.StopConditions.Completed;
        }

        public Microsoft.XLANGs.Core.StopConditions segment1(Microsoft.XLANGs.Core.StopConditions stopOn)
        {
            Microsoft.XLANGs.Core.Envelope __msgEnv__ = null;
            Microsoft.XLANGs.Core.Segment __seg__ = _segments[1];
            Microsoft.XLANGs.Core.Context __ctx__ = (Microsoft.XLANGs.Core.Context)_stateMgrs[1];
            ____scope33_2 __ctx2__ = (____scope33_2)_stateMgrs[2];
            __Sample1Orchestration_1 __ctx1__ = (__Sample1Orchestration_1)_stateMgrs[1];
            __Sample1Orchestration_root_0 __ctx0__ = (__Sample1Orchestration_root_0)_stateMgrs[0];

            switch (__seg__.Progress)
            {
            case 0:
                __ctx1__.__SSISApp = default(Microsoft.SqlServer.Dts.Runtime.Application);
                __ctx1__.__SSISPkg = default(Microsoft.SqlServer.Dts.Runtime.Package);
                __ctx1__.__SSISVar = default(Microsoft.SqlServer.Dts.Runtime.Variable);
                __ctx1__.__Sample1Message = null;
                __ctx__.PrologueCompleted = true;
                if ( !PostProgressInc( __seg__, __ctx__, 1 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                goto case 1;
            case 1:
                if ( !PreProgressInc( __seg__, __ctx__, 2 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                Tracker.FireEvent(__eventLocations[0],__eventData[0],_stateMgrs[1].TrackDataStream );
                if (IsDebugged)
                    return Microsoft.XLANGs.Core.StopConditions.InBreakpoint;
                goto case 2;
            case 2:
                if ( !PreProgressInc( __seg__, __ctx__, 3 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                Tracker.FireEvent(__eventLocations[1],__eventData[1],_stateMgrs[1].TrackDataStream );
                if (IsDebugged)
                    return Microsoft.XLANGs.Core.StopConditions.InBreakpoint;
                goto case 3;
            case 3:
                if (!Sample1Port.GetMessageId(__ctx0__.__subWrapper0.getSubscription(this), __seg__, __ctx1__, out __msgEnv__))
                    return Microsoft.XLANGs.Core.StopConditions.Blocked;
                if (__ctx1__.__Sample1Message != null)
                    __ctx1__.UnrefMessage(__ctx1__.__Sample1Message);
                __ctx1__.__Sample1Message = new __messagetype_Sample1_SalesSchema("Sample1Message", __ctx1__);
                __ctx1__.RefMessage(__ctx1__.__Sample1Message);
                Sample1Port.ReceiveMessage(0, __msgEnv__, __ctx1__.__Sample1Message, null, (Microsoft.XLANGs.Core.Context)_stateMgrs[1], __seg__);
                if ( !PostProgressInc( __seg__, __ctx__, 4 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                goto case 4;
            case 4:
                if ( !PreProgressInc( __seg__, __ctx__, 5 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                {
                    Microsoft.XLANGs.RuntimeTypes.EventData __edata = new Microsoft.XLANGs.RuntimeTypes.EventData(Microsoft.XLANGs.RuntimeTypes.Operation.End | Microsoft.XLANGs.RuntimeTypes.Operation.Receive);
                    __edata.Messages.Add(__ctx1__.__Sample1Message);
                    __edata.PortName = @"Sample1Port";
                    Tracker.FireEvent(__eventLocations[2],__edata,_stateMgrs[1].TrackDataStream );
                }
                if (IsDebugged)
                    return Microsoft.XLANGs.Core.StopConditions.InBreakpoint;
                goto case 5;
            case 5:
                __ctx1__.__SSISApp = new Microsoft.SqlServer.Dts.Runtime.Application();
                if ( !PostProgressInc( __seg__, __ctx__, 6 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                goto case 6;
            case 6:
                __ctx1__.__SSISPkg = new Microsoft.SqlServer.Dts.Runtime.Package();
                if ( !PostProgressInc( __seg__, __ctx__, 7 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                goto case 7;
            case 7:
                if ( !PreProgressInc( __seg__, __ctx__, 8 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                Tracker.FireEvent(__eventLocations[4],__eventData[2],_stateMgrs[1].TrackDataStream );
                if (IsDebugged)
                    return Microsoft.XLANGs.Core.StopConditions.InBreakpoint;
                goto case 8;
            case 8:
                __ctx2__ = new ____scope33_2(this);
                _stateMgrs[2] = __ctx2__;
                if ( !PostProgressInc( __seg__, __ctx__, 9 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                goto case 9;
            case 9:
                __ctx1__.StartContext(__seg__, __ctx2__);
                if ( !PostProgressInc( __seg__, __ctx__, 10 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                return Microsoft.XLANGs.Core.StopConditions.Blocked;
            case 10:
                if ( !PreProgressInc( __seg__, __ctx__, 11 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                Tracker.FireEvent(__eventLocations[9],__eventData[5],_stateMgrs[1].TrackDataStream );
                __ctx2__.Finally();
                if (IsDebugged)
                    return Microsoft.XLANGs.Core.StopConditions.InBreakpoint;
                goto case 11;
            case 11:
                if ( !PreProgressInc( __seg__, __ctx__, 12 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                Tracker.FireEvent(__eventLocations[3],__eventData[6],_stateMgrs[1].TrackDataStream );
                if (IsDebugged)
                    return Microsoft.XLANGs.Core.StopConditions.InBreakpoint;
                goto case 12;
            case 12:
                if (!__ctx1__.CleanupAndPrepareToCommit(__seg__))
                    return Microsoft.XLANGs.Core.StopConditions.Blocked;
                if ( !PostProgressInc( __seg__, __ctx__, 13 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                goto case 13;
            case 13:
                if ( !PreProgressInc( __seg__, __ctx__, 14 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                __ctx1__.OnCommit();
                goto case 14;
            case 14:
                __seg__.SegmentDone();
                _segments[0].PredecessorDone(this);
                break;
            }
            return Microsoft.XLANGs.Core.StopConditions.Completed;
        }

        public Microsoft.XLANGs.Core.StopConditions segment2(Microsoft.XLANGs.Core.StopConditions stopOn)
        {
            Microsoft.XLANGs.Core.Segment __seg__ = _segments[2];
            Microsoft.XLANGs.Core.Context __ctx__ = (Microsoft.XLANGs.Core.Context)_stateMgrs[2];
            ____scope33_2 __ctx2__ = (____scope33_2)_stateMgrs[2];
            __Sample1Orchestration_1 __ctx1__ = (__Sample1Orchestration_1)_stateMgrs[1];

            switch (__seg__.Progress)
            {
            case 0:
                __ctx__.PrologueCompleted = true;
                if ( !PostProgressInc( __seg__, __ctx__, 1 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                goto case 1;
            case 1:
                if ( !PreProgressInc( __seg__, __ctx__, 2 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                Tracker.FireEvent(__eventLocations[6],__eventData[3],_stateMgrs[2].TrackDataStream );
                if (IsDebugged)
                    return Microsoft.XLANGs.Core.StopConditions.InBreakpoint;
                goto case 2;
            case 2:
                __ctx1__.__SSISApp = new Microsoft.SqlServer.Dts.Runtime.Application();
                if ( !PostProgressInc( __seg__, __ctx__, 3 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                goto case 3;
            case 3:
                if ( !PreProgressInc( __seg__, __ctx__, 4 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                Tracker.FireEvent(__eventLocations[7],__eventData[4],_stateMgrs[2].TrackDataStream );
                if (IsDebugged)
                    return Microsoft.XLANGs.Core.StopConditions.InBreakpoint;
                goto case 4;
            case 4:
                __ctx1__.__SSISPkg = __ctx1__.__SSISApp.LoadPackage(@"C:\testsamples\Sample1\Sample1.dtsx", null);
                if (__ctx1__ != null)
                    __ctx1__.__SSISApp = null;
                if ( !PostProgressInc( __seg__, __ctx__, 5 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                goto case 5;
            case 5:
                __ctx1__.__SSISVar = __ctx1__.__SSISPkg.Variables["User::orders"];
                if ( !PostProgressInc( __seg__, __ctx__, 6 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                goto case 6;
            case 6:
                __ctx1__.__SSISVar.Value = (System.String)__ctx1__.__Sample1Message.part.GetDistinguishedField("SaleSummary.Orders");
                if (__ctx1__ != null)
                    __ctx1__.__SSISVar = null;
                if (__ctx1__ != null && __ctx1__.__Sample1Message != null)
                {
                    __ctx1__.UnrefMessage(__ctx1__.__Sample1Message);
                    __ctx1__.__Sample1Message = null;
                }
                if ( !PostProgressInc( __seg__, __ctx__, 7 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                goto case 7;
            case 7:
                __ctx1__.__SSISPkg.Execute();
                if (__ctx1__ != null)
                    __ctx1__.__SSISPkg = null;
                if ( !PostProgressInc( __seg__, __ctx__, 8 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                goto case 8;
            case 8:
                if (!__ctx2__.CleanupAndPrepareToCommit(__seg__))
                    return Microsoft.XLANGs.Core.StopConditions.Blocked;
                if ( !PostProgressInc( __seg__, __ctx__, 9 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                goto case 9;
            case 9:
                if ( !PreProgressInc( __seg__, __ctx__, 10 ) )
                    return Microsoft.XLANGs.Core.StopConditions.Paused;
                __ctx2__.OnCommit();
                goto case 10;
            case 10:
                __seg__.SegmentDone();
                _segments[1].PredecessorDone(this);
                break;
            }
            return Microsoft.XLANGs.Core.StopConditions.Completed;
        }
    }

    [System.SerializableAttribute]
    sealed public class __Sample1_SalesSchema__ : Microsoft.XLANGs.Core.XSDPart
    {
        private static Sample1.SalesSchema _schema = new Sample1.SalesSchema();

        public __Sample1_SalesSchema__(Microsoft.XLANGs.Core.XMessage msg, string name, int index) : base(msg, name, index) { }

        
        #region part reflection support
        public static Microsoft.XLANGs.BaseTypes.SchemaBase PartSchema { get { return (Microsoft.XLANGs.BaseTypes.SchemaBase)_schema; } }
        #endregion // part reflection support
    }

    [Microsoft.XLANGs.BaseTypes.MessageTypeAttribute(
        Microsoft.XLANGs.BaseTypes.EXLangSAccess.ePublic,
        Microsoft.XLANGs.BaseTypes.EXLangSMessageInfo.eThirdKind,
        "Sample1.SalesSchema",
        new System.Type[]{
            typeof(Sample1.SalesSchema)
        },
        new string[]{
            "part"
        },
        new System.Type[]{
            typeof(__Sample1_SalesSchema__)
        },
        0,
        @"http://OrdersSchema.FlatFileSchema1#Sales"
    )]
    [System.SerializableAttribute]
    sealed public class __messagetype_Sample1_SalesSchema : Microsoft.BizTalk.XLANGs.BTXEngine.BTXMessage
    {
        public __Sample1_SalesSchema__ part;

        private void __CreatePartWrappers()
        {
            part = new __Sample1_SalesSchema__(this, "part", 0);
            this.AddPart("part", 0, part);
        }

        public __messagetype_Sample1_SalesSchema(string msgName, Microsoft.XLANGs.Core.Context ctx) : base(msgName, ctx)
        {
            __CreatePartWrappers();
        }
    }

    [Microsoft.XLANGs.BaseTypes.BPELExportableAttribute(false)]
    sealed public class _MODULE_PROXY_ { }
}
