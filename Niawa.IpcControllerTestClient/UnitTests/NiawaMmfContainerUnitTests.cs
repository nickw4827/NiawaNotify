using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Niawa.IpcControllerTestClient.UnitTests
{
    [TestFixture]
    public class NiawaMmfContainerUnitTests
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
        public void Test_NIC_NMC_ToByteArray()
        {
            Niawa.IpcController.NiawaMmfContainer msg = new IpcController.NiawaMmfContainer();
            DateTime dt = new DateTime(2014,12,1);

            msg.RefreshedDate = dt;
            msg.IpcType = "TestIpcType";
            msg.IpcData = "TestIpcData";
            msg.SerialID = "TestSerialID";

            //set to byte array and back to object
            Byte[] bytes = msg.ToByteArray();

            Niawa.IpcController.NiawaMmfContainer msgNew = new IpcController.NiawaMmfContainer(bytes);

            //assertions
            Assert.AreEqual(msg.RefreshedDate, msgNew.RefreshedDate, "Message RefreshedDate doesn't match expectation");
            Assert.AreEqual(msg.IpcType, msgNew.IpcType, "Message IpcType doesn't match expectation");
            Assert.AreEqual(msg.IpcData, msgNew.IpcData, "Message IpcData doesn't match expectation");
            Assert.AreEqual(msg.SerialID, msgNew.SerialID, "Message SerialID doesn't match expectation");

        }

        [Test]
        public void Test_NIC_NMC_Prop_RefreshedDate()
        {
            Niawa.IpcController.NiawaMmfContainer msg = new IpcController.NiawaMmfContainer();
            DateTime dt = new DateTime(2014, 12, 1);

            msg.RefreshedDate = dt;

            //assertion
            Assert.AreEqual(dt, msg.RefreshedDate, "Message RefreshedDate property doesn't match expectation");
            
        }

        [Test]
        public void Test_NIC_NMC_Prop_IpcType()
        {
            Niawa.IpcController.NiawaMmfContainer msg = new IpcController.NiawaMmfContainer();
            msg.IpcType = "TestIpcType";

            //assertion
            Assert.AreEqual("TestIpcType", msg.IpcType, "Message IpcType property doesn't match expectation");

        }

        [Test]
        public void Test_NIC_NMC_Prop_IpcData()
        {
            Niawa.IpcController.NiawaMmfContainer msg = new IpcController.NiawaMmfContainer();
            msg.IpcData = "TestIpcData";

            //assertion
            Assert.AreEqual("TestIpcData", msg.IpcData, "Message IpcData property doesn't match expectation");

        }

        [Test]
        public void Test_NIC_NMC_Prop_SerialID()
        {
            Niawa.IpcController.NiawaMmfContainer msg = new IpcController.NiawaMmfContainer();
            msg.SerialID = "TestSerialID";

            //assertion
            Assert.AreEqual("TestSerialID", msg.SerialID, "Message SerialID property doesn't match expectation");

        }

        


    }
}
