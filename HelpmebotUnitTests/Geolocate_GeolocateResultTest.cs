using helpmebot6.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace HelpmebotUnitTests
{
    
    
    /// <summary>
    ///This is a test class for Geolocate_GeolocateResultTest and is intended
    ///to contain all Geolocate_GeolocateResultTest Unit Tests
    ///</summary>
    [TestClass()]
    public class Geolocate_GeolocateResultTest
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
        ///Tests no location data
        ///</summary>
        [TestMethod()]
        public void ToStringTestNoLocation()
        {
            Geolocate.GeolocateResult target = new Geolocate.GeolocateResult();
            target.longitude = -2.44f;
            target.latitude = 54.19f;
            target.city = "";
            target.region = "";
            target.country = "";

            string expected = "Latitude: 54.19N, Longitude: -2.44E";
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///Tests country only data
        ///</summary>
        [TestMethod()]
        public void ToStringTestCountryLocation()
        {
            Geolocate.GeolocateResult target = new Geolocate.GeolocateResult();
            target.longitude = -2.44f;
            target.latitude = 54.19f;
            target.country = "Latvia";
            target.city = "";
            target.region = "";

            string expected = "Latitude: 54.19N, Longitude: -2.44E (Estimated location: Latvia)";
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///Tests all data
        ///</summary>
        [TestMethod()]
        public void ToStringTestAllLocation()
        {
            Geolocate.GeolocateResult target = new Geolocate.GeolocateResult();
            target.longitude = -2.44f;
            target.latitude = 54.19f;
            target.country = "Latvia";
            target.city = "Buenos Aires";
            target.region = "California";

            string expected = "Latitude: 54.19N, Longitude: -2.44E (Estimated location: Buenos Aires, California, Latvia)";
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
        }
    }
}
