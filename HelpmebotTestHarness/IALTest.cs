using helpmebot6;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
namespace HelpmebotTestHarness
{
    
    
    /// <summary>
    ///This is a test class for IALTest and is intended
    ///to contain all IALTest Unit Tests
    ///</summary>
    [TestClass( )]
    public class IALTest
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
        ///A test for wrapCTCP
        ///</summary>
        [TestMethod( )]
        public void wrapCTCPTest( )
        {
            System.Text.ASCIIEncoding asc = new System.Text.ASCIIEncoding( );
            byte[ ] ctcp = { Convert.ToByte( 1 ) };

            string command = "PING"; // TODO: Initialize to an appropriate value
            string parameters = "12fvpddlpwf"; // TODO: Initialize to an appropriate value
            string expected = asc.GetString( ctcp ) + "PING 12fvpddlpwf" + asc.GetString( ctcp ); // TODO: Initialize to an appropriate value
            string actual;
            actual = IAL.wrapCTCP( command, parameters );
            Assert.AreEqual( expected, actual );
        }

        /// <summary>
        ///A test for basicParser
        ///</summary>
        [TestMethod( )]
        [DeploymentItem( "helpmebot6.exe" )]
        public void basicParserTest( )
        {
            string line = ":Helpmebot!i=stwalker@wikipedia/Stwalkerster/bot/Helpmebot PRIVMSG #wikipedia-en-help :+3 protected edit requests: [[Talk:Kedco]], [[Template talk:Cite web]], [[Talk:Thierry Henry]].";
            string source = string.Empty;
            string sourceExpected = "Helpmebot!i=stwalker@wikipedia/Stwalkerster/bot/Helpmebot";
            string command = string.Empty; 
            string commandExpected = "PRIVMSG";
            string parameters = string.Empty;
            string parametersExpected = "#wikipedia-en-help :+3 protected edit requests: [[Talk:Kedco]], [[Template talk:Cite web]], [[Talk:Thierry Henry]].";
            IAL_Accessor.basicParser( line, ref source, ref command, ref parameters );
            Assert.AreEqual( sourceExpected, source );
            Assert.AreEqual( commandExpected, command );
            Assert.AreEqual( parametersExpected, parameters );
        }
    }
}
