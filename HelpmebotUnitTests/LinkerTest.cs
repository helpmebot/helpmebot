using helpmebot6;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;

namespace HelpmebotUnitTests
{
    
    
    /// <summary>
    ///This is a test class for LinkerTest and is intended
    ///to contain all LinkerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LinkerTest
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


        ///// <summary>
        /////A test for encode
        /////</summary>
        //[TestMethod()]
        //[DeploymentItem("helpmebot6.exe")]
        //public void encodeTest()
        //{
        //    Linker_Accessor target = new Linker_Accessor(); // TODO: Initialize to an appropriate value
        //    string url = "http://helpmebot.org.uk/index.php?this(test)foo@bar;"; // TODO: Initialize to an appropriate value
        //    string expected = "http://helpmebot.org.uk/index.php?this%28test%29foo%40bar%3B"; // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.encode(url);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for antispace
        /////</summary>
        //[TestMethod()]
        //[DeploymentItem("helpmebot6.exe")]
        //public void antispaceTest()
        //{
        //    string source = "this is a test"; // TODO: Initialize to an appropriate value
        //    string expected = "this_is_a_test"; // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = Linker_Accessor.antispace(source);
        //    Assert.AreEqual(expected, actual);
        //}

        ///// <summary>
        /////A test for reallyParseMessage
        /////</summary>
        //[TestMethod()]
        //public void reallyParseMessageFullTest()
        //{
        //    Linker_Accessor target = new Linker_Accessor(); 
        //    string message = "this [[is]] a [[test [[of]] links [[lots and lots]] foo]] {{and}} this is {{some template|with=params}}"; 
        //    ArrayList actual;
        //    actual = target.reallyParseMessage(message);
        //    Assert.AreEqual("is", actual[0]);
        //    Assert.AreEqual("of", actual[1]);
        //    Assert.AreEqual("lots and lots", actual[2]);
        //    Assert.AreEqual("Template:and", actual[3]);
        //    Assert.AreEqual("Template:some template", actual[4]);
        //    Assert.AreEqual(5, actual.Count);
        //}
        ///// <summary>
        /////A test for reallyParseMessage
        /////</summary>
        //[TestMethod()]
        //public void reallyParseMessageSimpleLink()
        //{
        //    Linker_Accessor target = new Linker_Accessor(); 
        //    string message = "this [[is]] a test";
           
        //    ArrayList actual;
        //    actual = target.reallyParseMessage(message);
        //    Assert.AreEqual("is", actual[0]);
        //    Assert.AreEqual(1,actual.Count);
        //}

        ///// <summary>
        /////A test for reallyParseMessage
        /////</summary>
        //[TestMethod()]
        //public void reallyParseMessageNestedLink()
        //{
        //    Linker_Accessor target = new Linker_Accessor();
        //    string message = "this [[is [[a test]] of foo]]";
            
        //    ArrayList actual;
        //    actual = target.reallyParseMessage(message);
        //    Assert.AreEqual("a test", actual[0]);
        //    Assert.AreEqual(1, actual.Count);
        //}

        ///// <summary>
        /////A test for reallyParseMessage
        /////</summary>
        //[TestMethod()]
        //public void reallyParseMessageUnequal()
        //{
        //    Linker_Accessor target = new Linker_Accessor();
        //    string message = "this [[is [[a test]] [[of foo]]";
        //    ArrayList actual;
        //    actual = target.reallyParseMessage(message);
        //    Assert.AreEqual("a test", actual[0]);
        //    Assert.AreEqual("of foo", actual[1]);
        //    Assert.AreEqual(2, actual.Count);
        //}

        ///// <summary>
        /////A test for reallyParseMessage
        /////</summary>
        //[TestMethod()]
        //public void reallyParseMessageSimpleTemplate()
        //{
        //    Linker_Accessor target = new Linker_Accessor();
        //    string message = "this {{is}} a test";
        //    ArrayList actual;
        //    actual = target.reallyParseMessage(message);
        //    Assert.AreEqual("Template:is", actual[0]);
        //    Assert.AreEqual(1, actual.Count);
        //}

        ///// <summary>
        /////A test for reallyParseMessage
        /////</summary>
        //[TestMethod()]
        //public void reallyParseMessageParamsTemplate()
        //{
        //    Linker_Accessor target = new Linker_Accessor();
        //    string message = "this {{is|params=foo}} a test";
        //    ArrayList actual;
        //    actual = target.reallyParseMessage(message);
        //    Assert.AreEqual("Template:is", actual[0]);
        //    Assert.AreEqual(1, actual.Count);
        //}

        ///// <summary>
        /////A test for reallyParseMessage
        /////</summary>
        //[TestMethod()]
        //public void reallyParseMessagePipedLink()
        //{
        //    Linker_Accessor target = new Linker_Accessor();
        //    string message = "this [[is|foo]] a test";

        //    ArrayList actual;
        //    actual = target.reallyParseMessage(message);
        //    Assert.AreEqual("is", actual[0]);
        //    Assert.AreEqual(1, actual.Count);
        //}
    }
}
