using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Niawa.IpcControllerTestClient.UnitTests
{
    [TestFixture]
    public class IpcEventWriterUnitTests
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
        public void Test_NIC_ICW_Start()
        {
            string TEST_IPC_TYPE = "TEST_IPC_TYPE_" + Guid.NewGuid().ToString();
        
            Niawa.IpcController.IpcEventWriter writer = new IpcController.IpcEventWriter("TestIpcEventWriter", TEST_IPC_TYPE, TEST_BUFFER_LENGTH, _utilsBus, false);

            writer.Start();
            System.Threading.Thread.Sleep(100);
      
            //assertion
            Assert.IsTrue(writer.IsStarted, "Writer started value doesn't match expectation after starting writer");

            writer.Stop();

        }

        [Test]
        public void Test_NIC_ICW_Stop()
        {
            string TEST_IPC_TYPE = "TEST_IPC_TYPE_" + Guid.NewGuid().ToString();

            Niawa.IpcController.IpcEventWriter writer = new IpcController.IpcEventWriter("TestIpcEventWriter", TEST_IPC_TYPE, TEST_BUFFER_LENGTH, _utilsBus, false);

            writer.Start();
            System.Threading.Thread.Sleep(100);

            //assertion
            Assert.IsTrue(writer.IsStarted, "Writer started value doesn't match expectation after starting writer");

            writer.Stop();
            System.Threading.Thread.Sleep(100);

            //assertion
            Assert.IsFalse(writer.IsStarted, "Writer started value doesn't match expectation after stopping writer");
        }

        [Test]
        public void Test_NIC_ICW_Write()
        {
            string TEST_IPC_TYPE = "TEST_IPC_TYPE_" + Guid.NewGuid().ToString();

            Niawa.IpcController.IpcEventWriter writer = new IpcController.IpcEventWriter("TestIpcEventWriter", TEST_IPC_TYPE, TEST_BUFFER_LENGTH, _utilsBus, false);
            Niawa.IpcController.IpcEventReader reader = new IpcController.IpcEventReader(TEST_IPC_TYPE, TEST_BUFFER_LENGTH, _utilsBus, false);

            writer.Start();
            reader.Start();
            System.Threading.Thread.Sleep(100);

            //assertions
            Assert.IsTrue(writer.IsStarted, "Writer started value doesn't match expectation after starting writer");
            Assert.IsTrue(reader.IsStarted, "Reader started value doesn't match expectation after starting reader");

            //assertion
            Assert.AreEqual(0, reader.CountMessagesInQueue(), "Count of messages in read buffer expected to be 0 before writing message");

            //write some data
            System.Guid eventGuid = System.Guid.NewGuid();
            DateTime eventDate = DateTime.Now;
            string testAppName = "TestIpcEventWriter";
            string testAppInstance = "TestIpcEventWriterInstance";
            string testData = "test data";
            string testDataDetail = "test data detail";
            string nodeID = "testNodeID";
            string parentNodeID = "testParentNodeID";

            Niawa.IpcController.IpcEvent msg = new IpcController.IpcEvent(eventGuid, eventDate, testAppName, testAppInstance, TEST_IPC_TYPE, testData, testDataDetail, nodeID, parentNodeID);

            writer.Write(msg);
            System.Threading.Thread.Sleep(500);

            //assertion
            Assert.AreEqual(1, reader.CountMessagesInQueue(), "Count of messages in read buffer expected to be 1 after writing message");

            //retrieve message 
            Niawa.IpcController.IpcEvent newMsg = reader.ReadNextEvent();

            Assert.AreEqual(testAppInstance, msg.ApplicationInstance, "Message received from buffer doesn't have expected Application Instance");
            Assert.AreEqual(testAppName, msg.ApplicationName, "Message received from buffer doesn't have expected Application Name");
            Assert.AreEqual(eventDate, msg.EventDate, "Message received from buffer doesn't have expected Event Date");
            Assert.AreEqual(eventGuid, msg.EventID, "Message received from buffer doesn't have expected Event ID");
            Assert.AreEqual(testData, msg.EventMessage, "Message received from buffer doesn't have expected Event Message");
            Assert.AreEqual(testDataDetail, msg.EventMessageDetail, "Message received from buffer doesn't have expected Event Message Detail");
            Assert.AreEqual(TEST_IPC_TYPE, msg.EventType, "Message received from buffer doesn't have expected Event Type");
            Assert.AreEqual(nodeID, msg.NodeID, "Message received from buffer doesn't have expected Node ID");
            Assert.AreEqual(msg.ParentNodeID , parentNodeID, "Message received from buffer doesn't have epxected Parent Node ID");
            
            //stop
            writer.Stop();
            reader.Stop();

        }

        [Test]
        public void Test_NIC_ICW_Prop_IsActive()
        {
            string TEST_IPC_TYPE = "TEST_IPC_TYPE_" + Guid.NewGuid().ToString();

            Niawa.IpcController.IpcEventWriter writer = new IpcController.IpcEventWriter("TestIpcEventWriter", TEST_IPC_TYPE, TEST_BUFFER_LENGTH, _utilsBus, false);

            writer.Start();
            System.Threading.Thread.Sleep(100);

            //assertion
            Assert.IsTrue(writer.IsActive);

            writer.Stop();
            System.Threading.Thread.Sleep(100);

            //assertion
            Assert.IsFalse(writer.IsActive);
        }

        [Test]
        public void Test_NIC_ICW_Prop_IsStarted()
        {
            string TEST_IPC_TYPE = "TEST_IPC_TYPE_" + Guid.NewGuid().ToString();

            Niawa.IpcController.IpcEventWriter writer = new IpcController.IpcEventWriter("TestIpcEventWriter", TEST_IPC_TYPE, TEST_BUFFER_LENGTH, _utilsBus, false);

            writer.Start();
            System.Threading.Thread.Sleep(100);

            //assertion
            Assert.IsTrue(writer.IsStarted);

            writer.Stop();
            System.Threading.Thread.Sleep(100);

            //assertion
            Assert.IsFalse(writer.IsStarted);

        }

        [Test]
        public void Test_NIC_ICW_Prop_IpcType()
        {
            string TEST_IPC_TYPE = "TEST_IPC_TYPE_" + Guid.NewGuid().ToString();

            Niawa.IpcController.IpcEventWriter writer = new IpcController.IpcEventWriter("TestIpcEventWriter", TEST_IPC_TYPE, TEST_BUFFER_LENGTH, _utilsBus, false);

            //assertion
            Assert.AreEqual(TEST_IPC_TYPE, writer.IpcType);
        }

    }
}
