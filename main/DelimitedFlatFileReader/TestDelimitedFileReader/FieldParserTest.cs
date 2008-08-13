using System;
using Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace TestDelimitedFileReader
{
    
    
    /// <summary>
    ///This is a test class for FieldParserTest and is intended
    ///to contain all FieldParserTest Unit Tests
    ///</summary>
    [TestClass()]
    public class FieldParserTest
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

        static void VerifyFieldParsing(FieldParser parser, IFileReader reader, string expectedText)
        {
            parser.ParseNext(reader);
            Assert.AreEqual<string>(expectedText, parser.CurrentText);
        }

        static void VerifySuccessfulFieldParsing(FieldParser parser, IFileReader reader, string expectedText)
        {
            parser.ParseNext(reader);
            Assert.AreEqual<string>(expectedText, parser.CurrentText);
        }

        static void VerifySuccessfulLastRowFieldParsing(FieldParser parser, IFileReader reader, string expectedText)
        {
            parser.ParseNext(reader);
            Assert.AreEqual<bool>(true, parser.RowDelimiterMatch);
            Assert.AreEqual<string>(expectedText, parser.CurrentText);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BadArgumentsTest1()
        {
            FieldParser parser = FieldParser.BuildParserWithSingleDelimiter(string.Empty);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BadArgumentsTest2()
        {
            FieldParser parser = FieldParser.BuildParserWithSingleDelimiterAndQualifier(string.Empty, "\"");
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BadArgumentsTest3()
        {
            FieldParser parser = FieldParser.BuildParserWithSingleDelimiterAndQualifier(",", string.Empty);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BadArgumentsTest4()
        {
            FieldParser parser = FieldParser.BuildParserWithTwoDelimiters(string.Empty, "\r\n");
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BadArgumentsTest5()
        {
            FieldParser parser = FieldParser.BuildParserWithTwoDelimiters(",", string.Empty);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BadArgumentsTest6()
        {
            FieldParser parser = FieldParser.BuildParserWithTwoDelimitersAndQualifier(",", string.Empty, "\"");
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BadArgumentsTest7()
        {
            FieldParser parser = FieldParser.BuildParserWithTwoDelimitersAndQualifier(string.Empty, "\r\n", "\"");
        }
        
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BadArgumentsTest8()
        {
            FieldParser parser = FieldParser.BuildParserWithTwoDelimitersAndQualifier(",", "\r\n", string.Empty);
        }

        [TestMethod()]
        public void UninitializedParserTest()
        {
            FieldParser parser = FieldParser.BuildParserWithTwoDelimitersAndQualifier(",", "\r\n", "\"");
            Assert.AreEqual<bool>(false, parser.RowDelimiterMatch);
            Assert.AreEqual<string>(string.Empty, parser.CurrentText);
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void ParseSimpleTextOneDelimiterTest()
        {
            IFileReader reader = FileReaderTest.GetReader("A,B,C,1,2,3");
            FieldParser parser = FieldParser.BuildParserWithSingleDelimiter(",");

            VerifySuccessfulFieldParsing(parser, reader, "A");
            VerifySuccessfulFieldParsing(parser, reader, "B");
            VerifySuccessfulFieldParsing(parser, reader, "C");
            VerifySuccessfulFieldParsing(parser, reader, "1");
            VerifySuccessfulFieldParsing(parser, reader, "2");
            VerifyFieldParsing(parser, reader, "3");

            Assert.AreEqual(reader.IsEOF, true);
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void ParseSimpleRowsTest()
        {
            IFileReader reader = FileReaderTest.GetReader("A,B,C\r\n1,2,3");
            FieldParser parser = FieldParser.BuildParserWithSingleDelimiter("\r\n");

            VerifySuccessfulFieldParsing(parser, reader, "A,B,C");
            VerifyFieldParsing(parser, reader, "1,2,3");

            Assert.AreEqual(reader.IsEOF, true);
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void ParseSimpleTextTwoDelimitersTest()
        {
            IFileReader reader = FileReaderTest.GetReader("A,B,C\r\n1,2,3");
            FieldParser parser = FieldParser.BuildParserWithTwoDelimiters(",", "\r\n");

            VerifySuccessfulFieldParsing(parser, reader, "A");
            VerifySuccessfulFieldParsing(parser, reader, "B");
            VerifySuccessfulLastRowFieldParsing(parser, reader, "C");
            VerifySuccessfulFieldParsing(parser, reader, "1");
            VerifySuccessfulFieldParsing(parser, reader, "2");
            VerifyFieldParsing(parser, reader, "3");

            Assert.AreEqual(reader.IsEOF, true);
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void ParseDelimitedQualifiedFieldsTest()
        {
            IFileReader reader = FileReaderTest.GetReader("A,\"B,C\r\n1\",2,3\r\n1,2,3,\"4\"");
            FieldParser parser = FieldParser.BuildParserWithTwoDelimitersAndQualifier(",", "\r\n", "\"");

            VerifySuccessfulFieldParsing(parser, reader, "A");
            VerifySuccessfulFieldParsing(parser, reader, "B,C\r\n1");
            VerifySuccessfulFieldParsing(parser, reader, "2");
            VerifySuccessfulLastRowFieldParsing(parser, reader, "3");
            VerifySuccessfulFieldParsing(parser, reader, "1");
            VerifySuccessfulFieldParsing(parser, reader, "2");
            VerifySuccessfulFieldParsing(parser, reader, "3");
            VerifyFieldParsing(parser, reader, "4");

            Assert.AreEqual(reader.IsEOF, true);
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void ParseDelimitedRowsWithQualifiersTest()
        {
            IFileReader reader = FileReaderTest.GetReader("\"A,B,C\r\n1\",2,3\r\n1,2,3,\"4\"");
            FieldParser parser = FieldParser.BuildParserWithSingleDelimiterAndQualifier("\r\n", "\"");

            VerifySuccessfulFieldParsing(parser, reader, "A,B,C\r\n1,2,3");
            VerifyFieldParsing(parser, reader, "1,2,3,\"4\"");

            Assert.AreEqual(reader.IsEOF, true);
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void DelimitedFieldsWithQualifiersErrorTest()
        {
            IFileReader reader = FileReaderTest.GetReader("\"A\",\"B\",\"C\" \r\n\"1\",\"2\" ,3");
            FieldParser parser = FieldParser.BuildParserWithTwoDelimitersAndQualifier(",", "\r\n", "\"");

            VerifySuccessfulFieldParsing(parser, reader, "A");
            VerifySuccessfulFieldParsing(parser, reader, "B");
            VerifySuccessfulFieldParsing(parser, reader, "C ");
            VerifySuccessfulFieldParsing(parser, reader, "1");
            VerifySuccessfulFieldParsing(parser, reader, "2 ");
            VerifyFieldParsing(parser, reader, "3");

            Assert.AreEqual(reader.IsEOF, true);
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void ParseDelimitedFieldsWithQualifiersTest()
        {
            IFileReader reader = FileReaderTest.GetReader("A,\"B,C,D\",1,2,3,\"4\"");
            FieldParser parser = FieldParser.BuildParserWithSingleDelimiterAndQualifier(",", "\"");

            VerifySuccessfulFieldParsing(parser, reader, "A");
            VerifySuccessfulFieldParsing(parser, reader, "B,C,D");
            VerifySuccessfulFieldParsing(parser, reader, "1");
            VerifySuccessfulFieldParsing(parser, reader, "2");
            VerifySuccessfulFieldParsing(parser, reader, "3");
            VerifyFieldParsing(parser, reader, "4");

            Assert.AreEqual(reader.IsEOF, true);
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void ParseDelimitedFieldsWithEscapedQualifiersTest()
        {
            IFileReader reader = FileReaderTest.GetReader("A,\"B,\"\"C\"\",D\",1,2,3,\"4\"");
            FieldParser parser = FieldParser.BuildParserWithSingleDelimiterAndQualifier(",", "\"");

            VerifySuccessfulFieldParsing(parser, reader, "A");
            VerifySuccessfulFieldParsing(parser, reader, "B,\"C\",D");
            VerifySuccessfulFieldParsing(parser, reader, "1");
            VerifySuccessfulFieldParsing(parser, reader, "2");
            VerifySuccessfulFieldParsing(parser, reader, "3");
            VerifyFieldParsing(parser, reader, "4");

            Assert.AreEqual(reader.IsEOF, true);
        }

        [TestMethod()]
        public void ParseDelimitedFieldsWithEscapedMultiCharQualifiersTest()
        {
            IFileReader reader = FileReaderTest.GetReader("A,:\"::\"B,:\"::\":C:\"::\":,D:\":,1,:\"2,3,:\":4:\":");
            FieldParser parser = FieldParser.BuildParserWithSingleDelimiterAndQualifier(",", ":\":");

            VerifySuccessfulFieldParsing(parser, reader, "A");
            VerifySuccessfulFieldParsing(parser, reader, ":\"B,:\":C:\":,D");
            VerifySuccessfulFieldParsing(parser, reader, "1");
            VerifySuccessfulFieldParsing(parser, reader, ":\"2");
            VerifySuccessfulFieldParsing(parser, reader, "3");
            VerifyFieldParsing(parser, reader, "4");

            Assert.AreEqual(reader.IsEOF, true);
        }

        [TestMethod()]
        public void ParseDelimitedFieldsMultiCharacterDelimiterQualifiers()
        {
            IFileReader reader = FileReaderTest.GetReader("A,.,\"B,C,D\",.,1,2,3,.,4,\r\na,.,b");
            FieldParser parser = FieldParser.BuildParserWithTwoDelimitersAndQualifier(",.,", "\r\n", "\"");
            VerifySuccessfulFieldParsing(parser, reader, "A");
            VerifySuccessfulFieldParsing(parser, reader, "B,C,D");
            VerifySuccessfulFieldParsing(parser, reader, "1,2,3");
            VerifySuccessfulLastRowFieldParsing(parser, reader, "4,");
            VerifySuccessfulFieldParsing(parser, reader, "a");
            VerifyFieldParsing(parser, reader, "b");

            Assert.AreEqual(reader.IsEOF, true);
        }

        [TestMethod()]
        public void ParseDelimitedFieldsMultiCharacterDelimiterQualifiersError()
        {
            IFileReader reader = FileReaderTest.GetReader("A,.,\"B,C,D\",.1,2,3,.,4,\r\na,.,b");
            FieldParser parser = FieldParser.BuildParserWithTwoDelimitersAndQualifier(",.,", "\r\n", "\"");
            VerifySuccessfulFieldParsing(parser, reader, "A");
            VerifySuccessfulFieldParsing(parser, reader, "B,C,D,.1,2,3");
            VerifySuccessfulLastRowFieldParsing(parser, reader, "4,");
            VerifySuccessfulFieldParsing(parser, reader, "a");
            VerifyFieldParsing(parser, reader, "b");

            Assert.AreEqual(reader.IsEOF, true);
        }

        [TestMethod()]
        [ExpectedException(typeof(ParsingBufferOverflowException))]
        public void ParsingBufferOverflowTest()
        {
            IFileReader reader = new FileReaderTestImpl("abcdefghjklmn", 100000);
            FieldParser parser = FieldParser.BuildParserWithTwoDelimitersAndQualifier(",", "\r\n", "\"");
            RowParsingContextTestImpl rowParsingContext = new RowParsingContextTestImpl();
            try
            {
                parser.ParseNext(reader, rowParsingContext);
            }
            catch (ParsingBufferOverflowException ex)
            {
                Assert.AreEqual<int>(0, ex.ColumnIndex);
                throw ex;
            }
        }

        private static void TestEmbeddedQualifiers(string textToParse, string[] expectedFields)
        {
            IFileReader reader = FileReaderTest.GetReader(textToParse);
            FieldParser parser = FieldParser.BuildParserWithTwoDelimitersAndQualifier(",", "\r\n", "\"");

            foreach (string fieldData in expectedFields)
            {
                VerifySuccessfulFieldParsing(parser, reader, fieldData);
            }

            Assert.AreEqual(reader.IsEOF, true);
        }

        [TestMethod()]
        public void ParseDelimitedFieldsSingleEmbeddedQualifiers1()
        {
            TestEmbeddedQualifiers("\"a,\"\"b\",c,d", new string [] {"a,\"b", "c", "d"});
        }

        [TestMethod()]
        public void ParseDelimitedFieldsSingleEmbeddedQualifiers2()
        {
            TestEmbeddedQualifiers("\"\"\"a,\"\"b,c\"\"\",d,e", new string[] { "\"a,\"b,c\"", "d", "e" });
        }

        [TestMethod()]
        public void ParseDelimitedFieldsSingleEmbeddedQualifiers3()
        {
            TestEmbeddedQualifiers("a,\"b,c\"\"\",e", new string[] { "a", "b,c\"", "e" });
        }

        [TestMethod()]
        public void ParseDelimitedFieldsSingleEmbeddedQualifiers4()
        {
            TestEmbeddedQualifiers("a,\"\"b,c", new string[] { "a", "b", "c" });
        }

        [TestMethod()]
        public void ParseDelimitedFieldsThreeEmbeddedQualifiers()
        {
            TestEmbeddedQualifiers("\"\"\"a,\"\"b,c\"\"\",d,e", new string[] { "\"a,\"b,c\"", "d", "e" });
        }

        [TestMethod()]
        public void ParseDelimitedFieldsIgnoredEmbeddedQualifiers()
        {
            TestEmbeddedQualifiers("a\"\"\"a,b,c", new string[] { "a\"\"\"a", "b", "c" });
        }

        [TestMethod()]
        public void ParseDelimitedFieldsIgnoreUnmatchedQualifiers()
        {
            TestEmbeddedQualifiers("\"a\"\"\"a\",b,c", new string[] { "a\"a\"", "b", "c" });
        }
    }
}
