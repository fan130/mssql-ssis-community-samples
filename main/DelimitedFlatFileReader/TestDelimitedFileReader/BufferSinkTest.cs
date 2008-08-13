using Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using System;

namespace TestDelimitedFileReader
{
    
    
    /// <summary>
    ///This is a test class for BufferSinkTest and is intended
    ///to contain all BufferSinkTest Unit Tests
    ///</summary>
    [TestClass()]
    public class BufferSinkTest
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

        private void GenerateOutputColumns(string[] columnNames, bool errorBufferUsed, DTSRowDisposition errorDisposition, DTSRowDisposition truncationDisposition, out OutputTestImpl output, out ComponentBufferServiceTestImpl bufferService)
        {
            output = new OutputTestImpl();
            bufferService = new ComponentBufferServiceTestImpl(columnNames, errorBufferUsed);

            int currentID = 1;

            foreach (string columnName in columnNames)
            {
                IDTSOutputColumn100 outputColumn = output.OutputColumnCollection.New();
                outputColumn.ID = currentID;
                outputColumn.Name = columnName;
                outputColumn.ErrorRowDisposition = errorDisposition;
                outputColumn.TruncationRowDisposition = truncationDisposition;
                currentID++;
            }
        }

        private RowData GenerateRowData(string [] columnData)
        {
            RowData rowData = new RowData();
            foreach (string columnValue in columnData)
            {
                rowData.AddColumnData(columnValue);
            }

            return rowData;
        }

        private void VerifyAddedRowData(ComponentBufferServiceTestImpl bufferService, object [] data)
        {
            for (int i = 0; i < bufferService.ColumnCount; i++)
            {
                if (i < data.Length)
                {
                    Assert.AreEqual(data[i], bufferService.GetColumnData(i));
                }
                else
                {
                    Assert.AreEqual(null, bufferService.GetColumnData(i));
                }
            }
        }

        /// <summary>
        ///A test for BufferSink Constructor
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void BufferSinkConstructorTest1()
        {
            BufferSink target = new BufferSink(null, null, false);
        }

        /// <summary>
        ///A test for BufferSink Constructor
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void BufferSinkConstructorTest2()
        {
            ComponentBufferServiceTestImpl bufferService = new ComponentBufferServiceTestImpl(new string[0], false);
            BufferSink target = new BufferSink(bufferService, null, false);
        }

        /// <summary>
        ///A test for CurrentRowCount
        ///</summary>
        [TestMethod()]
        public void CurrentRowCountTest()
        {
            IDTSOutput100 output = new OutputTestImpl();
            IComponentBufferService bufferService = new ComponentBufferServiceTestImpl(new string[0], false);
            BufferSink target = new BufferSink(bufferService, output, false);
            Assert.AreEqual<Int64>(0, target.CurrentRowCount);
        }

        /// <summary>
        ///A test for AddRow
        ///</summary>
        [TestMethod()]
        public void AddRowTest()
        {
            OutputTestImpl output = null;
            ComponentBufferServiceTestImpl bufferService = null;
            GenerateOutputColumns(new string[] { "A", "B", "C", "D" }, false, DTSRowDisposition.RD_FailComponent, DTSRowDisposition.RD_FailComponent, out output, out bufferService);
            BufferSink target = new BufferSink(bufferService, output, false);
            string[] data = new string[] { "1", "2", "3", "4" };
            RowData rowData = GenerateRowData(data);
            target.AddRow(rowData);

            Assert.AreEqual<Int64>(1, target.CurrentRowCount);
            Assert.AreEqual<int>(1, bufferService.RowCount);
            Assert.AreEqual<int>(0, bufferService.ErrorRowCount);

            VerifyAddedRowData(bufferService, data);
        }

        /// <summary>
        ///A test for AddRow
        ///</summary>
        [TestMethod()]
        public void AddRowTestWithNullData()
        {
            OutputTestImpl output = null;
            ComponentBufferServiceTestImpl bufferService = null;
            GenerateOutputColumns(new string[] { "A", "B", "C", "D" }, false, DTSRowDisposition.RD_FailComponent, DTSRowDisposition.RD_FailComponent, out output, out bufferService);
            BufferSink target = new BufferSink(bufferService, output, true);
            string[] data = new string[] { "1", "", "3", "4" };

            RowData rowData = GenerateRowData(data);
            target.AddRow(rowData);

            Assert.AreEqual<Int64>(1, target.CurrentRowCount);
            Assert.AreEqual<int>(1, bufferService.RowCount);
            Assert.AreEqual<int>(0, bufferService.ErrorRowCount);

            VerifyAddedRowData(bufferService, new object[] { "1", null, "3", "4" });
        }

