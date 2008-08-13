using System.Text;
using System.IO;
using Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestDelimitedFileReader
{
    
    
    /// <summary>
    ///This is a test class for DelimitedFileReader and is intended
    ///to contain all DelimitedFileReader Unit Tests
    ///</summary>
    [TestClass()]
    public class FileReaderTest
    {
        const string DummyText = @"ABCDFGHJK1234567%^&*()";
        const string TempFileName = "temp.txt";


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
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            FileStream stream = new FileStream(GetTempFileName(), FileMode.Create, FileAccess.ReadWrite);
            StreamWriter writer = new StreamWriter(stream, Encoding.Unicode);

            writer.Write(DummyText);

            writer.Close();
            stream.Close();
        }

        //Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            File.Delete(GetTempFileName());
        }
        
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


        private static string GetTempFileName()
        {
            return System.IO.Path.GetTempPath() + TempFileName;
        }

        internal static IFileReader GetReader(string text)
        {
            IFileReader target = new FileReaderTestImpl(text);

            return target;
        }

        [TestMethod()]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void FileReaderConstructorTest1()
        {
            IFileReader target = new FileReader(string.Empty, Encoding.Unicode);
        }

        [TestMethod()]
        [ExpectedException(typeof(System.ArgumentException))]
        public void FileReaderConstructorTest2()
        {
            IFileReader target = new FileReader("@#<->@#\\BadFileName", Encoding.Unicode);
        }

        [TestMethod()]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void FileReaderConstructorTest3()
        {
            IFileReader target = new FileReader(GetTempFileName(), null);
        }

        [TestMethod()]
        public void GetCharTest()
        {
            FileReader target = new FileReader(GetTempFileName(), Encoding.Unicode);

            for (int i = 0; i < DummyText.Length; i++)
            {
                Assert.AreEqual<char>(DummyText[i], target.GetNextChar());
            }

            target.Close();
        }

        [TestMethod()]
        public void IsEOFTest()
        {
            FileReader target = new FileReader(GetTempFileName(), Encoding.Unicode);

            for (int i = 0; i < DummyText.Length; i++)
            {
                target.GetNextChar();
            }

            target.GetNextChar();
            Assert.AreEqual<bool>(target.IsEOF, true);

            target.Close();
        }

        /// <summary>
        ///A test for CountBOMBytes
        ///</summary>
        [TestMethod()]
        public void CountBOMBytesTest1()
        {
            byte[] byteBuffer = new byte[4];
            Assert.AreEqual<int>(0, FileReader.CountBOMBytes(byteBuffer));
        }

        /// <summary>
        ///A test for CountBOMBytes
        ///</summary>
        [TestMethod()]
        public void CountBOMBytesTest2()
        {
            byte[] byteBuffer = new byte[4];
            byteBuffer[0] = 0xFE;
            byteBuffer[1] = 0xFF;
            Assert.AreEqual<int>(2, FileReader.CountBOMBytes(byteBuffer));
        }

        /// <summary>
        ///A test for CountBOMBytes
        ///</summary>
        [TestMethod()]
        public void CountBOMBytesTest3()
        {
            byte[] byteBuffer = new byte[4];
            byteBuffer[0] = 0xFF;
            byteBuffer[1] = 0xFE;
            Assert.AreEqual<int>(2, FileReader.CountBOMBytes(byteBuffer));
        }

        /// <summary>
        ///A test for CountBOMBytes
        ///</summary>
        [TestMethod()]
        public void CountBOMBytesTest4()
        {
            byte[] byteBuffer = new byte[4];
            byteBuffer[0] = 0xEF;
            byteBuffer[1] = 0xBB;
            byteBuffer[2] = 0xBF;
            Assert.AreEqual<int>(3, FileReader.CountBOMBytes(byteBuffer));
        }

        /// <summary>
        ///A test for CountBOMBytes
        ///</summary>
        [TestMethod()]
        public void CountBOMBytesTest5()
        {
            byte[] byteBuffer = new byte[4];
            byteBuffer[0] = 0;
            byteBuffer[1] = 0;
            byteBuffer[2] = 0xFE;
            byteBuffer[3] = 0xFF;
            Assert.AreEqual<int>(4, FileReader.CountBOMBytes(byteBuffer));
        }

        /// <summary>
        ///A test for CountBOMBytes
        ///</summary>
        [TestMethod()]
        public void CountBOMBytesTest6()
        {
            byte[] byteBuffer = new byte[4];
            byteBuffer[0] = 0;
            byteBuffer[1] = 0;
            byteBuffer[2] = 0x11;
            byteBuffer[3] = 0x33;
            Assert.AreEqual<int>(0, FileReader.CountBOMBytes(byteBuffer));
        }
    }
}
