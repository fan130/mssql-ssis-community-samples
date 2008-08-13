using Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace TestDelimitedFileReader
{
    
    
    /// <summary>
    ///This is a test class for PropertiesManagerTest and is intended
    ///to contain all PropertiesManagerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PropertiesManagerTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for PropertiesManager Constructor
        ///</summary>
        [TestMethod()]
        public void PropertiesManagerConstructorTest()
        {
            PropertiesManager target = new PropertiesManager();
        }

        /// <summary>
        ///A test for ValidateProperties
        ///</summary>
        [TestMethod()]
        public void ValidatePropertiesTest()
        {
            PropertiesManager target = new PropertiesManager();
            IDTSCustomPropertyCollection100 customPropertyCollection = new CustomPropertyCollectionTestImpl();
            PropertiesManager.AddComponentProperties(customPropertyCollection);

            Assert.AreEqual<DTSValidationStatus>(DTSValidationStatus.VS_ISVALID, target.ValidateProperties(customPropertyCollection, DTSValidationStatus.VS_ISVALID));
        }

        /// <summary>
        ///A test for ValidatePropertyValue
        ///</summary>
        [TestMethod()]
        public void ValidatePropertyValueTest1()
        {
            PropertiesManager target = new PropertiesManager(); // TODO: Initialize to an appropriate value
            Assert.AreEqual<DTSValidationStatus>(DTSValidationStatus.VS_ISVALID, target.ValidatePropertyValue("AnyName", "AnyValue", DTSValidationStatus.VS_ISVALID));
        }

        /// <summary>
        ///A test for ValidatePropertyValue
        ///</summary>
        [TestMethod()]
        public void ValidatePropertyValueTest2()
        {
            PropertiesManager target = new PropertiesManager(); 
            Assert.AreEqual<DTSValidationStatus>(DTSValidationStatus.VS_ISBROKEN, target.ValidatePropertyValue("AnyName", "AnyValue", DTSValidationStatus.VS_ISBROKEN));
        }

        /// <summary>
        ///A test for ValidatePropertyValue
        ///</summary>
        [TestMethod()]
        public void ValidatePropertyValueTest3()
        {
            PropertiesManager target = new PropertiesManager(); 
            Assert.AreEqual<DTSValidationStatus>(DTSValidationStatus.VS_ISVALID, target.ValidatePropertyValue(PropertiesManager.IsUnicodePropName, true, DTSValidationStatus.VS_ISVALID));
        }

        /// <summary>
        ///A test for ValidatePropertyValue
        ///</summary>
        [TestMethod()]
        public void ValidatePropertyValueTest4()
        {
            PropertiesManager target = new PropertiesManager(); 
            Assert.AreEqual<DTSValidationStatus>(DTSValidationStatus.VS_ISCORRUPT, target.ValidatePropertyValue(PropertiesManager.TreatEmptyStringsAsNullPropName, 7, DTSValidationStatus.VS_ISVALID));
        }

        /// <summary>
        ///A test for ValidatePropertyValue
        ///</summary>
        [TestMethod()]
        public void ValidatePropertyValueTest5()
        {
            PropertiesManager target = new PropertiesManager(); 
            Assert.AreEqual<DTSValidationStatus>(DTSValidationStatus.VS_ISVALID, target.ValidatePropertyValue(PropertiesManager.CodePagePropName, true, DTSValidationStatus.VS_ISVALID));
        }

        /// <summary>
        ///A test for ValidatePropertyValue
        ///</summary>
        [TestMethod()]
        public void ValidatePropertyValueTest6()
        {
            PropertiesManager target = new PropertiesManager();
            Assert.AreEqual<DTSValidationStatus>(DTSValidationStatus.VS_ISCORRUPT, target.ValidatePropertyValue(PropertiesManager.HeaderRowDelimiterPropName, null, DTSValidationStatus.VS_ISVALID));
        }

        /// <summary>
        ///A test for ValidatePropertyValue
        ///</summary>
        [TestMethod()]
        public void ValidatePropertyValueTest7()
        {
            PropertiesManager target = new PropertiesManager();
            Assert.AreEqual<DTSValidationStatus>(DTSValidationStatus.VS_ISVALID, target.ValidatePropertyValue(PropertiesManager.TextQualifierPropName, string.Empty, DTSValidationStatus.VS_ISVALID));
        }

        /// <summary>
        ///A test for ValidatePropertyValue
        ///</summary>
        [TestMethod()]
        public void ValidatePropertyValueTest8()
        {
            PropertiesManager target = new PropertiesManager();
            Assert.AreEqual<DTSValidationStatus>(DTSValidationStatus.VS_ISBROKEN, target.ValidatePropertyValue(PropertiesManager.ColumnDelimiterPropName, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", DTSValidationStatus.VS_ISVALID));
        }

        /// <summary>
        ///A test for ValidatePropertyValue
        ///</summary>
        [TestMethod()]
        public void ValidatePropertyValueTest9()
        {
            PropertiesManager target = new PropertiesManager();
            target.PostErrorEvent += delegate(string message)
            {
                Assert.AreEqual<string>(MessageStrings.RowDelimiterEmpty, message);
            };
            Assert.AreEqual<DTSValidationStatus>(DTSValidationStatus.VS_ISBROKEN, target.ValidatePropertyValue(PropertiesManager.RowDelimiterPropName, string.Empty, DTSValidationStatus.VS_ISVALID));
        }

        /// <summary>
        ///A test for ValidatePropertyValue
        ///</summary>
        [TestMethod()]
        public void ValidatePropertyValueTest10()
        {
            PropertiesManager target = new PropertiesManager();
            Assert.AreEqual<DTSValidationStatus>(DTSValidationStatus.VS_ISCORRUPT, target.ValidatePropertyValue(PropertiesManager.RowDelimiterPropName, string.Empty, DTSValidationStatus.VS_ISCORRUPT));
        }

        /// <summary>
        ///A test for ValidateDelimiterProperty
        ///</summary>
        [TestMethod()]
        [DeploymentItem("DelimitedFileReader.dll")]
        public void ValidateDelimiterPropertyTest()
        {
            PropertiesManager_Accessor target = new PropertiesManager_Accessor(); // TODO: Initialize to an appropriate value
            Assert.AreEqual<DTSValidationStatus>(DTSValidationStatus.VS_ISVALID, target.ValidateDelimiterProperty(PropertiesManager.ColumnDelimiterPropName, ","));
        }

        /// <summary>
        ///A test for ValidateRowDelimiterProperty
        ///</summary>
        [TestMethod()]
        [DeploymentItem("DelimitedFileReader.dll")]
        public void ValidateRowDelimiterPropertyTest()
        {
            PropertiesManager_Accessor target = new PropertiesManager_Accessor();
            Assert.AreEqual<DTSValidationStatus>(DTSValidationStatus.VS_ISVALID, target.ValidateRowDelimiterProperty(PropertiesManager.RowDelimiterPropName, "\r\n"));
        }

        /// <summary>
        ///A test for ValidateBooleanProperty
        ///</summary>
        [TestMethod()]
        [DeploymentItem("DelimitedFileReader.dll")]
        public void ValidateBooleanPropertyTest()
        {
            PropertiesManager_Accessor target = new PropertiesManager_Accessor(); 
            Assert.AreEqual<DTSValidationStatus>(DTSValidationStatus.VS_ISCORRUPT, target.ValidateBooleanProperty(PropertiesManager.IsUnicodePropName, "blah"));
        }

        /// <summary>
        ///A test for PostError
        ///</summary>
        [TestMethod()]
        [DeploymentItem("DelimitedFileReader.dll")]
        public void PostErrorTest()
        {
            PropertiesManager_Accessor target = new PropertiesManager_Accessor();
            target.PostError("Message");
        }

        /// <summary>
        ///A test for GetPropertyValue
        ///</summary>
        [TestMethod()]
        public void GetPropertyValueTest()
        {
            IDTSCustomPropertyCollection100 customPropertyCollection = new CustomPropertyCollectionTestImpl();
            IDTSCustomProperty100 property = customPropertyCollection.New();
            property.Name = "Name";
            property.Value = 77;
            Assert.AreEqual<int>(77, (int)PropertiesManager.GetPropertyValue(customPropertyCollection, "Name"));
        }

        /// <summary>
        ///A test for AddCustomProperty
        ///</summary>
        [TestMethod()]
        [DeploymentItem("DelimitedFileReader.dll")]
        public void AddCustomPropertyTest1()
        {
            IDTSCustomPropertyCollection100 customPropertyCollection = new CustomPropertyCollectionTestImpl();
            PropertiesManager_Accessor.AddCustomProperty(customPropertyCollection, "Name", "Description", "Value");

            Assert.AreEqual<string>("Value", (string)PropertiesManager.GetPropertyValue(customPropertyCollection, "Name"));
            Assert.AreEqual(null, PropertiesManager.GetPropertyValue(customPropertyCollection, "Unexpected"));
        }

        /// <summary>
        ///A test for AddCustomProperty
        ///</summary>
        [TestMethod()]
        [DeploymentItem("DelimitedFileReader.dll")]
        public void AddCustomPropertyTest()
        {
            IDTSCustomPropertyCollection100 customPropertyCollection = new CustomPropertyCollectionTestImpl();
            PropertiesManager_Accessor.AddCustomProperty(customPropertyCollection, "NewName", "Test description", 100.25, "TestConverter");
            IDTSCustomProperty100 prop = customPropertyCollection["NewName"];

            Assert.AreEqual<string>("Test description", prop.Description);
            Assert.AreEqual<string>("TestConverter", prop.TypeConverter);
            Assert.AreEqual(100.25, prop.Value);
        }

        /// <summary>
        ///A test for AddComponentProperties
        ///</summary>
        [TestMethod()]
        public void AddComponentPropertiesTest()
        {
            IDTSCustomPropertyCollection100 customPropertyCollection = new CustomPropertyCollectionTestImpl();
            PropertiesManager.AddComponentProperties(customPropertyCollection);

            Assert.AreEqual<string>(",", (string)PropertiesManager.GetPropertyValue(customPropertyCollection, PropertiesManager.ColumnDelimiterPropName));
            Assert.AreEqual<string>("\r\n", (string)PropertiesManager.GetPropertyValue(customPropertyCollection, PropertiesManager.RowDelimiterPropName));
            Assert.AreEqual<string>(string.Empty, (string)PropertiesManager.GetPropertyValue(customPropertyCollection, PropertiesManager.TextQualifierPropName));
            Assert.AreEqual<string>("\r\n", (string)PropertiesManager.GetPropertyValue(customPropertyCollection, PropertiesManager.HeaderRowDelimiterPropName));
            Assert.AreEqual<bool>(true, (bool)PropertiesManager.GetPropertyValue(customPropertyCollection, PropertiesManager.IsUnicodePropName));
            Assert.AreEqual<bool>(true, (bool)PropertiesManager.GetPropertyValue(customPropertyCollection, PropertiesManager.TreatEmptyStringsAsNullPropName));
            Assert.AreEqual<bool>(false, (bool)PropertiesManager.GetPropertyValue(customPropertyCollection, PropertiesManager.ColumnNamesInFirstRowPropName));
            Assert.AreEqual<int>(1252, (int)PropertiesManager.GetPropertyValue(customPropertyCollection, PropertiesManager.CodePagePropName));
            Assert.AreEqual<int>(0, (int)PropertiesManager.GetPropertyValue(customPropertyCollection, PropertiesManager.DataRowsToSkipPropName));
            Assert.AreEqual<int>(0, (int)PropertiesManager.GetPropertyValue(customPropertyCollection, PropertiesManager.HeaderRowsToSkipPropName));
        }
    }
}
