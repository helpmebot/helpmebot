using helpmebot6.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using helpmebot6;

namespace HelpmebotTestHarness
{
    
    
    /// <summary>
    ///This is a test class for AccessTest and is intended
    ///to contain all AccessTest Unit Tests
    ///</summary>
    [TestClass( )]
    public class AccessTest
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
        ///A test for execute
        ///</summary>
        [TestMethod( )]
        [DeploymentItem( "helpmebot6.exe" )]
        public void executeTest( )
        {
            Access_Accessor target = new Access_Accessor( ); // TODO: Initialize to an appropriate value
            User source = null; // TODO: Initialize to an appropriate value
            string channel = string.Empty; // TODO: Initialize to an appropriate value
            string[ ] args = null; // TODO: Initialize to an appropriate value
            CommandResponseHandler_Accessor expected = null; // TODO: Initialize to an appropriate value
            CommandResponseHandler_Accessor actual;
            actual = target.execute( source, channel, args );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for delAccessEntry
        ///</summary>
        [TestMethod( )]
        [DeploymentItem( "helpmebot6.exe" )]
        [Ignore()]
        public void delAccessEntryTest( )
        {
            Access_Accessor target = new Access_Accessor( ); // TODO: Initialize to an appropriate value
            int id = 0; // TODO: Initialize to an appropriate value
            target.delAccessEntry( id );
            Assert.Inconclusive( "A method that does not return a value cannot be verified." );
        }

        /// <summary>
        ///A test for addAccessEntry
        ///</summary>
        [TestMethod( )]
        [DeploymentItem( "helpmebot6.exe" )]
        [Ignore()]
        public void addAccessEntryTest( )
        {
            Access_Accessor target = new Access_Accessor( ); // TODO: Initialize to an appropriate value
            User newEntry = null; // TODO: Initialize to an appropriate value
            User.userRights AccessLevel = new User.userRights( ); // TODO: Initialize to an appropriate value
            target.addAccessEntry( newEntry, AccessLevel );
            Assert.Inconclusive( "A method that does not return a value cannot be verified." );
        }
    }
}
