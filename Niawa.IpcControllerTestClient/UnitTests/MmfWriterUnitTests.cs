using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Niawa.IpcControllerTestClient.UnitTests
{
    public class MmfWriterUnitTests
    {
        private const int TEST_BUFFER_LENGTH = 32000;

        //utility bus
        Niawa.Utilities.UtilsServiceBus _utilsBus = null;

        [SetUp]
        public void Setup()
        {
            _utilsBus = new Niawa.Utilities.UtilsServiceBus();
        }

        [TearDown]
        public void TearDown()
        {
            _utilsBus = null;
        }

        [Test]
        public void Test_NIC_MW_StartWriting()
        {
            string TEST_IPC_TYPE = "TEST_IPC_TYPE_" + Guid.NewGuid().ToString();
        
            Niawa.IpcController.MmfWriter writer = new IpcController.MmfWriter(TEST_IPC_TYPE, TEST_BUFFER_LENGTH, _utilsBus, false);

            writer.StartWriting();
            System.Threading.Thread.Sleep(100);
            
            //assertion
            Assert.AreEqual(Niawa.Threading.ThreadStatus.STATUS_STARTED, writer.ThreadStatus.Status, "Thread status doesn't match expectation after starting writer");

            writer.StopWriting(true);

        }

        [Test]
        public void Test_NIC_MW_StopWriting()
        {
            string TEST_IPC_TYPE = "TEST_IPC_TYPE_" + Guid.NewGuid().ToString();
            
            Niawa.IpcController.MmfWriter writer = new IpcController.MmfWriter(TEST_IPC_TYPE, TEST_BUFFER_LENGTH, _utilsBus, false);

            writer.StartWriting();
            System.Threading.Thread.Sleep(100);
            
            //assertion
            Assert.AreEqual(Niawa.Threading.ThreadStatus.STATUS_STARTED, writer.ThreadStatus.Status, "Thread status doesn't match expectation after starting writer");

            writer.StopWriting(true);
            System.Threading.Thread.Sleep(250);

            //assertion
            Assert.AreEqual(Niawa.Threading.ThreadStatus.STATUS_STOPPED, writer.ThreadStatus.Status, "Thread status doesn't match expectation after stopping writer");

        }

        [Test]
        public void Test_NIC_MW_WriteData()
        {
            string TEST_IPC_TYPE = "TEST_IPC_TYPE_" + Guid.NewGuid().ToString();
            
            Niawa.IpcController.MmfWriter writer = new IpcController.MmfWriter(TEST_IPC_TYPE, TEST_BUFFER_LENGTH, _utilsBus, false);
            Niawa.IpcController.MmfReader reader = new IpcController.MmfReader(TEST_IPC_TYPE, TEST_BUFFER_LENGTH, _utilsBus, false);

            writer.StartWriting();
            reader.StartListening();
            System.Threading.Thread.Sleep(100);
            
            //assertion
            Assert.AreEqual(0, reader.CountMessagesInBuffer(), "Count of messages in buffer expected to be zero before writing message");

            //write some data
            Niawa.IpcController.NiawaMmfContainer msg = new IpcController.NiawaMmfContainer(DateTime.Now, TEST_IPC_TYPE, "test data");
            writer.WriteData(msg);

            System.Threading.Thread.Sleep(1000);

            //assertion
            Assert.AreEqual(1, reader.CountMessagesInBuffer(), "Count of messages in buffer expected to be 1 after writing message");

            //retrieve message
            Niawa.IpcController.NiawaMmfContainer msgNew = reader.GetNextMessageFromBuffer();

            //assertion
            Assert.AreEqual(0, reader.CountMessagesInBuffer(), "Count of messages in buffer expected to be zero after reading message");

            //assertion
            Assert.AreEqual(msg.IpcData, msgNew.IpcData, "Message received from buffer doesn't have the same IpcData");
            Assert.AreEqual(msg.IpcType, msgNew.IpcType, "Message received from buffer doesn't have the same IpcType");
            Assert.AreEqual(msg.RefreshedDate, msgNew.RefreshedDate, "Message received from buffer doesn't have the same RefreshedDate");
            Assert.AreEqual(msg.SerialID, msgNew.SerialID, "Message received from buffer doesn't have the same SerialID");
            Assert.AreEqual(msg.ToString(), msgNew.ToString(), "Message received from buffer doesn't have the same ToString value");

            reader.StopListening(true);
            writer.StopWriting(true);

        }

        [Test]
        public void Test_NIC_MW_Prop_ThreadStatus()
        {
            string TEST_IPC_TYPE = "TEST_IPC_TYPE_" + Guid.NewGuid().ToString();
            
            Niawa.IpcController.MmfWriter writer = new IpcController.MmfWriter(TEST_IPC_TYPE, TEST_BUFFER_LENGTH, _utilsBus, false);

            writer.StartWriting();
            System.Threading.Thread.Sleep(100);
            
            //assertion
            Assert.AreEqual(Niawa.Threading.ThreadStatus.STATUS_STARTED, writer.ThreadStatus.Status, "ThreadStatus property doesn't match expectation");

            writer.StopWriting(true);

        }

        [Test]
        public void Test_NIC_MW_Prop_IpcType()
        {
            string TEST_IPC_TYPE = "TEST_IPC_TYPE_" + Guid.NewGuid().ToString();
            
            Niawa.IpcController.MmfWriter writer = new IpcController.MmfWriter(TEST_IPC_TYPE, TEST_BUFFER_LENGTH, _utilsBus, false);

            //assertion
            Assert.AreEqual(TEST_IPC_TYPE, writer.IpcType, "IpcType property doesn't match expectation");

        }
    }
}
