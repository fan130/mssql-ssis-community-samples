using Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace TestDelimitedFileReader
{
    
    
    /// <summary>
    ///This is a test class for MatchTokenParsingStateTest and is intended
    ///to contain all MatchTokenParsingStateTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MatchTokenParsingStateTest
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
        ///A test for ProcessCharacter
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void EmptyTokenTest()
        {
            MatchTokenParsingState target = new MatchTokenParsingState(string.Empty);
        }

        [TestMethod()]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void EmptyTokenWithEscapingTest()
        {
            MatchEscapedTokenParsingState target = new MatchEscapedTokenParsingState(string.Empty);
        }

        [TestMethod()]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void NullParsingContextTest()
        {
            MatchTokenParsingState target = new MatchTokenParsingState(",");
            target.ProcessCharacter(null, 'a');
        }

        /// <summary>
        ///A test for ProcessCharacter
        ///</summary>
        [TestMethod()]
        public void SingleCharTokenTest()
        {
            ParsingContextTestImpl context = new ParsingContextTestImpl();
            MatchTokenParsingState target = new MatchTokenParsingState(",");
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 'a'), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 'c'), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ','), ParsingResult.Done);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 'x'), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ','), ParsingResult.Done);
        }

        [TestMethod()]
        public void SingleCharTokenWithEscapingTest()
        {
            ParsingContextTestImpl context = new ParsingContextTestImpl();
            MatchEscapedTokenParsingState target = new MatchEscapedTokenParsingState("\"");
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 'a'), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 'c'), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, '\"'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 'x'), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, '\"'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, '\"'), ParsingResult.Done);
        }

        [TestMethod()]
        public void MultiCharTokenTest()
        {
            ParsingContextTestImpl context = new ParsingContextTestImpl();
            MatchTokenParsingState target = new MatchTokenParsingState(",;:!");
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 'a'), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 'c'), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ','), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 'x'), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ','), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ';'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ':'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, '!'), ParsingResult.Done);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 'y'), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ','), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ';'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ':'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 'z'), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ','), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ';'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ':'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, '!'), ParsingResult.Done);
        }

        [TestMethod()]
        public void MultiCharTokenWithEscapingTest()
        {
            ParsingContextTestImpl context = new ParsingContextTestImpl();
            MatchEscapedTokenParsingState target = new MatchEscapedTokenParsingState(":\":");
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 'a'), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 'c'), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ':'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 'x'), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ':'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, '\"'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ':'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 'd'), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ':'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, '\"'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ':'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ':'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ','), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ':'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, '\"'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ':'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ':'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, '\"'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ':'), ParsingResult.Done);
        }

        [TestMethod()]
        public void MultiCharTokenWithNextTest()
        {
            ParsingContextTestImpl context = new ParsingContextTestImpl();
            ParsingStateTestImpl nextState = new ParsingStateTestImpl(ParsingResult.Match);
            MatchTokenParsingState target = new MatchTokenParsingState(",;:!");
            target.AddNextState(nextState);

            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 'a'), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 'c'), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ','), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 'x'), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ','), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ';'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ':'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, '!'), ParsingResult.Match);
            Assert.AreEqual<IParsingState>(nextState, context.CurrentState);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 'y'), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ','), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ';'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ':'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 'z'), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ','), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ';'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ':'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, '!'), ParsingResult.Match);
            Assert.AreEqual<IParsingState>(nextState, context.CurrentState);
        }

        [TestMethod()]
        public void MultiCharTokenWithNextAndErrorTest()
        {
            ParsingContextTestImpl context = new ParsingContextTestImpl();
            ParsingStateTestImpl nextState = new ParsingStateTestImpl(ParsingResult.Match);
            ParsingStateTestImpl errorState = new ParsingStateTestImpl(ParsingResult.Match);

            MatchTokenParsingState target = new MatchTokenParsingState(":=>");
            target.AddNextState(nextState);
            target.SetErrorState(errorState);

            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 'f'), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 't'), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ':'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 'x'), ParsingResult.Miss);
            Assert.AreEqual<IParsingState>(errorState, context.CurrentState);
            Assert.AreEqual<string>(":", context.CurrentText);
            context.ResetText();
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ':'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, '='), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, '>'), ParsingResult.Match);
            Assert.AreEqual<IParsingState>(nextState, context.CurrentState);
            Assert.AreEqual<string>(string.Empty, context.CurrentText);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 'y'), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, 'm'), ParsingResult.Miss);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, ':'), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, '='), ParsingResult.Match);
            Assert.AreEqual<ParsingResult>(target.ProcessCharacter(context, '>'), ParsingResult.Match);
            Assert.AreEqual<IParsingState>(nextState, context.CurrentState);
            Assert.AreEqual<string>(string.Empty, context.CurrentText);
        }
    }
}
