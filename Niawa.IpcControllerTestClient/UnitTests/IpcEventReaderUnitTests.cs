using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Niawa.IpcControllerTestClient.UnitTests
{
    [TestFixture]
    public class IpcEventReaderUnitTests
    {

        Niawa.Utilities.UtilsServiceBus _utilsBus = null;

        [SetUp]
        public void Setup()
        {
            //create utility bus
            _utilsBus = new Niawa.Utilities.UtilsServiceBus();

        }

        [TearDown]
        public void TearDown()
        {
            _utilsBus = null;

        }

        [Test]
        public void Test_NIC_IER_Start()
        {
            string ipcEventGroup = "Niawa.IpcEvent.UnitTestIPCGroup";

            Niawa.IpcController.IpcEventReader evtReader = new Niawa.IpcController.IpcEventReader(ipcEventGroup
                ,Niawa.IpcController.IpcEvent.IPC_EVENT_MMF_LENGTH, _utilsBus, false);
   
            evtReader.Start();

            //assertion
            Assert.True(evtReader.IsStarted, "Event Reader did not start");

            evtReader.Stop();
            evtReader.Dispose();

        }

        [Test]
        public void Test_NIC_IER_Stop()
        {
            string ipcEventGroup = "Niawa.IpcEvent.UnitTestIPCGroup";

            Niawa.IpcController.IpcEventReader evtReader = new Niawa.IpcController.IpcEventReader(ipcEventGroup
                , Niawa.IpcController.IpcEvent.IPC_EVENT_MMF_LENGTH, _utilsBus, false);

            evtReader.Start();

            //assertion
            Assert.True(evtReader.IsStarted, "Event Reader did not start");

            evtReader.Stop();

            //assertion
            Assert.False(evtReader.IsStarted, "Event Reader did not stop");

            evtReader.Dispose();

        }

        [Test]
        public void Test_NIC_IER_ReadNextEvent()
        {
            string ipcEventGroup = "Niawa.IpcEvent.UnitTestIPCGroup";

            Niawa.IpcController.IpcEventReader evtReader = new Niawa.IpcController.IpcEventReader(ipcEventGroup
                , Niawa.IpcController.IpcEvent.IPC_EVENT_MMF_LENGTH, _utilsBus, false);

            evtReader.Start();

            //assertion
            Assert.True(evtReader.IsStarted, "Event Reader did not start");

            /* WRITER */

            Niawa.IpcController.iEventWriter evtWriter = new Niawa.IpcController.IpcEventWriter("Niawa.IpcControllerTestClient"
                ,ipcEventGroup
                ,Niawa.IpcController.IpcEvent.IPC_EVENT_MMF_LENGTH
                ,_utilsBus, false);
            
            evtWriter.Start();

            //assertion
            Assert.True(evtWriter.IsStarted, "Event Writer did not start");

            string eventType = "testEvent";
            string eventMessage = "eventMessage";
            string eventDetail = "eventDetail";
            string nodeID = "testNodeID";
            string parentNodeID = "testParentNodeID";

            evtWriter.Write(Environment.MachineName, eventType, eventMessage, eventDetail, nodeID, parentNodeID);

            /* /WRITER */

            Niawa.IpcController.IpcEvent evt = evtReader.ReadNextEvent();

            Assert.AreEqual(eventType, evt.EventType, "Read Event Type does not match expectation");
            Assert.AreEqual(eventMessage, evt.EventMessage, "Read Event Message does not match expectation");
            Assert.AreEqual(eventDetail, evt.EventMessageDetail, "Read Event Message Detail does not match expectation");
            Assert.AreEqual(nodeID, evt.NodeID, "Read Event Node ID does not match expectation");
            Assert.AreEqual(parentNodeID, evt.ParentNodeID, "Read Event Parent Node ID does not match expectation");

            //dispose
            evtReader.Stop();
            evtReader.Dispose();

            evtWriter.Stop();
            evtWriter.Dispose();

        }

        [Test]
        public void Test_NIC_IER_Prop_Started_True()
        {
            string ipcEventGroup = "Niawa.IpcEvent.UnitTestIPCGroup";

            Niawa.IpcController.IpcEventReader evtReader = new Niawa.IpcController.IpcEventReader(ipcEventGroup
                , Niawa.IpcController.IpcEvent.IPC_EVENT_MMF_LENGTH, _utilsBus, false);
            
            evtReader.Start();

            //assertion
            Assert.True(evtReader.IsStarted, "Event Reader did not start");

            evtReader.Stop();
            evtReader.Dispose();

        }

        [Test]
        public void Test_NIC_IER_Prop_Started_False()
        {
            string ipcEventGroup = "Niawa.IpcEvent.UnitTestIPCGroup";

            Niawa.IpcController.IpcEventReader evtReader = new Niawa.IpcController.IpcEventReader(ipcEventGroup
                , Niawa.IpcController.IpcEvent.IPC_EVENT_MMF_LENGTH, _utilsBus, false);
            
            evtReader.Start();

            //assertion
            Assert.True(evtReader.IsStarted, "Event Reader did not start");

            evtReader.Stop();

            //assertion
            Assert.False(evtReader.IsStarted, "Event Reader did not stop");

            evtReader.Dispose();

        }

        [Test]
        public void Test_NIC_IER_Prop_IsActive_True()
        {
            string ipcEventGroup = "Niawa.IpcEvent.UnitTestIPCGroup";

            Niawa.IpcController.IpcEventReader evtReader = new Niawa.IpcController.IpcEventReader(ipcEventGroup
                , Niawa.IpcController.IpcEvent.IPC_EVENT_MMF_LENGTH, _utilsBus, false);
            
            evtReader.Start();
            System.Threading.Thread.Sleep(500);

            //assertion
            Assert.True(evtReader.IsStarted, "Event Reader is not active");

            evtReader.Stop();
            evtReader.Dispose();

        }

        [Test]
        public void Test_NIC_IER_Prop_IsActive_False()
        {
            string ipcEventGroup = "Niawa.IpcEvent.UnitTestIPCGroup";

            Niawa.IpcController.IpcEventReader evtReader = new Niawa.IpcController.IpcEventReader(ipcEventGroup
                , Niawa.IpcController.IpcEvent.IPC_EVENT_MMF_LENGTH, _utilsBus, false);
            
            evtReader.Start();
            System.Threading.Thread.Sleep(500);

            //assertion
            Assert.True(evtReader.IsActive, "Event Reader is not active");

            evtReader.Stop();
            System.Threading.Thread.Sleep(500);

            //assertion
            Assert.False(evtReader.IsActive, "Event Reader is still active");

            evtReader.Dispose();

        }

        [Test]
        public void Test_NIC_IER_Prop_IpcType()
        {
            string ipcEventGroup = "Niawa.IpcEvent.UnitTestIPCGroup";

            Niawa.IpcController.IpcEventReader evtReader = new Niawa.IpcController.IpcEventReader(ipcEventGroup
                ,Niawa.IpcController.IpcEvent.IPC_EVENT_MMF_LENGTH, _utilsBus, false);

            //assertion
            Assert.AreEqual(ipcEventGroup, evtReader.IpcType);

        }


    }
}
