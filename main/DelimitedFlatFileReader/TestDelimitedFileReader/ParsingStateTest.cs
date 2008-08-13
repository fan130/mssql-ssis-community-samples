using System;
using Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace TestDelimitedFileReader
{
    
    
    /// <summary>
    ///This is a test class for ParsingStateTest and is intended
    ///to contain all ParsingStateTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ParsingStateTest
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddNextStateNullTest()
        {
            ParsingState target = new ParsingState();
            target.SetErrorState(null);
            target.AddNextState(null);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullParsingContextTest()
        {
            ParsingState target = new ParsingState();
            target.ProcessCharacter(null, 'a');
        }

        /// <summary>
        ///A test for ProcessCharacter
        ///</summary>
        [TestMethod()]
        public void ProcessRegularCharacterTest()
        {
            ParsingContextTestImpl context = new ParsingContextTestImpl();
            ParsingState target = new ParsingState();
            FileReaderTestImpl reader = new FileReaderTestImpl(50);
            System.Text.StringBuilder localText = new System.Text.StringBuilder();
            while (!reader.IsEOF)
            {
                char nextChar = reader.GetNextChar();
                localText.Append(nextChar);
                Assert.AreEqual(target.ProcessCharacter(context, nextChar), ParsingResult.Match);
                Assert.AreEqual<string>(localText.ToString(), context.CurrentText);
            }
        }

        /// <summary>
        ///A test for ProcessCharacter
        ///</summary>
        [TestMethod()]
        public void ProcessCharacterWithLinkedStateTest()
        {
            TestLinkedStates(new ParsingResult[] { ParsingResult.Match }, 0);
            TestLinkedStates(new ParsingResult[] { ParsingResult.Miss }, -1);
            TestLinkedStates(new ParsingResult[] { ParsingResult.Match, ParsingResult.Miss }, 0);
            TestLinkedStates(new ParsingResult[] { ParsingResult.Miss, ParsingResult.Miss, ParsingResult.Match }, 2);
            TestLinkedStates(new ParsingResult[] { ParsingResult.Miss, ParsingResult.Miss, ParsingResult.Miss }, -1);
            TestLinkedStates(new ParsingResult[] { ParsingResult.Miss, ParsingResult.Done, ParsingResult.Match }, 1);
        }

        private static void TestLinkedStates(ParsingResult [] returnResults, int transitionIndex)
        {
            ParsingState target = new ParsingState();
            ParsingResult finalResult = ParsingResult.Match;
            IParsingState expectedState = target;

            for (int i=0; i< returnResults.Length; i++)
            {
                ParsingResult result = returnResults[i];
                ParsingStateTestImpl nextState = new ParsingStateTestImpl(result);
                target.AddNextState(nextState);
                if (i == transitionIndex)
                {
                    finalResult = result;
                    if (finalResult != ParsingResult.Miss)
                    {
                        expectedState = nextState;
                    }
                    else
                    {
                        expectedState = target;
                    }
                }
            }

            ParsingContextTestImpl context = new ParsingContextTestImpl();
            context.CurrentState = target;
            char randomChar = (char)(new Random().Next(char.MinValue, char.MaxValue));
            Assert.AreEqual(target.ProcessCharacter(context, randomChar), finalResult);
            Assert.AreEqual<IParsingState>(expectedState, context.CurrentState);
        }
    }
}
