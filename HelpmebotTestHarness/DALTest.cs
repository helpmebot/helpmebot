using helpmebot6;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace HelpmebotTestHarness
{
    
    
    /// <summary>
    ///This is a test class for DALTest and is intended
    ///to contain all DALTest Unit Tests
    ///</summary>
    [TestClass( )]
    public class DALTest
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
        ///A test for buildSelect
        ///</summary>
        [TestMethod( )]
        [DeploymentItem( "helpmebot6.exe" )]
        public void buildSelectTest( )
        {
            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
            DAL_Accessor target = new DAL_Accessor( param0 ); // TODO: Initialize to an appropriate value
            string[ ] select = null; // TODO: Initialize to an appropriate value
            string from = string.Empty; // TODO: Initialize to an appropriate value
            DAL.join[ ] joinConds = null; // TODO: Initialize to an appropriate value
            string[ ] @where = null; // TODO: Initialize to an appropriate value
            string[ ] groupby = null; // TODO: Initialize to an appropriate value
            DAL.order[ ] orderby = null; // TODO: Initialize to an appropriate value
            string[ ] having = null; // TODO: Initialize to an appropriate value
            int limit = 0; // TODO: Initialize to an appropriate value
            int offset = 0; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.buildSelect( select, from, joinConds, @where, groupby, orderby, having, limit, offset );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }
    }
}
