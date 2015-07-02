using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Niawa.UtilitiesTestClient.UnitTests
{
    [TestFixture]
    public class IdGeneratorUtilsUnitTests
    {

        private Niawa.Utilities.IdGeneratorUtils _idGeneratorUtils = null;

        [SetUp]
        public void Setup()
        {
            _idGeneratorUtils = new Utilities.IdGeneratorUtils();

        }

        [TearDown]
        public void TearDown()
        {
            _idGeneratorUtils = null;

        }

        [Test]
        public void Test_NU_IGU_InitializeSerialId()
        {
            int serialIDRoot = 123;

            //create serial ID
            Niawa.Utilities.SerialId id = _idGeneratorUtils.InitializeSerialId(serialIDRoot);

            Assert.AreEqual(serialIDRoot, id.IdRoot, "SerialID root does not match");
            Assert.AreNotEqual(0, id.IdSession, "SerialID Session should not be zero");
            Assert.AreNotEqual(0, id.IdSerial, "SerialID Serial should not be zero");

        }

        [Test]
        public void Test_NU_IGU_InitializeSerialID_TwoSequential()
        {
            int serialIDRoot = 123;

            //create 2 serial IDs
            Niawa.Utilities.SerialId id1 = _idGeneratorUtils.InitializeSerialId(serialIDRoot);
            Niawa.Utilities.SerialId id2 = _idGeneratorUtils.InitializeSerialId(serialIDRoot);

            Assert.AreNotEqual(id1.IdSession, id2.IdSession, "SerialID1 and SerialID2 Session should not be equal");

        }

        [Test]
        public void Test_NU_IGU_IncrementSerialId()
        {
            int serialIDRoot = 123;
            int serialIDSession = 456;
            int serialIDSerial = 789;

            //increment serial ID from manually created
            Niawa.Utilities.SerialId id = new Utilities.SerialId(serialIDRoot, serialIDSession, serialIDSerial);
            Niawa.Utilities.SerialId idIncremented = Niawa.Utilities.IdGeneratorUtils.IncrementSerialId(id);

            //increment serial ID from auto created
            Niawa.Utilities.SerialId id2 = _idGeneratorUtils.InitializeSerialId(serialIDRoot);
            Niawa.Utilities.SerialId idIncremented2 = Niawa.Utilities.IdGeneratorUtils.IncrementSerialId(id2);

            Assert.AreEqual(id2.IdSerial + 1, idIncremented2.IdSerial, "SerialID Serial incremented value is incorrect");

        }

        [Test]
        public void Test_NU_IGU_IncrementSerialId_Max()
        {
            int serialIDRoot = 123;
            int serialIDSession = 456;
            int serialIDSerial = Int32.MaxValue;

            //increment serial ID from manually created (max value)
            Niawa.Utilities.SerialId id = new Utilities.SerialId(serialIDRoot, serialIDSession, serialIDSerial);
            Niawa.Utilities.SerialId idIncremented = Niawa.Utilities.IdGeneratorUtils.IncrementSerialId(id);

            Assert.AreEqual(1, idIncremented.IdSerial, "SerialID Serial incremented value is incorrect");

        }

    }
}
