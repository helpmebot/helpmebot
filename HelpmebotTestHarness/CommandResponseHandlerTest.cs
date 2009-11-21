using helpmebot6;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;

namespace HelpmebotTestHarness
{
    
    
    /// <summary>
    ///This is a test class for CommandResponseHandlerTest and is intended
    ///to contain all CommandResponseHandlerTest Unit Tests
    ///</summary>
    [TestClass( )]
    public class CommandResponseHandlerTest
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
        ///A test for CommandResponseHandler Constructor
        ///</summary>
        [TestMethod( )]
        public void CommandResponseHandlerConstructorTest1( )
        {
            string message = "dsfghmjtr45rtgfretyhgrty5edsfre3";
            CommandResponseHandler target = new CommandResponseHandler( message );
            Assert.AreEqual( ( (CommandResponse)( target.getResponses( )[ 0 ] ) ).Message, message );
        }

        /// <summary>
        ///A test for CommandResponseHandler Constructor
        ///</summary>
        [TestMethod( )]
        public void CommandResponseHandlerConstructorTest( )
        {
            string message = "dsfghmjtr45rtgfretyhgrty5edsfre3"; 
            CommandResponseDestination destination = CommandResponseDestination.CHANNEL_DEBUG;
            CommandResponseHandler target = new CommandResponseHandler( message, destination );
            Assert.AreEqual( ( (CommandResponse)( target.getResponses( )[ 0 ] ) ).Message, message );
            Assert.AreEqual( ( (CommandResponse)( target.getResponses( )[ 0 ] ) ).Destination, destination );

        }
    }
}
