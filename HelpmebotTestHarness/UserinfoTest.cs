using helpmebot6.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using helpmebot6;

namespace HelpmebotTestHarness
{
    
    
    /// <summary>
    ///This is a test class for UserinfoTest and is intended
    ///to contain all UserinfoTest Unit Tests
    ///</summary>
    [TestClass( )]
    public class UserinfoTest
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
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            DAL.Singleton( "helpmebot.org.uk", 3306, "helpmebot", new System.IO.StreamReader( "D:\\hmbot.pw" ).ReadLine( ), "helpmebot_v6_dev" );
            Assert.IsTrue( DAL.Singleton( ).Connect( ) );
        }
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
        ///A test for sendShortUserInfo
        ///</summary>
        [TestMethod( )]
        [DeploymentItem( "helpmebot6.exe" )]
        public void sendShortUserInfoTest( )
        {
            Userinfo_Accessor target = new Userinfo_Accessor( ); // TODO: Initialize to an appropriate value
            Userinfo_Accessor.UserInformation userInformation = null; // TODO: Initialize to an appropriate value
            target.sendShortUserInfo( userInformation );
            Assert.Inconclusive( "A method that does not return a value cannot be verified." );
        }

        /// <summary>
        ///A test for sendLongUserInfo
        ///</summary>
        [TestMethod( )]
        [DeploymentItem( "helpmebot6.exe" )]
        public void sendLongUserInfoTest( )
        {
            Userinfo_Accessor target = new Userinfo_Accessor( ); // TODO: Initialize to an appropriate value
            Userinfo_Accessor.UserInformation userInformation = null; // TODO: Initialize to an appropriate value
            target.sendLongUserInfo( userInformation );
            Assert.Inconclusive( "A method that does not return a value cannot be verified." );
        }

        /// <summary>
        ///A test for retrieveUserInformation
        ///</summary>
        [TestMethod( )]
        [DeploymentItem( "helpmebot6.exe" )]
        public void retrieveUserInformationTest( )
        {
            Userinfo_Accessor target = new Userinfo_Accessor( ); // TODO: Initialize to an appropriate value
            string userName = string.Empty; // TODO: Initialize to an appropriate value
            Userinfo_Accessor.UserInformation initial = null; // TODO: Initialize to an appropriate value
            Userinfo_Accessor.UserInformation initialExpected = null; // TODO: Initialize to an appropriate value
            Userinfo_Accessor.UserInformation expected = null; // TODO: Initialize to an appropriate value
            Userinfo_Accessor.UserInformation actual;
            actual = target.retrieveUserInformation( userName, ref initial );
            Assert.AreEqual( initialExpected, initial );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for getUserTalkPageUrl
        ///</summary>
        [TestMethod( )]
        [DeploymentItem( "helpmebot6.exe" )]
        public void getUserTalkPageUrlTest( )
        {
            Userinfo_Accessor target = new Userinfo_Accessor( ); // TODO: Initialize to an appropriate value
            string userName = string.Empty; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.getUserTalkPageUrl( userName );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for getUserPageUrl
        ///</summary>
        [TestMethod( )]
        [DeploymentItem( "helpmebot6.exe" )]
        public void getUserPageUrlTest( )
        {
            Userinfo_Accessor target = new Userinfo_Accessor( ); // TODO: Initialize to an appropriate value
            string userName = string.Empty; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.getUserPageUrl( userName );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for getUserContributionsUrl
        ///</summary>
        [TestMethod( )]
        [DeploymentItem( "helpmebot6.exe" )]
        public void getUserContributionsUrlTest( )
        {
            Userinfo_Accessor target = new Userinfo_Accessor( ); // TODO: Initialize to an appropriate value
            string userName = string.Empty; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.getUserContributionsUrl( userName );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for getBlockLogUrl
        ///</summary>
        [TestMethod( )]
        [DeploymentItem( "helpmebot6.exe" )]
        public void getBlockLogUrlTest( )
        {
            Userinfo_Accessor target = new Userinfo_Accessor( ); // TODO: Initialize to an appropriate value
            string userName = string.Empty; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.getBlockLogUrl( userName );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for execute
        ///</summary>
        [TestMethod( )]
        [DeploymentItem( "helpmebot6.exe" )]
        public void executeTest( )
        {
            Userinfo_Accessor target = new Userinfo_Accessor( );
            User source = User.newFromString( "Stwalkerster!n=stwalker@wikipedia/Stwalkerster" );
            string channel = "##helpmebot";
            string[ ] args = { "@long", "Stwalkerster"};
            CommandResponseHandler_Accessor actual;
            actual = target.execute( source, channel, args );
            Assert.IsTrue( actual.getResponses().Count==8 );
        }

        /// <summary>
        ///A test for Userinfo Constructor
        ///</summary>
        [TestMethod( )]
        public void UserinfoConstructorTest( )
        {
            Userinfo target = new Userinfo( );
            Assert.Inconclusive( "TODO: Implement code to verify target" );
        }
    }
}
