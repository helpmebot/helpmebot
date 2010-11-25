using helpmebot6;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace HelpmebotUnitTests
{
    
    
    /// <summary>
    ///This is a test class for UserTest and is intended
    ///to contain all UserTest Unit Tests
    ///</summary>
    [TestClass()]
    public class UserTest
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
        ///A test for ToString
        ///</summary>
        [TestMethod()]
        public void ToStringTest()
        {
            User target = new User();
            target.nickname = "foo";
            target.username = "bar";
            target.hostname = "baz";
            string expected = "foo!bar@baz";
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [TestMethod()]
        public void ToStringTest2()
        {
            User target = new User();
            target.nickname = "foo";
            target.username = null;
            target.hostname = "baz";
            string expected = "foo@baz";
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [TestMethod()]
        public void ToStringTest3()
        {
            User target = new User();
            target.nickname = "foo";
            target.username = null;
            target.hostname = null;
            string expected = "foo";
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [TestMethod()]
        public void ToStringTest4()
        {
            User target = new User();
            target.nickname = null;
            target.username = null;
            target.hostname = null;
            string expected = "";
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for newFromString
        ///</summary>
        [TestMethod()]
        public void newFromStringTest()
        {
            string source = "foo!bar@baz";
            User expected = new User();
            expected.nickname = "foo";
            expected.username = "bar";
            expected.hostname = "baz";
            User actual;
            actual = User.newFromString(source);
            Assert.AreEqual(expected.nickname, actual.nickname);
            Assert.AreEqual(expected.username, actual.username);
            Assert.AreEqual(expected.hostname, actual.hostname);
        }

        /// <summary>
        ///A test for newFromString
        ///</summary>
        [TestMethod()]
        public void newFromStringTest2()
        {
            string source = "foo@baz";
            User expected = new User();
            expected.nickname = "foo";
            expected.username = null;
            expected.hostname = "baz";
            User actual;
            actual = User.newFromString(source);

            Assert.AreEqual(expected.nickname, actual.nickname);
            Assert.AreEqual(expected.username, actual.username);
            Assert.AreEqual(expected.hostname, actual.hostname);
        }

        /// <summary>
        ///A test for newFromString
        ///</summary>
        [TestMethod()]
        public void newFromStringTest3()
        {
            string source = "foo";
            User expected = new User();
            expected.nickname = "foo";
            expected.username = null;
            expected.hostname = null;
            User actual;
            actual = User.newFromString(source);

            Assert.AreEqual(expected.nickname, actual.nickname);
            Assert.AreEqual(expected.username, actual.username);
            Assert.AreEqual(expected.hostname, actual.hostname);
        }


        /// <summary>
        ///A test for newFromString
        ///</summary>
        [TestMethod()]
        public void newFromStringTest4()
        {
            string source = "";
            User expected = new User();
            expected.nickname = null;
            expected.username = null;
            expected.hostname = null;
            User actual;
            actual = User.newFromString(source);
            Assert.AreEqual(expected.nickname, actual.nickname);
            Assert.AreEqual(expected.username, actual.username);
            Assert.AreEqual(expected.hostname, actual.hostname);
        }
    }
}