        /// <summary>
        ///A test for AddRow
        ///</summary>
        [TestMethod()]
        public void AddRowTestWithLessColumns()
        {
            OutputTestImpl output = null;
            ComponentBufferServiceTestImpl bufferService = null;
            GenerateOutputColumns(new string[] { "A", "B", "C", "D" }, false, DTSRowDisposition.RD_FailComponent, DTSRowDisposition.RD_FailComponent, out output, out bufferService);
            BufferSink target = new BufferSink(bufferService, output, true);
            string[] data = new string[] { "1", "", "3" };

            RowData rowData = GenerateRowData(data);
            target.AddRow(rowData);

            Assert.AreEqual<Int64>(1, target.CurrentRowCount);
            Assert.AreEqual<int>(1, bufferService.RowCount);
            Assert.AreEqual<int>(0, bufferService.ErrorRowCount);

            VerifyAddedRowData(bufferService, new object[] { "1", null, "3" });
        }

        /// <summary>
        ///A test for AddRow
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(BufferSinkException))]
        public void AddRowTestWithTooManyColumns()
        {
            OutputTestImpl output = null;
            ComponentBufferServiceTestImpl bufferService = null;
            GenerateOutputColumns(new string[] { "A", "B", "C", "D" }, true, DTSRowDisposition.RD_FailComponent, DTSRowDisposition.RD_FailComponent, out output, out bufferService);
            BufferSink target = new BufferSink(bufferService, output, true);
            string[] data = new string[] { "1", "", "3", "4", "5", "6" };

            RowData rowData = GenerateRowData(data);
            try
            {
                target.AddRow(rowData);
            }
            catch (BufferSinkException ex)
            {
                Assert.AreEqual<Int64>(1, target.CurrentRowCount);
                Assert.AreEqual<int>(0, bufferService.RowCount);
                Assert.AreEqual<int>(0, bufferService.ErrorRowCount);
                throw ex;
            }
        }

        /// <summary>
        ///A test for AddRow
        ///</summary>
        [TestMethod()]
        public void AddRowTestWithTooManyColumnsRedirect()
        {
            OutputTestImpl output = null;
            ComponentBufferServiceTestImpl bufferService = null;
            GenerateOutputColumns(new string[] { "A", "B", "C", "D" }, true, DTSRowDisposition.RD_FailComponent, DTSRowDisposition.RD_FailComponent, out output, out bufferService);
            output.TruncationRowDisposition = DTSRowDisposition.RD_RedirectRow;
            BufferSink target = new BufferSink(bufferService, output, true);
            string[] data = new string[] { "1", "", "3", "4", "5", "6" };

            RowData rowData = GenerateRowData(data);
            target.AddRow(rowData);
            Assert.AreEqual<Int64>(1, target.CurrentRowCount);
            Assert.AreEqual<int>(0, bufferService.RowCount);
            Assert.AreEqual<int>(1, bufferService.ErrorRowCount);
        }

        /// <summary>
        ///A test for AddRow
        ///</summary>
        [TestMethod()]
        public void AddRowTestWithTooManyColumnsIgnore()
        {
            OutputTestImpl output = null;
            ComponentBufferServiceTestImpl bufferService = null;
            GenerateOutputColumns(new string[] { "A", "B", "C", "D" }, true, DTSRowDisposition.RD_FailComponent, DTSRowDisposition.RD_FailComponent, out output, out bufferService);
            output.TruncationRowDisposition = DTSRowDisposition.RD_IgnoreFailure;
            BufferSink target = new BufferSink(bufferService, output, true);
            string[] data = new string[] { "1", "", "3", "4", "5", "6" };

            RowData rowData = GenerateRowData(data);
            target.AddRow(rowData);
            Assert.AreEqual<Int64>(1, target.CurrentRowCount);
            Assert.AreEqual<int>(0, bufferService.RowCount);
            Assert.AreEqual<int>(0, bufferService.ErrorRowCount);
        }

        /// <summary>
        ///A test for AddRow
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(BufferSinkException))]
        public void AddRowTestWithException()
        {
            OutputTestImpl output = null;
            ComponentBufferServiceTestImpl bufferService = null;
            GenerateOutputColumns(new string[] { "A", "B", "C", "D" }, true, DTSRowDisposition.RD_FailComponent, DTSRowDisposition.RD_FailComponent, out output, out bufferService);
            bufferService.ExceptionToFire = new System.Runtime.InteropServices.COMException();
            BufferSink target = new BufferSink(bufferService, output, true);
            string[] data = new string[] { "1", "", "3", "4" };

            RowData rowData = GenerateRowData(data);
            try
            {
                target.AddRow(rowData);
            }
            catch (Exception ex)
            {
                Assert.AreEqual<Int64>(1, target.CurrentRowCount);
                Assert.AreEqual<int>(0, bufferService.RowCount);
                Assert.AreEqual<int>(0, bufferService.ErrorRowCount);
                throw ex;
            }
        }

        /// <summary>
        ///A test for AddRow
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(BufferSinkException))]
        public void AddRowTestWithOverflowException()
        {
            OutputTestImpl output = null;
            ComponentBufferServiceTestImpl bufferService = null;
            GenerateOutputColumns(new string[] { "A", "B", "C", "D" }, true, DTSRowDisposition.RD_FailComponent, DTSRowDisposition.RD_FailComponent, out output, out bufferService);
            bufferService.ExceptionToFire = new DoesNotFitBufferException();
            BufferSink target = new BufferSink(bufferService, output, true);
            // Say we have headers, etc...
            target.CurrentRowCount = 3;
            string[] data = new string[] { "1", "", "3", "4" };

            RowData rowData = GenerateRowData(data);
            try
            {
                target.AddRow(rowData);
            }
            catch (Exception ex)
            {
                Assert.AreEqual<Int64>(4, target.CurrentRowCount);
                Assert.AreEqual<int>(0, bufferService.RowCount);
                Assert.AreEqual<int>(0, bufferService.ErrorRowCount);
                throw ex;
            }
        }

        /// <summary>
        ///A test for AddRow
        ///</summary>
        [TestMethod()]
        public void AddRowTestWithOverflowExceptionRedirect()
        {
            OutputTestImpl output = null;
            ComponentBufferServiceTestImpl bufferService = null;
            GenerateOutputColumns(new string[] { "A", "B", "C", "D" }, true, DTSRowDisposition.RD_FailComponent, DTSRowDisposition.RD_RedirectRow, out output, out bufferService);
            BufferSink target = new BufferSink(bufferService, output, true);
            // Say we have headers, etc...
            target.CurrentRowCount = 3;
            string[] data = new string[] { "1", "", "3", "4" };

            RowData rowData = GenerateRowData(data);
            target.AddRow(rowData);
            Assert.AreEqual<Int64>(4, target.CurrentRowCount);
            Assert.AreEqual<int>(1, bufferService.RowCount);
            Assert.AreEqual<int>(0, bufferService.ErrorRowCount);

            VerifyAddedRowData(bufferService, new object[] { "1", null, "3", "4" });

            bufferService.ExceptionToFire = new OverflowException();

            target.AddRow(rowData);
            Assert.AreEqual<Int64>(5, target.CurrentRowCount);
            Assert.AreEqual<int>(1, bufferService.RowCount);
            Assert.AreEqual<int>(1, bufferService.ErrorRowCount);
        }

        /// <summary>
        ///A test for AddRow
        ///</summary>
        [TestMethod()]
        public void AddRowTestWithOverflowExceptionIgnore()
        {
            OutputTestImpl output = null;
            ComponentBufferServiceTestImpl bufferService = null;
            GenerateOutputColumns(new string[] { "A", "B", "C", "D" }, true, DTSRowDisposition.RD_IgnoreFailure, DTSRowDisposition.RD_RedirectRow, out output, out bufferService);
            bufferService.ExceptionToFire = new ArgumentException();
            BufferSink target = new BufferSink(bufferService, output, true);
            // Say we have headers, etc...
            target.CurrentRowCount = 3;
            string[] data = new string[] { "1", "", "3", "4" };

            RowData rowData = GenerateRowData(data);
            target.AddRow(rowData);
            Assert.AreEqual<Int64>(4, target.CurrentRowCount);
            Assert.AreEqual<int>(0, bufferService.RowCount);
            Assert.AreEqual<int>(0, bufferService.ErrorRowCount);
        }
    }
}
