using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Niawa.UtilitiesTestClient.UnitTests
{
    [TestFixture]
    public class SerialIdUnitTests
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
        public void Test_NU_S_SerialId()
        {
            int serialIDRoot = 123;
            int serialIDSession = 456;
            int serialIDSerial = 789;
            string serialIDFormatted = "123-456-789";

            //increment serial ID from manually created
            Niawa.Utilities.SerialId id = new Utilities.SerialId(serialIDRoot, serialIDSession, serialIDSerial);

            Assert.AreEqual(serialIDRoot, id.IdRoot, "SerialID Root does not match");
            Assert.AreEqual(serialIDSession, id.IdSession, "SerialID Session does not match");
            Assert.AreEqual(serialIDSerial, id.IdSerial, "SerialID Serial does not match");
            Assert.AreEqual(serialIDFormatted, id.ToString(), "SerialID Formatted does not match");

        }

    }
}
