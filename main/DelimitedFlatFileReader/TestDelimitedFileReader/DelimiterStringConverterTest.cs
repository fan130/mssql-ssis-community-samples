using Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using System.Globalization;
using System;

namespace TestDelimitedFileReader
{
    
    
    /// <summary>
    ///This is a test class for DelimiterStringConverterTest and is intended
    ///to contain all DelimiterStringConverterTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DelimiterStringConverterTest
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
        ///A test for ToValueDelim
        ///</summary>
        [TestMethod()]
        [DeploymentItem("DelimitedFileReader.dll")]
        public void ToValueDelimTest()
        {
            Assert.AreEqual<string>("\r\n", DelimiterStringConverter_Accessor.ToValueDelim("{CR}{LF}"));
        }

        /// <summary>
        ///A test for ToReadableDelim
        ///</summary>
        [TestMethod()]
        [DeploymentItem("DelimitedFileReader.dll")]
        public void ToReadableDelimTest()
        {
            Assert.AreEqual<string>("ABC{CR} K{LF}CF{t}ASD", DelimiterStringConverter_Accessor.ToReadableDelim("ABC\r K\nCF\tASD"));
        }

        /// <summary>
        ///A test for ConvertTo
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(System.NotSupportedException))]
        public void ConvertToTest()
        {
            DelimiterStringConverter target = new DelimiterStringConverter();
            target.ConvertTo(null, CultureInfo.CurrentCulture, null, typeof(int));
        }

        /// <summary>
        ///A test for ConvertTo
        ///</summary>
        [TestMethod()]
        public void ConvertToTest1()
        {
            DelimiterStringConverter target = new DelimiterStringConverter();
            Assert.AreEqual<string>("<none>", (string)target.ConvertTo(null, CultureInfo.CurrentCulture, string.Empty, typeof(string)));
        }

        /// <summary>
        ///A test for ConvertTo
        ///</summary>
        [TestMethod()]
        public void ConvertToTest2()
        {
            DelimiterStringConverter target = new DelimiterStringConverter();
            Assert.AreEqual<string>("{CR}{CR}{CR}{t}{LF}{LF} {LF}{LF}", (string)target.ConvertTo(null, CultureInfo.CurrentCulture, "\r\r\r\t\n\n \n\n", typeof(string)));
        }

        /// <summary>
        ///A test for ConvertFrom
        ///</summary>
        [TestMethod()]
        public void ConvertFromTest()
        {
            DelimiterStringConverter target = new DelimiterStringConverter();
            target.ConvertFrom(null, CultureInfo.CurrentCulture, null);
        }

        /// <summary>
        ///A test for ConvertFrom
        ///</summary>
        [TestMethod()]
        public void ConvertFromTest1()
        {
            DelimiterStringConverter target = new DelimiterStringConverter(); 
            Assert.AreEqual<string>(string.Empty, (string)target.ConvertFrom(null, CultureInfo.CurrentCulture, "<none>"));
        }

        /// <summary>
        ///A test for ConvertFrom
        ///</summary>
        [TestMethod()]
        public void ConvertFromTest2()
        {
            DelimiterStringConverter target = new DelimiterStringConverter();
            Assert.AreEqual<string>("\r---\n\tBLAH\r\n", (string)target.ConvertFrom(null, CultureInfo.CurrentCulture, "{CR}---{LF}{t}BLAH{CR}{LF}"));
        }

        /// <summary>
        ///A test for DelimiterStringConverter Constructor
        ///</summary>
        [TestMethod()]
        public void DelimiterStringConverterConstructorTest()
        {
            DelimiterStringConverter target = new DelimiterStringConverter();
        }
    }
}
