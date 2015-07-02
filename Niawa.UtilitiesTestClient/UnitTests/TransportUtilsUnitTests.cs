using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Niawa.UtilitiesTestClient.UnitTests
{
    [TestFixture]
    public class TransportUtilsUnitTests
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
        public void Test_NU_TU_GetBytes()
        {
            byte[] stringSpaceArray = new byte[] { 0x20, 0x0, 0x20, 0x0, 0x20, 0x0, 0x20, 0x0 };
            string stringSpaces = "    ";

            byte[] result = Niawa.Utilities.TransportUtils.GetBytes(stringSpaces);

            Assert.AreEqual(stringSpaceArray, result, "Byte arrays do not match");

        }

        [Test]
        public void Test_NU_TU_GetString()
        {
            byte[] stringSpaceArray = new byte[] { 0x20, 0x0, 0x20, 0x0, 0x20, 0x0, 0x20, 0x0 };
            string stringSpaces = "    ";

            string result = Niawa.Utilities.TransportUtils.GetString(stringSpaceArray);

            Assert.AreEqual(stringSpaces, result, "Strings do not match");

        }

    }
}
