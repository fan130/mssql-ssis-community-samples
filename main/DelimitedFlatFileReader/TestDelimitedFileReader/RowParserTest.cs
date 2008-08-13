using System.Collections.Generic;
using Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace TestDelimitedFileReader
{
    
    
    /// <summary>
    ///This is a test class for RowParserTest and is intended
    ///to contain all RowParserTest Unit Tests
    ///</summary>
    [TestClass()]
    public class RowParserTest
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


        [TestMethod()]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void RowParserBuilderArgumentsTest()
        {          
            RowParser target = new RowParser(null, null, null);
        }

        /// <summary>
        ///A test for ParseNextRow
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void ParseNextRowBadReaderTest()
        {
            RowData rowData = new RowData();
            RowParser target = new RowParser(string.Empty, ",", string.Empty);
            target.ParseNextRow(null, rowData);
        }

        [TestMethod()]
        public void ParseSimpleRows()
        {
            RowData rowData = new RowData();
            IFileReader reader = FileReaderTest.GetReader("A,B,C\r\n1,2,3");
            RowParser rowParser = new RowParser(",", "\r\n", string.Empty);

            rowParser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, "A,B,C\r\n", new string[] { "A", "B", "C" });
            rowParser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, "1,2,3", new string[] { "1", "2", "3" });
        }

        [TestMethod()]
        public void ParseSingleQualifiedColumn()
        {
            RowData rowData = new RowData();
            IFileReader reader = FileReaderTest.GetReader("\"A,B\r\n\",C\r\n1,2,3");
            RowParser rowParser = new RowParser(string.Empty, "\r\n", "\"");

            rowParser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, "\"A,B\r\n\",C\r\n", new string[] { "A,B\r\n,C" });
            rowParser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, "1,2,3", new string[] { "1,2,3" });
        }

        [TestMethod()]
        public void ParseSimpleUnevenRows()
        {
            RowData rowData = new RowData();
            IFileReader reader = FileReaderTest.GetReader("A,B\r\n1,2,3");
            RowParser rowParser = new RowParser(",", "\r\n", string.Empty);

            rowParser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "A", "B"});
            rowParser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "1", "2", "3" });
            rowParser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[0]);
        }

        [TestMethod()]
        public void ParseQualifiedRowsWithError()
        {
            RowData rowData = new RowData();
            IFileReader reader = FileReaderTest.GetReader("A,\"B\" \r\n1,2,3");
            RowParser rowParser = new RowParser(",", "\r\n", "\"");

            rowParser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "A", "B "});
            rowParser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "1", "2", "3" });
        }

        [TestMethod()]
        public void ParseRowsWithUnevenFields()
        {
            RowData rowData = new RowData();
            IFileReader reader = FileReaderTest.GetReader("A,B,C,D\r\n1,2,3");
            RowParser rowParser = new RowParser(",", "\r\n", "\"");

            rowParser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "A", "B", "C", "D" });
            rowParser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "1", "2", "3" });
        }

        [TestMethod()]
        [ExpectedException(typeof(RowColumnNumberOverflow))]
        public void ParseRowWithTooManyColumns()
        {
            RowData rowData = new RowData();
            // It will repeat the given string up to 8000 characters nad that 
            // will pass the maximum number of columns.
            IFileReader reader = new FileReaderTestImpl("ABC,", 2000*4);
            RowParser rowParser = new RowParser(",", "\r\n", string.Empty);

            rowParser.ParseNextRow(reader, rowData);
        }
    }
}
