using helpmebot6;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace HelpmebotUnitTests
{
    
    
    /// <summary>
    ///This is a test class for CommandParserTest and is intended
    ///to contain all CommandParserTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CommandParserTest
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
        ///A test for parseRawLineForMessage
        ///</summary>
        [TestMethod()]
        [DeploymentItem("helpmebot6.exe")]
        public void parseRawLineForMessageTest()
        {
            string message = "!this is a  test"; // TODO: Initialize to an appropriate value
            string messageExpected = "this is a  test"; // TODO: Initialize to an appropriate value
            string nickname = "Helpmebot"; // TODO: Initialize to an appropriate value
            string trigger = "!"; // TODO: Initialize to an appropriate value
            bool actual = CommandParser_Accessor.parseRawLineForMessage(ref message, nickname, trigger);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(true, actual);
        }

        [TestMethod()]
        [DeploymentItem("helpmebot6.exe")]
        public void parseRawLineForMessageTest2()
        {
            string message = "hey folks "; // TODO: Initialize to an appropriate value
            string messageExpected = "hey folks "; // TODO: Initialize to an appropriate value
            string nickname = "Helpmebot"; // TODO: Initialize to an appropriate value
            string trigger = "!"; // TODO: Initialize to an appropriate value
            bool actual = CommandParser_Accessor.parseRawLineForMessage(ref message, nickname, trigger);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(false, actual);
        }

        [TestMethod()]
        [DeploymentItem("helpmebot6.exe")]
        public void parseRawLineForMessageTest3()
        {
            string message = "!helpmebot this is a  test"; // TODO: Initialize to an appropriate value
            string messageExpected = "this is a  test"; // TODO: Initialize to an appropriate value
            string nickname = "Helpmebot"; // TODO: Initialize to an appropriate value
            string trigger = "!"; // TODO: Initialize to an appropriate value
            bool actual = CommandParser_Accessor.parseRawLineForMessage(ref message, nickname, trigger);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(true, actual);
        }
    }
}
