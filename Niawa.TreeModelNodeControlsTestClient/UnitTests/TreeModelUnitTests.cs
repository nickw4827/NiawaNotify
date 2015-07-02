using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Niawa.TreeModelNodeControlsTestClient.UnitTests
{
    [TestFixture]
    public class TreeModelUnitTests
    {
        TestTreeModelViewImpl _view = null;
        Niawa.TreeModelNodeControls.TreeModel _treeModel = null;
        TestTreeModelNodeViewFactoryImpl _nodeViewFactory = null;


        [SetUp]
        public void Setup()
        {
            _view = new TestTreeModelViewImpl();
            _treeModel = new Niawa.TreeModelNodeControls.TreeModel(_view, null, "");
            _nodeViewFactory = new TestTreeModelNodeViewFactoryImpl();

        }

        [TearDown]
        public void TearDown()
        {
            _treeModel = null;
            _view = null;

        }

        [Test]
        public void Test_TMNC_TM_AddNode()
        {
            //create node
            Niawa.TreeModelNodeControls.TreeModelNode node = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node.NodeView = _nodeViewFactory.CreateNodeView("");
            node.NodeID = "1";
            node.ParentNodeID = "0";

            //add node
            _treeModel.AddNode(node);

            //assertion
            Niawa.TreeModelNodeControls.TreeModelNode node2 = _treeModel.GetNode("1");
            Assert.AreEqual(node, node2, "Value doesn't match expectation");

            //cleanup
            _treeModel.RemoveNode("1");

        }

        [Test]
        public void Test_TMNC_TM_RemoveNode()
        {
            //create node
            Niawa.TreeModelNodeControls.TreeModelNode node = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node.NodeView = _nodeViewFactory.CreateNodeView("");
            node.NodeID = "2";
            node.ParentNodeID = "0";

            //add node
            _treeModel.AddNode(node);

            //assertion
            Niawa.TreeModelNodeControls.TreeModelNode node2 = _treeModel.GetNode("2");
            Assert.AreEqual(node, node2, "Value doesn't match expectation");

            //cleanup
            _treeModel.RemoveNode("2");

            //assertion
            Assert.IsFalse(_treeModel.DoesNodeExist("2"));

        }

        [Test]
        public void Test_TMNC_TM_SelectNode()
        {
            //create node
            Niawa.TreeModelNodeControls.TreeModelNode node = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node.NodeView = _nodeViewFactory.CreateNodeView("");
            node.NodeID = "3";
            node.ParentNodeID = "0";

            //add node
            _treeModel.AddNode(node);

            //assertion
            Niawa.TreeModelNodeControls.TreeModelNode node2 = _treeModel.GetNode("3");
            Assert.AreEqual(node, node2, "Failed to add node");

            _treeModel.SelectNode("3");

            //assertions
            TestTreeModelNodeViewImpl viewImpl = (TestTreeModelNodeViewImpl) node.NodeView;

            Assert.IsTrue(viewImpl.Active, "Node view is not active after being selected");
            
            //cleanup
            _treeModel.RemoveNode("3");

        }

        [Test]
        public void Test_TMNC_TM_DoesNodeExist()
        {

            //create node
            Niawa.TreeModelNodeControls.TreeModelNode node = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node.NodeView = _nodeViewFactory.CreateNodeView("");
            node.NodeID = "4";
            node.ParentNodeID = "0";

            //add node
            _treeModel.AddNode(node);

            //assertions
            Assert.IsTrue(_treeModel.DoesNodeExist("4"), "Node does not exist when expected");
            Assert.IsFalse(_treeModel.DoesNodeExist("44"), "Node exists when not expected");

            //cleanup
            _treeModel.RemoveNode("4");

        }

        [Test]
        public void Test_TMNC_TM_GetNode()
        {
            //create node
            Niawa.TreeModelNodeControls.TreeModelNode node = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node.NodeView = _nodeViewFactory.CreateNodeView("");
            node.NodeID = "5";
            node.ParentNodeID = "0";

            //add node
            _treeModel.AddNode(node);

            //assertion
            Niawa.TreeModelNodeControls.TreeModelNode node2 = _treeModel.GetNode("5");
            Assert.AreEqual(node, node2, "Failed to get node");

            //cleanup
            _treeModel.RemoveNode("5");

        }

        [Test]
        public void Test_TMNC_TM_HasNodeTextChanged()
        {
            //create node
            Niawa.TreeModelNodeControls.TreeModelNode node = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node.NodeView = _nodeViewFactory.CreateNodeView("");
            node.NodeID = "6";
            node.ParentNodeID = "0";
            node.NodeText = "Test Text Changed";

            //add node
            _treeModel.AddNode(node);

            //assertions
            Assert.IsFalse(_treeModel.HasNodeTextChanged("6", "Test Text Changed"), "Node text changed but was expected to be the same");
            Assert.IsTrue(_treeModel.HasNodeTextChanged("6", "Test Text Changed 2"), "Node text is the same but expected to change");


            //cleanup
            _treeModel.RemoveNode("6");

        }

        [Test]
        public void Test_TMNC_TM_UpdateNodeText()
        {
            //create node
            Niawa.TreeModelNodeControls.TreeModelNode node = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node.NodeView = _nodeViewFactory.CreateNodeView("");
            node.NodeID = "8";
            node.ParentNodeID = "0";
            node.NodeText = "Test Text Changed 3";

            //add node
            _treeModel.AddNode(node);

            //assertions
            Assert.IsFalse(_treeModel.HasNodeTextChanged("8", "Test Text Changed 3"));
            Assert.IsTrue(_treeModel.HasNodeTextChanged("8", "Test Text Changed 4"));

            //change node text
            _treeModel.UpdateNodeText("8", "Test Text Changed 4");

            //assertions
            Assert.IsTrue(_treeModel.HasNodeTextChanged("8", "Test Text Changed 3"));
            Assert.IsFalse(_treeModel.HasNodeTextChanged("8", "Test Text Changed 4"));

            //cleanup
            _treeModel.RemoveNode("8");

        }

        [Test]
        public void Test_TMNC_TM_UpdateViewUnorphanedNodes()
        {
            //create node
            Niawa.TreeModelNodeControls.TreeModelNode node = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node.NodeView = _nodeViewFactory.CreateNodeView("");
            node.NodeID = "9";
            node.ParentNodeID = "10";

            //add node
            _treeModel.AddNode(node);

            //update view 
            //_treeModel.UpdateViewUnorphanedNodes();

            //assertions
            Assert.IsTrue(node.IsOrphaned, "Node IsOrphaned was false but expected to be true");
            Assert.IsFalse(node.IsUnorphaned, "Node IsUnorphaned was true but expected to be false");
            Assert.IsFalse(node.UnorphanedHistory, "Node UnorphanedHistory was true but expected to be false");
            Assert.IsTrue(_treeModel.ITreeModelView.IsNodeOrphaned("9"), "IsNodeOrphaned at Tree Model View was false but expected to be true");
            
            /*unorphan node*/
            //create node
            Niawa.TreeModelNodeControls.TreeModelNode node2 = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node2.NodeView = _nodeViewFactory.CreateNodeView("");
            node2.NodeID = "10";
            node2.ParentNodeID = "0";

            //add node
            _treeModel.AddNode(node2);

            //update view
            //_treeModel.UpdateViewUnorphanedNodes();

            //assertion
            Assert.IsFalse(node.IsOrphaned, "Node IsOrphaned was true but expected to be false");
            Assert.IsFalse(node.IsUnorphaned, "Node IsUnorphaned was true but expected to be false");
            Assert.IsTrue(node.UnorphanedHistory, "Node UnorphanedHistory was false but expected to be true");
            Assert.IsFalse(_treeModel.ITreeModelView.IsNodeOrphaned("9"), "IsNodeOrphaned at Tree Model View was true but expected to be false");

            //cleanup
            _treeModel.RemoveNode("9");
            _treeModel.RemoveNode("10");

        }

        [Test]
        public void Test_TMNC_TM_UpdateViewDisabledNodes()
        {
            //create node
            Niawa.TreeModelNodeControls.TreeModelNode node = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node.NodeView = _nodeViewFactory.CreateNodeView("");
            node.NodeID = "11";
            node.ParentNodeID = "0";
            node.NodeText = "Test Node Text";

            //add node
            _treeModel.AddNode(node);

            //assertions
            Assert.IsFalse(node.IsDisabled, "Node IsDisabled was true but expected to be false");
            Assert.IsFalse(_treeModel.ITreeModelView.IsNodeDisabled("11"), "Node is disabled at the view but expected to be enabled");

            /*disable node*/
            //create node
            Niawa.TreeModelNodeControls.TreeModelNode node2 = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node2.NodeView = _nodeViewFactory.CreateNodeView("");
            node2.NodeID = "12";
            node2.ParentNodeID = "0";
            node2.NodeText = "Test Node Text";

            //add node
            _treeModel.AddNode(node2);

            //assertion
            Assert.IsTrue(node.IsDisabled, "Node IsDisabled was false but expected to be true");
            Assert.IsTrue(_treeModel.ITreeModelView.IsNodeDisabled("11"), "Node is enabled at the view but expected to be disabled");

            //cleanup
            _treeModel.RemoveNode("11");
            _treeModel.RemoveNode("12");

        }

        [Test]
        public void Test_TMNC_TM_NodesByNodeText()
        {
            //create node
            Niawa.TreeModelNodeControls.TreeModelNode node = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node.NodeView = _nodeViewFactory.CreateNodeView("");
            node.NodeID = "13";
            node.ParentNodeID = "0";
            node.NodeText = "Test Node Text Return";

            //create node
            Niawa.TreeModelNodeControls.TreeModelNode node2 = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node2.NodeView = _nodeViewFactory.CreateNodeView("");
            node2.NodeID = "14";
            node2.ParentNodeID = "0";
            node2.NodeText = "Test Node Text Return";

            //create node
            Niawa.TreeModelNodeControls.TreeModelNode node3 = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node3.NodeView = _nodeViewFactory.CreateNodeView("");
            node3.NodeID = "15";
            node3.ParentNodeID = "0";
            node3.NodeText = "Test Node Text Return2";

            //add nodes
            _treeModel.AddNode(node);
            _treeModel.AddNode(node2);
            _treeModel.AddNode(node3);

            //get nodes
            List<Niawa.TreeModelNodeControls.TreeModelNode> nodes1 = _treeModel.NodesByNodeText("Test Node Text Return");
            List<Niawa.TreeModelNodeControls.TreeModelNode> nodes2 = _treeModel.NodesByNodeText("Test Node Text Return2");
            List<Niawa.TreeModelNodeControls.TreeModelNode> nodes3 = _treeModel.NodesByNodeText("Empty List");

            //assertions
            Assert.AreEqual(2, nodes1.Count, "Expected 2 items in nodes1 list");
            Assert.AreEqual(1, nodes2.Count, "Expected 1 item in nodes2 list");
            Assert.AreEqual(0, nodes3.Count, "Expected 0 items in nodes3 list");
            Assert.AreEqual(node, nodes1[0]);
            Assert.AreEqual(node2, nodes1[1]);
            Assert.AreEqual(node3, nodes2[0]);

            //cleanup
            _treeModel.RemoveNode("13");
            _treeModel.RemoveNode("14");
            _treeModel.RemoveNode("15");

        }

        [Test]
        public void Test_TMNC_TM_AcquireLock()
        {
            _treeModel.AcquireLock("Test_TMNC_TM_AcquireLock", 100);
            _treeModel.ReleaseLock();
      
        }
        
        [Test]
        [ExpectedException("System.TimeoutException")]
        public void Test_TMNC_TM_AcquireLockTimeout()
        {
            _treeModel.AcquireLock("Test_TMNC_TM_AcquireLockTimeout", 100);
            _treeModel.AcquireLock("Test_TMNC_TM_AcquireLockTimeout", 50);
            _treeModel.ReleaseLock();

        }

        [Test]
        public void Test_TMNC_TM_ReleaseLock()
        {
            _treeModel.AcquireLock("Test_TMNC_TM_ReleaseLock", 100);
            _treeModel.ReleaseLock();

        }

        [Test]
        public void Test_TMNC_TM_CurrentNode()
        {
            //create node
            Niawa.TreeModelNodeControls.TreeModelNode node = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node.NodeView = _nodeViewFactory.CreateNodeView("");
            node.NodeID = "16";
            node.ParentNodeID = "0";
            node.NodeText = "Test Node Text16";

            //add nodes
            _treeModel.AddNode(node);

            _treeModel.SelectNode("16");

            //assertion
            Assert.AreEqual(node, _treeModel.CurrentNode);

            //cleanup
            _treeModel.RemoveNode("16");

        }
    }
}
