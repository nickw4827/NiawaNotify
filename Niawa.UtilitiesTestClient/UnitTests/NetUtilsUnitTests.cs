using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Niawa.UtilitiesTestClient.UnitTests
{
    [TestFixture]
    public class NetUtilsUnitTests
    {
        [SetUp]
        public void Setup()
        {
            
        }

        [TearDown]
        public void TearDown()
        {
            
        }

        [Test]
        public void Test_NU_NU_FindLanAddress()
        {
            System.Net.IPAddress myAddress = System.Net.IPAddress.Parse("192.168.2.2");

            Assert.AreEqual(myAddress, Niawa.Utilities.NetUtils.FindLanAddress(), "IP Addresses do not match");

        }


    }
}
