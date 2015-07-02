using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Niawa.ThreadingTestClient.UnitTests
{
    public class ThreadStatusContainerUnitTests
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
        public void Test_NT_TSC_Prop_Description()
        {
            Niawa.Threading.ThreadStatusContainer tsc = new Niawa.Threading.ThreadStatusContainer();
            string testValue = "Test value";

            //set value
            tsc.Description = testValue ;

            //assertion
            Assert.AreEqual(testValue, tsc.Description, "Value doesn't match expectation");

        }

        [Test]
        public void Test_NT_TSC_Prop_NodeID()
        {
            Niawa.Threading.ThreadStatusContainer tsc = new Niawa.Threading.ThreadStatusContainer();

            int serialIDRoot = 123;
            int serialIDSession = 456;
            int serialIDSerial = 789;

            Utilities.SerialId testSerialID = new Utilities.SerialId(serialIDRoot, serialIDSession, serialIDSerial);

            //set value
            tsc.NodeID = testSerialID.ToString();

            //assertion
            Assert.AreEqual(testSerialID.ToString(), tsc.NodeID, "Value doesn't match expectation");

        }

        [Test]
        public void Test_NT_TSC_Prop_ParentNodeID()
        {
            Niawa.Threading.ThreadStatusContainer tsc = new Niawa.Threading.ThreadStatusContainer();

            int serialIDRoot = 123;
            int serialIDSession = 456;
            int serialIDSerial = 789;

            Utilities.SerialId testSerialID = new Utilities.SerialId(serialIDRoot, serialIDSession, serialIDSerial);

            //set value
            tsc.ParentNodeID = testSerialID.ToString();

            //assertion
            Assert.AreEqual(testSerialID.ToString(), tsc.ParentNodeID, "Value doesn't match expectation");

        }

        [Test]
        public void Test_NT_TSC_Prop_Status()
        {
            Niawa.Threading.ThreadStatusContainer tsc = new Niawa.Threading.ThreadStatusContainer();
            string testValue = "Test value";

            //set value
            tsc.Status= testValue;

            //assertion
            Assert.AreEqual(testValue, tsc.Status, "Value doesn't match expectation");

        }

        [Test]
        public void Test_NT_TSC_Prop_StatusDate()
        {
            Niawa.Threading.ThreadStatusContainer tsc = new Niawa.Threading.ThreadStatusContainer();
            DateTime testDate = DateTime.Now;

            //set value
            tsc.StatusDate = testDate;

            //assertion
            Assert.AreEqual(testDate, tsc.StatusDate, "Value doesn't match expectation");

        }

        [Test]
        public void Test_NT_TSC_Prop_Initialized()
        {
            Niawa.Threading.ThreadStatusContainer tsc = new Niawa.Threading.ThreadStatusContainer();
            
            //set value
            tsc.Initialized = true;

            //assertion
            Assert.True(tsc.Initialized, "Value doesn't match expectation");

        }

        [Test]
        public void Test_NT_TSC_Prop_InitializedDate()
        {
            Niawa.Threading.ThreadStatusContainer tsc = new Niawa.Threading.ThreadStatusContainer();
            DateTime testDate = DateTime.Now;

            //set value
            tsc.InitializedDate= testDate;

            //assertion
            Assert.AreEqual(testDate, tsc.InitializedDate, "Value doesn't match expectation");
        }

        [Test]
        public void Test_NT_TSC_Prop_Finalized()
        {
            Niawa.Threading.ThreadStatusContainer tsc = new Niawa.Threading.ThreadStatusContainer();

            //set value
            tsc.Finalized= true;

            //assertion
            Assert.True(tsc.Finalized, "Value doesn't match expectation");

        }

        [Test]
        public void Test_NT_TSC_Prop_FinalizedDate()
        {
            Niawa.Threading.ThreadStatusContainer tsc = new Niawa.Threading.ThreadStatusContainer();
            DateTime testDate = DateTime.Now;

            //set value
            tsc.FinalizedDate = testDate;

            //assertion
            Assert.AreEqual(testDate, tsc.FinalizedDate, "Value doesn't match expectation");

        }

        [Test]
        public void Test_NT_TSC_Prop_ThreadHealthFailed()
        {
            Niawa.Threading.ThreadStatusContainer tsc = new Niawa.Threading.ThreadStatusContainer();

            //set value
            tsc.ThreadHealthFailed = true;

            //assertion
            Assert.True(tsc.ThreadHealthFailed, "Value doesn't match expectation");
        }

        [Test]
        public void Test_NT_TSC_Prop_ThreadHealthDate()
        {
            Niawa.Threading.ThreadStatusContainer tsc = new Niawa.Threading.ThreadStatusContainer();
            DateTime testDate = DateTime.Now;

            //set value
            tsc.ThreadHealthDate= testDate;

            //assertion
            Assert.AreEqual(testDate, tsc.ThreadHealthDate, "Value doesn't match expectation");

        }

        [Test]
        public void Test_NT_TSC_Prop_ThreadActive()
        {
            Niawa.Threading.ThreadStatusContainer tsc = new Niawa.Threading.ThreadStatusContainer();

            //set value
            tsc.ThreadActive = true;

            //assertion
            Assert.True(tsc.ThreadActive, "Value doesn't match expectation");

        }

        [Test]
        public void Test_NT_TSC_Prop_ThreadEnabled()
        {
            Niawa.Threading.ThreadStatusContainer tsc = new Niawa.Threading.ThreadStatusContainer();

            //set value
            tsc.ThreadEnabled = true;

            //assertion
            Assert.True(tsc.ThreadEnabled, "Value doesn't match expectation");

        }

        [Test]
        public void Test_NT_TSC_Prop_ErrorCount()
        {
            Niawa.Threading.ThreadStatusContainer tsc = new Niawa.Threading.ThreadStatusContainer();

            int testValue = 3;

            //set value
            tsc.ErrorCount = testValue;

            //assertion
            Assert.AreEqual(testValue, tsc.ErrorCount, "Value doesn't match expectation");

        }

        [Test]
        public void Test_NT_TSC_Prop_MessageCount()
        {
            Niawa.Threading.ThreadStatusContainer tsc = new Niawa.Threading.ThreadStatusContainer();

            int testValue = 3;

            //set value
            tsc.MessageCount = testValue;

            //assertion
            Assert.AreEqual(testValue, tsc.MessageCount, "Value doesn't match expectation");

        }

        [Test]
        public void Test_NT_TSC_Prop_MessageErrorCount()
        {
            Niawa.Threading.ThreadStatusContainer tsc = new Niawa.Threading.ThreadStatusContainer();

            int testValue = 3;

            //set value
            tsc.MessageErrorCount = testValue;

            //assertion
            Assert.AreEqual(testValue, tsc.MessageErrorCount, "Value doesn't match expectation");

        }

        [Test]
        public void Test_NT_TSC_Prop_LastErrorDate()
        {
            Niawa.Threading.ThreadStatusContainer tsc = new Niawa.Threading.ThreadStatusContainer();
            DateTime testDate = DateTime.Now;

            //set value
            tsc.LastErrorDate= testDate;

            //assertion
            Assert.AreEqual(testDate, tsc.LastErrorDate, "Value doesn't match expectation");

        }

        [Test]
        public void Test_NT_TSC_Prop_LastMessageDate()
        {
            Niawa.Threading.ThreadStatusContainer tsc = new Niawa.Threading.ThreadStatusContainer();
            DateTime testDate = DateTime.Now;

            //set value
            tsc.LastMessageDate= testDate;

            //assertion
            Assert.AreEqual(testDate, tsc.LastMessageDate, "Value doesn't match expectation");

        }

        [Test]
        public void Test_NT_TSC_Prop_LastMessageErrorDate()
        {
            Niawa.Threading.ThreadStatusContainer tsc = new Niawa.Threading.ThreadStatusContainer();
            DateTime testDate = DateTime.Now;

            //set value
            tsc.LastMessageErrorDate= testDate;

            //assertion
            Assert.AreEqual(testDate, tsc.LastMessageErrorDate, "Value doesn't match expectation");

        }

    }
}
