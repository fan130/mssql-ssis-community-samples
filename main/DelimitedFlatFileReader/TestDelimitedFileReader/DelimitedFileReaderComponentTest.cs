using Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace TestDelimitedFileReader
{
    
    
    /// <summary>
    ///This is a test class for DelimitedFileReaderComponentTest and is intended
    ///to contain all DelimitedFileReaderComponentTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DelimitedFileReaderComponentTest
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
        ///A test for DelimitedFileReaderComponent Constructor
        ///</summary>
        [TestMethod()]
        public void DelimitedFileReaderComponentConstructorTest()
        {
            DelimitedFileReaderComponent target = new DelimitedFileReaderComponent();
        }

        ///// <summary>
        /////A test for DelimitedFileReaderComponent Constructor
        /////</summary>
        //[TestMethod()]
        //public void ProvideComponentPropertiesTest()
        //{
        //    DelimitedFileReaderComponent target = new DelimitedFileReaderComponent();
        //    target.ProvideComponentProperties();
        //}

        ///// <summary>
        /////A test for DelimitedFileReaderComponent Constructor
        /////</summary>
        //[TestMethod()]
        //[ExpectedException(typeof(System.Runtime.InteropServices.COMException))]
        //public void InsertInputTest()
        //{
        //    DelimitedFileReaderComponent target = new DelimitedFileReaderComponent();
        //    target.InsertInput(Microsoft.SqlServer.Dts.Pipeline.Wrapper.DTSInsertPlacement.IP_AFTER, 0);
        //}
    }
}
