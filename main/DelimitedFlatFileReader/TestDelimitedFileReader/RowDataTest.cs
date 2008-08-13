using Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace TestDelimitedFileReader
{
    
    
    /// <summary>
    ///This is a test class for RowDataTest and is intended
    ///to contain all RowDataTest Unit Tests
    ///</summary>
    [TestClass()]
    public class RowDataTest
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


        internal static void VerifyParsedRow(RowData rowData, string[] expectedValues)
        {
            VerifyParsedRow(rowData, null, expectedValues);
        }

        internal static void VerifyParsedRow(RowData rowData, string expectedRowText, string[] expectedValues)
        {
            Assert.AreEqual<int>(expectedValues.Length, rowData.ColumnCount);
            if (!string.IsNullOrEmpty(expectedRowText))
            {
                Assert.AreEqual<string>(expectedRowText, rowData.RowText);
            }

            for (int i = 0; i < rowData.ColumnCount && i < expectedValues.Length; i++)
            {
                Assert.AreEqual<string>(expectedValues[i], rowData.GetColumnData(i));
            }
        }

        /// <summary>
        ///A test for RowData Constructor
        ///</summary>
        [TestMethod()]
        public void RowDataConstructorTest()
        {
            RowData target = new RowData();
        }

        /// <summary>
        ///A test for ColumnCount
        ///</summary>
        [TestMethod()]
        public void ColumnCountTest()
        {
            RowData target = new RowData();
            Assert.AreEqual<int>(0, target.ColumnCount);
        }

        /// <summary>
        ///A test for GetColumnData
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(System.ArgumentException))]
        public void GetColumnDataBadArgTest()
        {
            RowData target = new RowData();
            target.GetColumnData(3);
        }

        /// <summary>
        ///A test for GetColumnData
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(System.ArgumentException))]
        public void GetColumnDataBadArgTest2()
        {
            RowData target = new RowData();
            target.GetColumnData(-3);
        }

        /// <summary>
        ///A test for ResetRowData
        ///</summary>
        [TestMethod()]
        public void ResetRowDataTest()
        {
            RowData target = new RowData();
            target.AddColumnData("A");
            target.AddColumnData("B");
            Assert.AreEqual<int>(2, target.ColumnCount);
            target.ResetRowData();
            Assert.AreEqual<int>(0, target.ColumnCount);
        }

        /// <summary>
        ///A test for AddColumnData
        ///</summary>
        [TestMethod()]
        public void AddColumnDataTest()
        {
            RowData target = new RowData();
            target.AddColumnData("ABC");
            Assert.AreEqual<int>(1, target.ColumnCount);
            target.AddColumnData("DEF");
            Assert.AreEqual<int>(2, target.ColumnCount);
            Assert.AreEqual<string>("ABC", target.GetColumnData(0));
            Assert.AreEqual<string>("DEF", target.GetColumnData(1));
        }

        /// <summary>
        ///A test for AddColumnData
        ///</summary>
        [TestMethod()]
        public void RowTextTest()
        {
            RowData target = new RowData();
            Assert.AreEqual<string>(string.Empty, target.RowText);
        }
    }
}
