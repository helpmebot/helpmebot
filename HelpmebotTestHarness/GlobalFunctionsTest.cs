using helpmebot6;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace HelpmebotTestHarness
{
    
    
    /// <summary>
    ///This is a test class for GlobalFunctionsTest and is intended
    ///to contain all GlobalFunctionsTest Unit Tests
    ///</summary>
    [TestClass( )]
    public class GlobalFunctionsTest
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
        ///A test for removeItemFromArray
        ///</summary>
        [TestMethod( )]
        public void removeItemFromArrayTest( )
        {
            string item = "foo"; 
            string[ ] array = {"foo", "bar", "baz", "bob", "gill", "bill", "fred"}; 
            string[ ] arrayExpected = { "bar", "baz", "bob", "gill", "bill", "fred" }; 
            GlobalFunctions.removeItemFromArray( item, ref array );
            CollectionAssert.AreEquivalent( arrayExpected, array );
        }

        /// <summary>
        ///A test for RealArrayLength
        ///</summary>
        [TestMethod( )]
        public void RealArrayLengthTest( )
        {
            string[ ] args = { "", "foo", "", "bar" }; 
            int expected = 2; 
            int actual;
            actual = GlobalFunctions.RealArrayLength( args );
            Assert.AreEqual( expected, actual );
        }

        /// <summary>
        ///A test for prefixIsInArray
        ///</summary>
        [TestMethod( )]
        public void prefixIsInArrayTest( )
        {
            string needlehead = "foo";
            string[ ] haystack = { "sgdfsdhsgn", "sdgsdfbsdfvk", "sdsfvdsfbsdfb", "foosdfsbncgn" };
            int expected = 3;
            int actual;
            actual = GlobalFunctions.prefixIsInArray( needlehead, haystack );
            Assert.AreEqual( expected, actual );
        }

        /// <summary>
        ///A test for popFromFront
        ///</summary>
        [TestMethod( )]
        public void popFromFrontTest( )
        {
            string[ ] list = { "1", "2", "3", "4" };
            string[ ] listExpected = { "2", "3", "4" };
            string expected = "1";
            string actual;
            actual = GlobalFunctions.popFromFront( ref list );

            CollectionAssert.AreEquivalent( listExpected, list );
            Assert.AreSame( expected, actual );
        }

        /// <summary>
        ///A test for isInArray
        ///</summary>
        [TestMethod( )]
        public void isInArrayTest( )
        {
            string needle = "foo";
            string[ ] haystack = { "bar", "foo", "baz" };
            int expected = 1;
            int actual;
            actual = GlobalFunctions.isInArray( needle, haystack );
            Assert.AreEqual( expected, actual );
        }

        /// <summary>
        ///A test for commandAccessLevel
        ///</summary>
        [TestMethod( )]
        public void commandAccessLevelTest1( )
        {
            User.userRights expected = User.userRights.Developer; // TODO: Initialize to an appropriate value
            User.userRights actual;
            actual = GlobalFunctions.commandAccessLevel( );
            Assert.AreEqual( expected, actual );
        }
    }
}
