using helpmebot6;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace HelpmebotUnitTests
{
    
    
    /// <summary>
    ///This is a test class for GlobalFunctionsTest and is intended
    ///to contain all GlobalFunctionsTest Unit Tests
    ///</summary>
    [TestClass()]
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
        [TestMethod()]
        public void removeItemFromArrayTest()
        {
            string item = "needle";
            string[] array = {"foo", "bar", "baz", "needle", "qux", "quux"};
            string[] arrayExpected = { "foo", "bar", "baz", "qux", "quux" };
            GlobalFunctions.removeItemFromArray(item, ref array);
            Assert.AreEqual(array.Length, arrayExpected.Length);
            for (int i = 0; i < array.Length; i++)
                Assert.AreEqual(array[i], arrayExpected[i]);
        }

        /// <summary>
        ///A test for realArrayLength
        ///</summary>
        [TestMethod()]
        public void realArrayLengthTest()
        {
            string[] args = { "foo", "", "baz", "", "qux", "quux" }; ; 
            int expected = 4;
            int actual;
            actual = GlobalFunctions.realArrayLength(args);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for prefixIsInArray
        ///</summary>
        [TestMethod()]
        public void prefixIsInArrayTest()
        {
            string needlehead = "nd";
            string[] haystack = {"foo", "bar", "ndbaz", "qux"};
            int expected = 2; 
            int actual;
            actual = GlobalFunctions.prefixIsInArray(needlehead, haystack);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for popFromFront
        ///</summary>
        [TestMethod()]
        public void popFromFrontTest()
        {
            string[] list = { "foo", "bar", "baz", "qux", "quux" };
            string[] listExpected = { "bar", "baz", "qux", "quux" };
            string expected = "foo";
            string actual;
            actual = GlobalFunctions.popFromFront(ref list);
            Assert.AreEqual(list.Length, listExpected.Length);
            for (int i = 0; i < list.Length; i++)
                Assert.AreEqual(list[i], listExpected[i]);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for isInArray
        ///</summary>
        [TestMethod()]
        public void isInArrayTest()
        {
            string needle = "needle"; 
            string[] haystack = { "foo", "bar", "baz", "needle", "qux", "quux" }; 
            int expected = 3; 
            int actual;
            actual = GlobalFunctions.isInArray(needle, haystack);
            Assert.AreEqual(expected, actual);
        }
    }
}
