using Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace TestDelimitedFileReader
{
    
    
    /// <summary>
    ///This is a test class for MessageStringsTest and is intended
    ///to contain all MessageStringsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MessageStringsTest
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
        ///A test for UnsupportedDataType
        ///</summary>
        [TestMethod()]
        public void UnsupportedDataTypeTest()
        {
            VeryfyMessageString(MessageStrings.UnsupportedDataType("DT_BYTES"), new string [] {"DT_BYTES"});
        }

        /// <summary>
        ///A test for RowOveflow
        ///</summary>
        [TestMethod()]
        public void RowOveflowTest()
        {
            VeryfyMessageString(MessageStrings.RowOveflow(17, 33, 12), new string[] { "17", "33", "12" });
        }

        /// <summary>
        ///A test for PropertyStringTooLong
        ///</summary>
        [TestMethod()]
        public void PropertyStringTooLongTest()
        {
            VeryfyMessageString(MessageStrings.PropertyStringTooLong("propertyName", "propertyValue"), new string[] { "propertyName", "propertyValue" });
        }

        /// <summary>
        ///A test for ParsingBufferOverflow
        ///</summary>
        [TestMethod()]
        public void ParsingBufferOverflowTest()
        {
            VeryfyMessageString(MessageStrings.ParsingBufferOverflow(17, 33, 64000), new string[] { "17", "33", "64000" });
        }

        /// <summary>
        ///A test for MaximumColumnNumberOverflow
        ///</summary>
        [TestMethod()]
        public void MaximumColumnNumberOverflowTest()
        {
            VeryfyMessageString(MessageStrings.MaximumColumnNumberOverflow(17, 2000), new string[] { "17", "2000" });
        }

        /// <summary>
        ///A test for InvalidPropertyValue
        ///</summary>
        [TestMethod()]
        public void InvalidPropertyValueTest()
        {
            VeryfyMessageString(MessageStrings.InvalidPropertyValue("propertyName", "propertyValue"), new string[] { "propertyName", "propertyValue" });
        }

        /// <summary>
        ///A test for InvalidConnectionReference
        ///</summary>
        [TestMethod()]
        public void InvalidConnectionReferenceTest()
        {
            VeryfyMessageString(MessageStrings.InvalidConnectionReference("Connection"), new string[] { "Connection" });
        }

        /// <summary>
        ///A test for FileDoesNotExist
        ///</summary>
        [TestMethod()]
        public void FileDoesNotExistTest()
        {
            VeryfyMessageString(MessageStrings.FileDoesNotExist("c:\\temp\\file.txt"), new string[] { "c:\\temp\\file.txt" });
        }

        /// <summary>
        ///A test for FailedToAssignColumnValue
        ///</summary>
        [TestMethod()]
        public void FailedToAssignColumnValueTest()
        {
            VeryfyMessageString(MessageStrings.FailedToAssignColumnValue(17, "some data", "column \"First column\" (343)"), new string[] { "17", "some data", "column \"First column\" (343)" });
        }

        /// <summary>
        ///A test for DefaultColumnName
        ///</summary>
        [TestMethod()]
        public void DefaultColumnNameTest()
        {
            VeryfyMessageString(MessageStrings.DefaultColumnName(77), new string[] { "77" });
        }

        /// <summary>
        ///A test for CantFindProperty
        ///</summary>
        [TestMethod()]
        public void CantFindPropertyTest()
        {
            VeryfyMessageString(MessageStrings.CantFindProperty("propertyName"), new string[] { "propertyName" });
        }

        private void VeryfyMessageString(string messageString, string[] substrings)
        {
            Assert.IsFalse(string.IsNullOrEmpty(messageString));

            foreach (string substring in substrings)
            {
                Assert.IsTrue(messageString.Contains(substring));
            }
        }
    }
}
