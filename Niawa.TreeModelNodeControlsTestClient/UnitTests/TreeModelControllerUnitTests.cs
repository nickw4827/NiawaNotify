using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Niawa.TreeModelNodeControlsTestClient.UnitTests
{
    public class TreeModelControllerUnitTests
    {
        private Niawa.TreeModelNodeControls.TreeModelController _controller = null;
        TestTreeModelViewImpl _view = null;
        TestTreeModelNodeViewFactoryImpl _nodeViewFactory = null;

        [SetUp]
        public void Setup()
        {
            _view = new TestTreeModelViewImpl();
            _nodeViewFactory = new TestTreeModelNodeViewFactoryImpl();

        }

        [TearDown]
        public void TearDown()
        {

        }

        [Test]
        public void Test_NTMNC_TMC_AddEventToQueue()
        {
            
            //start up controller
            _controller = new TreeModelNodeControls.TreeModelController(_view, _nodeViewFactory, null, "", "");
            _controller.Start();

            //create event
            IpcController.IpcEvent evt = new IpcController.IpcEvent(new Guid()
                    , DateTime.Now
                    , "TestApp"
                    , "TestInstance"
                    , "TestEventType"
                    , "Test Event Msg"
                    , "Test Event Msg Detail"
                    , "Test Node ID"
                    , "Test Parent Node ID");

            TestTreeModelEventImpl evnt = new TestTreeModelEventImpl(evt, "1", "TestNodeText", "2");
            
            //add to queue
            _controller.AddEventToQueue(evnt);

            System.Threading.Thread.Sleep(1000);

            //assertions
            Niawa.TreeModelNodeControls.TreeModelNode node = _controller.TreeModel.GetNode("1");
            Assert.AreEqual("1", node.NodeID, "Node created from event NodeID doesn't match expectation");
            Assert.AreEqual("2", node.ParentNodeID, "Node created from event ParentNodeID doesn't match expectation");
            Assert.AreEqual("TestNodeText", node.NodeText, "Node created from event NodeText doesn't match expectation");

            TestTreeModelNodeViewImpl view = (TestTreeModelNodeViewImpl) _controller.TreeModel.GetNode("1").NodeView;
            Assert.AreEqual(evnt, view.LatestEvent, "Event added to node doesn't match expectation when Latest Event interrogated");

            //cleanup
            _controller.Stop();
            _controller.Dispose();

        }

        [Test]
        public void Test_NTMNC_TMC_Start()
        {
            //start up controller
            _controller = new TreeModelNodeControls.TreeModelController(_view, _nodeViewFactory, null, "", "");
            _controller.Start();

            System.Threading.Thread.Sleep(1000);

            //assertion
            Assert.IsTrue(_controller.IsThreadRunning);

            //cleanup
            _controller.Stop();
            _controller.Dispose();

        }

        [Test]
        public void Test_NTMNC_TMC_Stop()
        {
            //start up controller
            _controller = new TreeModelNodeControls.TreeModelController(_view, _nodeViewFactory, null, "", "");
            _controller.Start();

            System.Threading.Thread.Sleep(1000);

            //assertion
            Assert.IsTrue(_controller.IsThreadRunning);

            _controller.Stop();

            System.Threading.Thread.Sleep(1000);

            //assertion
            Assert.IsFalse(_controller.IsThreadRunning);

            //cleanup
            _controller.Dispose();

        }

    }
}
