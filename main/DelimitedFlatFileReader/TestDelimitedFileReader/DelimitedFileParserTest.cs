using Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;

namespace TestDelimitedFileReader
{
    
    
    /// <summary>
    ///This is a test class for DelimitedFileParserTest and is intended
    ///to contain all DelimitedFileParserTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DelimitedFileParserTest
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
        ///A test for DelimitedFileParser Constructor
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void DelimitedFileParserConstructorTest1()
        {
            DelimitedFileParser target = new DelimitedFileParser(string.Empty, string.Empty);
        }

        /// <summary>
        ///A test for DelimitedFileParser Constructor
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void DelimitedFileParserConstructorTest2()
        {
            DelimitedFileParser target = new DelimitedFileParser(null, string.Empty);
        }

        /// <summary>
        ///A test for DelimitedFileParser Constructor
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void DelimitedFileParserConstructorTest3()
        {
            DelimitedFileParser target = new DelimitedFileParser(",", string.Empty);
        }

        /// <summary>
        ///A test for TextQualifier
        ///</summary>
        [TestMethod()]
        public void TextQualifierTest()
        {
            DelimitedFileParser target = new DelimitedFileParser(",", "\r\n");
            string expected = "\"";
            string actual;
            target.TextQualifier = expected;
            actual = target.TextQualifier;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for HeaderRowsToSkip
        ///</summary>
        [TestMethod()]
        public void HeaderRowsToSkipTest()
        {
            DelimitedFileParser target = new DelimitedFileParser(",", "\r\n");
            int expected = 5;
            int actual;
            target.HeaderRowsToSkip = expected;
            actual = target.HeaderRowsToSkip;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for HeaderRowsToSkip
        ///</summary>
        [TestMethod()]
        public void DataRowsToSkipTest()
        {
            DelimitedFileParser target = new DelimitedFileParser(",", "\r\n");
            int expected = 17;
            int actual;
            target.DataRowsToSkip = expected;
            actual = target.DataRowsToSkip;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for HeaderRowDelimiter
        ///</summary>
        [TestMethod()]
        public void HeaderRowDelimiterTest()
        {
            DelimitedFileParser target = new DelimitedFileParser(",", "\r\n");
            string expected = "\r\n";
            string actual;
            target.HeaderRowDelimiter = expected;
            actual = target.HeaderRowDelimiter;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ColumnNameInFirstRow
        ///</summary>
        [TestMethod()]
        public void ColumnNameInFirstRowTest()
        {
            DelimitedFileParser target = new DelimitedFileParser(",", "\r\n");
            bool expected = false;
            bool actual;
            target.ColumnNameInFirstRow = expected;
            actual = target.ColumnNameInFirstRow;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void ParseNExtRowArgumentTest()
        {
            DelimitedFileParser parser = new DelimitedFileParser(",", "\r\n");

            parser.ParseNextRow(null, null);
        }

        [TestMethod()]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void SkipHeaderRowsArgumentTest()
        {
            DelimitedFileParser parser = new DelimitedFileParser(",", "\r\n");

            parser.SkipHeaderRows(null);
        }

        [TestMethod()]
        public void ReadColumnNamesTest()
        {
            RowData rowData = new RowData();
            IFileReader reader = FileReaderTest.GetReader("A,B,C\r\n1,2,3");
            DelimitedFileParser parser = new DelimitedFileParser(",", "\r\n");
            parser.ColumnNameInFirstRow = true;

            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "A", "B", "C" });
        }

        [TestMethod()]
        public void ReadColumnNamesAfterHeaderTest()
        {
            RowData rowData = new RowData();
            IFileReader reader = FileReaderTest.GetReader("\r\n\r\nHeader text\r\nA,B,C\r\n1,2,3");
            DelimitedFileParser parser = new DelimitedFileParser(",", "\r\n");
            parser.ColumnNameInFirstRow = true;
            parser.HeaderRowDelimiter = "\r\n";
            parser.HeaderRowsToSkip = 3;

            parser.SkipHeaderRows(reader);
            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "A", "B", "C" });
        }

        [TestMethod()]
        public void ReadQualifiedColumnNamesAfterHeaderTest()
        {
            RowData rowData = new RowData();
            IFileReader reader = FileReaderTest.GetReader("\r\n\r\nHeader text\r\n\"A\",B,\"C\"\r\n1,2,3");
            DelimitedFileParser parser = new DelimitedFileParser(",", "\r\n");
            parser.ColumnNameInFirstRow = true;
            parser.HeaderRowDelimiter = "\r\n";
            parser.HeaderRowsToSkip = 3;
            parser.TextQualifier = "\"";

            parser.SkipHeaderRows(reader);
            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "A", "B", "C" });
        }

        /// <summary>
        ///A test for ParseNextRow
        ///</summary>
        [TestMethod()]
        public void ReadSingleDataRow()
        {
            RowData rowData = new RowData();
            IFileReader reader = FileReaderTest.GetReader("1,2,3");
            DelimitedFileParser parser = new DelimitedFileParser(",", "\r\n");

            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "1", "2", "3" });
        }

        /// <summary>
        ///A test for ParseNextRow
        ///</summary>
        [TestMethod()]
        public void ReadSingleColumnRows()
        {
            RowData rowData = new RowData();
            IFileReader reader = FileReaderTest.GetReader("1,2,3");
            DelimitedFileParser parser = new DelimitedFileParser(string.Empty, ",");

            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "1" });
            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "2" });
            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "3" });
        }

        /// <summary>
        ///A test for ParseNextRow
        ///</summary>
        [TestMethod()]
        public void ReadSingleColumnRows2()
        {
            RowData rowData = new RowData();
            IFileReader reader = FileReaderTest.GetReader("ABC\r\nDEF\r\nGHI");
            DelimitedFileParser parser = new DelimitedFileParser(string.Empty, "\r\n");

            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "ABC" });
            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "DEF" });
            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "GHI" });
        }

        /// <summary>
        ///A test for ParseNextRow
        ///</summary>
        [TestMethod()]
        public void ReadSingleColumnRows3()
        {
            RowData rowData = new RowData();
            IFileReader reader = FileReaderTest.GetReader("\"ABC\"\r\n\"DE\"F\r\n\"G\"HI");
            DelimitedFileParser parser = new DelimitedFileParser(string.Empty, "\r\n");
            parser.TextQualifier = "\"";

            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "ABC" });
            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "DEF" });
            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "GHI" });
        }

        /// <summary>
        ///A test for ParseNextRow
        ///</summary>
        [TestMethod()]
        public void ReadMultipleDataRows()
        {
            RowData rowData = new RowData();
            IFileReader reader = FileReaderTest.GetReader("1,1,1,1\r\n2,2,2,2\r\n3,3,3,3\r\n4,4,4,4");
            DelimitedFileParser parser = new DelimitedFileParser(",", "\r\n");

            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "1", "1", "1", "1" });
            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "2", "2", "2", "2" });
            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "3", "3", "3", "3" });
            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "4", "4", "4", "4" });
        }

        /// <summary>
        ///A test for ParseNextRow
        ///</summary>
        [TestMethod()]
        public void ReadMultipleDataRowsAfterHeader()
        {
            RowData rowData = new RowData();
            IFileReader reader = FileReaderTest.GetReader("\r\n\r\nHeader text\r\n1,1,1,1\r\n2,2,2,2\r\n3,3,3,3\r\n4,4,4,4");
            DelimitedFileParser parser = new DelimitedFileParser(",", "\r\n");
            parser.HeaderRowDelimiter = "\r\n";
            parser.HeaderRowsToSkip = 3;

            parser.SkipHeaderRows(reader);
            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "1", "1", "1", "1" });
            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "2", "2", "2", "2" });
            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "3", "3", "3", "3" });
            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "4", "4", "4", "4" });
        }

        /// <summary>
        ///A test for ParseNextRow
        ///</summary>
        [TestMethod()]
        public void ReadMultipleDataRowsAndBlankLastRow()
        {
            RowData rowData = new RowData();
            IFileReader reader = FileReaderTest.GetReader("1,1,1,1\r\n2,2,2,2\r\n3,3,3,3\r\n4,4,4,4\r\n");
            DelimitedFileParser parser = new DelimitedFileParser(",", "\r\n");

            parser.SkipInitialRows(reader);
            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "1", "1", "1", "1" });
            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "2", "2", "2", "2" });
            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "3", "3", "3", "3" });
            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "4", "4", "4", "4" });
            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[0]);
        }

        [TestMethod()]
        public void TestSkippingRows()
        {
            RowData rowData = new RowData();
            IFileReader reader = FileReaderTest.GetReader("\r\n\r\nHeader\r\nA,B,C,D\r\n1,1,1,1\r\n2,2,2,2\r\n3,3,3,3\r\n4,4,4,4\r\n");
            DelimitedFileParser parser = new DelimitedFileParser(",", "\r\n");
            parser.HeaderRowDelimiter = "\r\n";
            parser.HeaderRowsToSkip = 3;
            parser.DataRowsToSkip = 3;
            parser.ColumnNameInFirstRow = true;

            parser.SkipInitialRows(reader);
            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "4", "4", "4", "4" });
            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[0]);

            Assert.AreEqual<bool>(true, reader.IsEOF);
        }

        [TestMethod()]
        public void TestSkippingTooManyDataRows()
        {
            RowData rowData = new RowData();
            IFileReader reader = FileReaderTest.GetReader("\r\n\r\nHeader\r\nA,B,C,D\r\n1,1,1,1\r\n2,2,2,2\r\n3,3,3,3\r\n4,4,4,4\r\n");
            DelimitedFileParser parser = new DelimitedFileParser(",", "\r\n");
            parser.HeaderRowDelimiter = "\r\n";
            parser.HeaderRowsToSkip = 3;
            parser.DataRowsToSkip = 15;
            parser.ColumnNameInFirstRow = true;

            parser.SkipInitialRows(reader);

            Assert.AreEqual<bool>(true, reader.IsEOF);
        }


        [TestMethod()]
        public void TestSkippingTooManyHeaderRows()
        {
            RowData rowData = new RowData();
            IFileReader reader = FileReaderTest.GetReader("\r\n\r\nHeader\r\nA,B,C,D\r\n1,1,1,1\r\n2,2,2,2\r\n3,3,3,3\r\n4,4,4,4\r\n");
            DelimitedFileParser parser = new DelimitedFileParser(",", "\r\n");
            parser.HeaderRowDelimiter = "\r\n";
            parser.HeaderRowsToSkip = 12;
            parser.DataRowsToSkip = 15;
            parser.ColumnNameInFirstRow = true;

            parser.SkipInitialRows(reader);

            Assert.AreEqual<bool>(true, reader.IsEOF);
        }
        /// <summary>
        ///A test for ParseNextRow
        ///</summary>
        [TestMethod()]
        public void ReadMultipleDataRowsAfterHeaderWithMessyParsing()
        {
            RowData rowData = new RowData();
            IFileReader reader = FileReaderTest.GetReader("\r\n\r\nHeader text\r\n A)::-)brr\"-:-)\r\n-\r\nasdfghj\"-\"\"-\":-)\"-\":-)\"-\"\"-\"\"-\":--\r");
            DelimitedFileParser parser = new DelimitedFileParser(":-)", "-\r\n");
            parser.HeaderRowDelimiter = "\r\n";
            parser.HeaderRowsToSkip = 3;
            parser.TextQualifier = "\"-\"";

            parser.SkipHeaderRows(reader);
            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { " A):", "brr\"-", "\r\n" });
            parser.ParseNextRow(reader, rowData);
            RowDataTest.VerifyParsedRow(rowData, new string[] { "asdfghj\"-\"\"-\"", ":-)\"-\":-" });
        }
    }
}
