using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Niawa.ThreadingTestClient.UnitTests
{
    public class ThreadStatusUnitTests
    {
        private Niawa.Threading.ThreadStatus _threadStatus = null;
        private MockedObjects.MockedEventRaiser _evtRaiser = null;

        private Niawa.Utilities.SerialId _testSerialID = null;

        [SetUp]
        public void Setup()
        {
            int serialIDRoot = 123;
            int serialIDSession = 456;
            int serialIDSerial = 789;

            _testSerialID = new Utilities.SerialId(serialIDRoot, serialIDSession, serialIDSerial);

            //create mock event raiser
            _evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            _threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, _evtRaiser);

        }

        [TearDown]
        public void TearDown()
        {
            _threadStatus = null;
            _evtRaiser = null;

        }

        [Test]
        public void Test_NT_TS_Instantiate()
        {
            DateTime beforeDate = DateTime.Now;

            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            //assertions
            Assert.GreaterOrEqual(threadStatus.InitializedDate, beforeDate, "Initialized Date is incorrect");
            Assert.AreEqual(Niawa.Threading.ThreadStatus.STATUS_UNKNOWN, threadStatus.Status, "Status is incorrect");


        }

        [Test]
        public void Test_NT_TS_RaiseStatusReport()
        {
            string requestor = "TestRequestor";

            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            //raise status report
            threadStatus.RaiseStatusReport(requestor);

            //assertions
            Assert.AreEqual("StatusReport", evtRaiser.MessageType, "Message Type is incorrect");
            Assert.AreEqual(Niawa.Threading.ThreadStatus.STATUS_UNKNOWN, evtRaiser.Message, "Message is incorrect");
            Assert.True(evtRaiser.MessageDetail.ContainsKey("str:Requestor"), "Message Detail does not contain Requestor key");
            Assert.AreEqual(requestor, evtRaiser.MessageDetail["str:Requestor"], "Message Detail does not contain Requestor value");

        }

        [Test]
        public void Test_NT_TS_RaiseThreadHealth()
        {

            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            //raise status report
            threadStatus.RaiseThreadHealth();

            //assertions
            Assert.AreEqual("ThreadHealth", evtRaiser.MessageType, "Message Type is incorrect");
            Assert.AreEqual("Thread Health", evtRaiser.Message, "Message is incorrect");
            Assert.True(evtRaiser.MessageDetail.ContainsKey("str:ThreadHealth"), "Message Detail does not contain ThreadHealth key");
            Assert.True(evtRaiser.MessageDetail.ContainsKey("str:ThreadHealthFailed"), "Message Detail does not contain ThreadHealthFailed key");
            
        }

        [Test]
        public void Test_NT_TS_AddElectiveProperty()
        {
            string requestor = "TestRequestor";
            string testEPKey = "TestElectivePropertyKey";
            string testEPValue = "TestElectivePropertyValue";

            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);
            
            //add elective property
            threadStatus.AddElectiveProperty(testEPKey, testEPValue);

    
            //raise status report
            threadStatus.RaiseStatusReport(requestor);

            //assertions
            Assert.AreEqual("StatusReport", evtRaiser.MessageType, "Message Type is incorrect");
            Assert.True(evtRaiser.MessageDetail.ContainsKey("str:" + testEPKey), "Message Detail does not contain expected key");
            Assert.AreEqual(testEPValue, evtRaiser.MessageDetail["str:" + testEPKey], "Message Detail does not contain expected value");

            
        }

        [Test]
        public void Test_NT_TS_RemoveElectiveProperty()
        {
            string requestor = "TestRequestor";
            
            string testEPKey1 = "TestElectivePropertyKey1";
            string testEPValue1 = "TestElectivePropertyValue1";
            
            string testEPKey2 = "TestElectivePropertyKey2";
            string testEPValue2 = "TestElectivePropertyValue2";

            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            //add elective properties
            threadStatus.AddElectiveProperty(testEPKey1, testEPValue1);
            threadStatus.AddElectiveProperty(testEPKey2, testEPValue2);

            //remove elective property
            threadStatus.RemoveElectiveProperty(testEPKey2);

            
            //raise status report
            threadStatus.RaiseStatusReport(requestor);

            //assertions
            Assert.AreEqual("StatusReport", evtRaiser.MessageType, "Message Type is incorrect");
            Assert.False(evtRaiser.MessageDetail.ContainsKey("str:" + testEPKey2), "Message Detail does not contain expected key");
            
        }

        [Test]
        public void Test_NT_TS_CalculateThreadHealth()
        {
            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            DateTime healthDate = DateTime.Now;

            threadStatus.ThreadHealthDate = healthDate;
            double threadHealth = threadStatus.CalculateThreadHealth();

            Assert.Greater(threadHealth, 0, "Thread Health should be greater than 0");
            Assert.LessOrEqual(threadHealth, 0.1, "Thread Health should be less than 0.1");


        }

        [Test]
        public void Test_NT_TS_CalculateThreadHealthFailed_False()
        {
            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            DateTime healthDate = DateTime.Now;

            threadStatus.ThreadHealthDate = healthDate;
            bool threadHealth = threadStatus.CalculateThreadHealthFailed();

            Assert.False(threadHealth, "ThreadHealth should be false");
            Assert.False(threadStatus.IsThreadHealthFailed, "IsThreadhealthFailed should be false");
            Assert.AreEqual(healthDate, threadStatus.ThreadHealthDate, "Thread Health Date value does not match");

        }

        [Test]
        public void Test_NT_TS_CalculateThreadHealthFailed_True()
        {
            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            DateTime healthDate = DateTime.Now - new TimeSpan(0,5,0);
            
            threadStatus.ThreadHealthDate = healthDate;
            bool threadHealth = threadStatus.CalculateThreadHealthFailed();

            Assert.True(threadHealth, "ThreadHealth should be true");
            Assert.True(threadStatus.IsThreadHealthFailed, "IsThreadHealthFailed should be true");
            Assert.AreEqual(healthDate, threadStatus.ThreadHealthDate, "Thread Health Date value does not match");

        }

        [Test]
        public void Test_NT_TS_ToJson()
        {

        }

        [Test]
        public void Test_NT_TS_Prop_Description()
        {
            _threadStatus.Description = "TestPropertyDescription";

            Assert.AreEqual("TestPropertyDescription", _threadStatus.Description, "Thread Status Description is incorrect");
        }

        [Test]
        public void Test_NT_TS_Prop_Status_Changed()
        {
            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            //change status
            threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STARTED;

            //assertions
            Assert.AreEqual(Niawa.Threading.ThreadStatus.STATUS_STARTED, threadStatus.Status, "Thread Status is incorrect");
            Assert.AreEqual("Status", evtRaiser.MessageType, "Message Type is incorrect");
            Assert.AreEqual(Niawa.Threading.ThreadStatus.STATUS_STARTED, evtRaiser.Message, "Message is incorrect");

        }

        [Test]
        public void Test_NT_TS_Prop_Status_DidNotChange()
        {
            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            //change status
            threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STARTED;

            //clear event raiser
            evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //change status (same value)
            threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STARTED;

            //assertions
            Assert.AreEqual(Niawa.Threading.ThreadStatus.STATUS_STARTED, threadStatus.Status, "Thread Status is incorrect");
            Assert.AreNotEqual("Status", evtRaiser.MessageType, "Message Type is incorrect");
            Assert.AreNotEqual(Niawa.Threading.ThreadStatus.STATUS_STARTED, evtRaiser.Message, "Message is incorrect");

        }

        [Test]
        public void Test_NT_TS_Prop_StatusDate()
        {
            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            DateTime beforeDate = DateTime.Now;

            //change status
            threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STARTED;

            //assertions
            Assert.GreaterOrEqual(threadStatus.StatusDate, beforeDate, "Thread Status Date value is incorrect");
            
        }

        [Test]
        public void Test_NT_TS_Prop_IsInitialized_True()
        {
            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            //set property
            threadStatus.IsInitialized = true;

            //assertion
            Assert.True(threadStatus.IsInitialized, "Thread Status IsInitialized value is incorrect");

        }

        [Test]
        public void Test_NT_TS_Prop_IsInitialized_False()
        {
            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            //set property
            threadStatus.IsInitialized = false;

            //assertion
            Assert.False(threadStatus.IsInitialized, "Thread Status IsInitialized value is incorrect");

        }

        [Test]
        public void Test_NT_TS_Prop_InitializedDate()
        {
            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            DateTime beforeDate = DateTime.Now;

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            //set property
            threadStatus.IsInitialized = true;

            //assertions
            Assert.GreaterOrEqual(threadStatus.InitializedDate, beforeDate, "Thread Status InitializedDate value is incorrect");
        }

        [Test]
        public void Test_NT_TS_Prop_IsFinalized_True()
        {
            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            //set property
            threadStatus.IsFinalized = true;

            //assertion
            Assert.True(threadStatus.IsFinalized, "Thread Status IsFinalized value is incorrect");

        }

        [Test]
        public void Test_NT_TS_Prop_IsFinalized_False()
        {
            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            //set property
            threadStatus.IsFinalized = false;

            //assertion
            Assert.False(threadStatus.IsFinalized, "Thread Status IsFinalized value is incorrect");

        }

        [Test]
        public void Test_NT_TS_Prop_FinalizedDate()
        {
            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            DateTime beforeDate = DateTime.Now;

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            //set property
            threadStatus.IsFinalized = true;

            //assertions
            Assert.GreaterOrEqual(threadStatus.FinalizedDate, beforeDate, "Thread Status FinalizedDate value is incorrect");

        }

        [Test]
        public void Test_NT_TS_Prop_IsThreadActive_True()
        {
            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            //set property
            threadStatus.IsThreadActive = true;

            //assertion
            Assert.True(threadStatus.IsThreadActive, "Thread Status IsThreadActive value is incorrect");

        }

        [Test]
        public void Test_NT_TS_Prop_IsThreadActive_False()
        {
            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            //set property
            threadStatus.IsThreadActive = false;

            //assertion
            Assert.False(threadStatus.IsThreadActive, "Thread Status IsThreadActive value is incorrect");

        }

        [Test]
        public void Test_NT_TS_Prop_IsThreadEnabled_True()
        {
            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            //set property
            threadStatus.IsThreadEnabled = true;

            //assertion
            Assert.True(threadStatus.IsThreadEnabled, "Thread Status IsThreadEnabled value is incorrect");

        }

        [Test]
        public void Test_NT_TS_Prop_IsThreadEnabled_False()
        {
            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            //set property
            threadStatus.IsThreadEnabled = false;

            //assertion
            Assert.False(threadStatus.IsThreadEnabled, "Thread Status IsThreadEnabled value is incorrect");

        }

        [Test]
        public void Test_NT_TS_Prop_ErrorCount()
        {
            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            //set property
            threadStatus.ErrorCount = 3;

            //assertion
            Assert.AreEqual(3, threadStatus.ErrorCount, "Thread Status ErrorCount value is incorrect");

        }

        [Test]
        public void Test_NT_TS_Prop_MessageCount()
        {
            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            //set property
            threadStatus.MessageCount = 4;

            //assertion
            Assert.AreEqual(4, threadStatus.MessageCount, "Thread Status MessageCount value is incorrect");

        }

        [Test]
        public void Test_NT_TS_Prop_MessageErrorCount()
        {
            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            //set property
            threadStatus.MessageErrorCount = 5;

            //assertion
            Assert.AreEqual(5, threadStatus.MessageErrorCount, "Thread Status MessageErrorCount value is incorrect");

        }

        [Test]
        public void Test_NT_TS_Prop_LastErrorDate()
        {
            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            DateTime beforeDate = DateTime.Now;
            //set property
            threadStatus.ErrorCount = 3;

            //assertion
            Assert.GreaterOrEqual(threadStatus.LastErrorDate, beforeDate, "Thread Status LastErrorDate value is incorrect");
        }

        [Test]
        public void Test_NT_TS_Prop_LastMessageDate()
        {
            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            DateTime beforeDate = DateTime.Now;
            //set property
            threadStatus.MessageCount = 4;

            //assertion
            Assert.GreaterOrEqual(threadStatus.LastMessageDate, beforeDate, "Thread Status LastMessageDate value is incorrect");
        }

        [Test]
        public void Test_NT_TS_Prop_LastMessageErrorDate()
        {
            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            DateTime beforeDate = DateTime.Now;
            //set property
            threadStatus.MessageErrorCount = 5;

            //assertion
            Assert.GreaterOrEqual(threadStatus.LastMessageErrorDate, beforeDate, "Thread Status LastMessageErrorDate value is incorrect");

        }

        [Test]
        public void Test_NT_TS_Prop_NodeID()
        {
            int serialIDRoot1 = 123;
            int serialIDSession1 = 456;
            int serialIDSerial1 = 789;

            Utilities.SerialId serialID1 = new Utilities.SerialId(serialIDRoot1, serialIDSession1, serialIDSerial1);

            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, serialID1.ToString(), string.Empty, evtRaiser);

            //assertion
            Assert.AreEqual(serialID1.ToString(), threadStatus.NodeID, "Thread Status NodeID value is incorrect");
        }

        [Test]
        public void Test_NT_TS_Prop_ParentNodeID()
        {
            int serialIDRoot1 = 123;
            int serialIDSession1 = 456;
            int serialIDSerial1 = 789;

            int serialIDRoot2 = 1123;
            int serialIDSession2 = 1456;
            int serialIDSerial2 = 1789;

            Utilities.SerialId serialID1 = new Utilities.SerialId(serialIDRoot1, serialIDSession1, serialIDSerial1);
            Utilities.SerialId serialID2 = new Utilities.SerialId(serialIDRoot2, serialIDSession2, serialIDSerial2);

            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, serialID1.ToString(), serialID2.ToString(), evtRaiser);

            //assertion
            Assert.AreEqual(serialID2.ToString(), threadStatus.ParentNodeID, "Thread Status ParentNodeID value is incorrect");

        }

        //[Test]
        //public void Test_NT_TS_Prop_NodeID()
        //{
        //    int serialIDRoot1 = 123;
        //    int serialIDSession1 = 456;
        //    int serialIDSerial1 = 789;

        //    Utilities.SerialId serialID1 = new Utilities.SerialId(serialIDRoot1, serialIDSession1, serialIDSerial1);

        //    //create mock event raiser
        //    MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

        //    //create thread status
        //    Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, serialID1.ToString(), string.Empty, evtRaiser);

        //    //assertion
        //    Assert.AreEqual(serialID1.ToString(), threadStatus.NodeID, "Thread Status NodeID value is incorrect");

        //}

        //[Test]
        //public void Test_NT_TS_Prop_ParentNodeID()
        //{
        //    int serialIDRoot1 = 123;
        //    int serialIDSession1 = 456;
        //    int serialIDSerial1 = 789;

        //    int serialIDRoot2 = 1123;
        //    int serialIDSession2 = 1456;
        //    int serialIDSerial2 = 1789;

        //    Utilities.SerialId serialID1 = new Utilities.SerialId(serialIDRoot1, serialIDSession1, serialIDSerial1);
        //    Utilities.SerialId serialID2 = new Utilities.SerialId(serialIDRoot2, serialIDSession2, serialIDSerial2);

        //    //create mock event raiser
        //    MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

        //    //create thread status
        //    Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, serialID1.ToString(), serialID2.ToString(), evtRaiser);

        //    //assertion
        //    Assert.AreEqual(serialID2.ToString(), threadStatus.ParentNodeID, "Thread Status ParentNodeID value is incorrect");

        //}

        [Test]
        public void Test_NT_TS_Prop_IsEvtRaiserSet_False()
        {
            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, null);

            //assertion
            Assert.False(threadStatus.IsEvtRaiserSet(), "Thread Status IsEvtRaiserSet value is incorrect");
        }

        [Test]
        public void Test_NT_TS_Prop_IsEvtRaiserSet_True()
        {
            //create mock event raiser
            MockedObjects.MockedEventRaiser evtRaiser = new MockedObjects.MockedEventRaiser("TestAppGroup", "TestAppName", "TestAppInstance", null);

            //create thread status
            Threading.ThreadStatus threadStatus = new Threading.ThreadStatus("TestThreadStatus", 60, _testSerialID.ToString(), string.Empty, evtRaiser);

            //assertion
            Assert.True(threadStatus.IsEvtRaiserSet(), "Thread Status IsEvtRaiserSet value is incorrect");

        }

    }
}
