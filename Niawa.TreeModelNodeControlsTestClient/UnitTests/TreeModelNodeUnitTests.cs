using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Niawa.TreeModelNodeControlsTestClient.UnitTests
{
    [TestFixture]
    public class TreeModelNodeUnitTests
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
        public void Test_NTMNC_TMN_RefreshOrphanedState_True()
        {
            //node 1
            Niawa.TreeModelNodeControls.TreeModelNode node = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node.NodeView = _nodeViewFactory.CreateNodeView("");
            node.NodeID = "11";
            node.ParentNodeID = "12";

            _treeModel.AddNode(node);

            //assertion
            Assert.IsTrue(node.IsOrphaned, "Value is false but expected to be true");


            _treeModel.RemoveNode("11");

        }

        [Test]
        public void Test_NTMNC_TMN_RefreshOrphanedState_False()
        {
            //node 1
            Niawa.TreeModelNodeControls.TreeModelNode node = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node.NodeView = _nodeViewFactory.CreateNodeView("");
            node.NodeID = "11";
            node.ParentNodeID = "12";

            _treeModel.AddNode(node);

            //assertion
            Assert.IsTrue(node.IsOrphaned, "Value is false but expected to be true");

            //node 2
            Niawa.TreeModelNodeControls.TreeModelNode node2 = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node2.NodeView = _nodeViewFactory.CreateNodeView("");
            node2.NodeID = "12";

            _treeModel.AddNode(node2);

            //assertion
            Assert.IsFalse(node.IsOrphaned, "Value is true but expected to be false");

            _treeModel.RemoveNode("11");
            _treeModel.RemoveNode("12");

        }

        [Test]
        public void Test_NTMNC_TMN_RefreshDisabledState_True()
        {
            Niawa.TreeModelNodeControls.TreeModelNode node = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node.NodeView = _nodeViewFactory.CreateNodeView("");
            node.NodeID = "3";
            node.NodeText = "Test Node";

            _treeModel.AddNode(node);

            //assertion
            Assert.IsFalse(node.IsDisabled, "Value is true but expected to be false");

            Niawa.TreeModelNodeControls.TreeModelNode node2 = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node2.NodeView = _nodeViewFactory.CreateNodeView("");
            node2.NodeID = "4";
            //same name will cause the first node to be disabled
            node2.NodeText = "Test Node";

            _treeModel.AddNode(node2);

            //assertion
            Assert.IsTrue(node.IsDisabled, "Value is false but expected to be true");

            _treeModel.RemoveNode("3");
            _treeModel.RemoveNode("4");
  
        }

        [Test]
        public void Test_NTMNC_TMN_RefreshDisabledState_False()
        {
            Niawa.TreeModelNodeControls.TreeModelNode node = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node.NodeView = _nodeViewFactory.CreateNodeView("");
            node.NodeID = "3";
            node.NodeText = "Test Node";

            _treeModel.AddNode(node);

            //assertion
            Assert.IsFalse(node.IsDisabled, "Value is true but expected to be false");

            _treeModel.RemoveNode("3");
  
        }

        [Test]
        public void Test_NTMNC_TMN_Prop_NodeID()
        {
            Niawa.TreeModelNodeControls.TreeModelNode node = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            
            //set value
            string testNodeID = "Test1";
            node.NodeID = testNodeID ;
            
            //assertion
            Assert.AreEqual(testNodeID, node.NodeID, "Value doesn't match expectation");
        }

        [Test]
        public void Test_NTMNC_TMN_Prop_NodeText()
        {
            Niawa.TreeModelNodeControls.TreeModelNode node = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);

            //set value
            string testNodeText = "Test2";
            node.NodeText = testNodeText;

            //assertion
            Assert.AreEqual(testNodeText, node.NodeText, "Value doesn't match expectation");
        }

        [Test]
        public void Test_NTMNC_TMN_Prop_ParentNodeID()
        {
            Niawa.TreeModelNodeControls.TreeModelNode node = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);

            //set value
            string testParentNodeID = "Test12";
            node.ParentNodeID = testParentNodeID;

            //assertion
            Assert.AreEqual(testParentNodeID, node.ParentNodeID, "Value doesn't match expectation");
        }

        [Test]
        public void Test_NTMNC_TMN_Prop_NodeView()
        {
            Niawa.TreeModelNodeControls.TreeModelNode node = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);

            //set value
            TestTreeModelNodeViewImpl nodeView = new TestTreeModelNodeViewImpl();
            node.NodeView = nodeView;

            //assertion
            Assert.AreEqual(nodeView, node.NodeView, "Value doesn't match expectation");
        }

        [Test]
        public void Test_NTMNC_TMN_Prop_IsOrphaned()
        {
            //node 1
            Niawa.TreeModelNodeControls.TreeModelNode node = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node.NodeView = _nodeViewFactory.CreateNodeView("");
            node.NodeID = "11";
            node.ParentNodeID = "12";

            _treeModel.AddNode(node);

            //assertion
            Assert.IsTrue(node.IsOrphaned, "Value is false but expected to be true");

            //node 2
            Niawa.TreeModelNodeControls.TreeModelNode node2 = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node2.NodeView = _nodeViewFactory.CreateNodeView("");
            node2.NodeID = "12";

            _treeModel.AddNode(node2);

            //assertion
            Assert.IsFalse(node.IsOrphaned, "Value is true but expected to be false");

            _treeModel.RemoveNode("11");
            _treeModel.RemoveNode("12");

        }

        [Test]
        public void Test_NTMNC_TMN_Prop_IsUnorphaned()
        {
            Niawa.TreeModelNodeControls.TreeModelNode node = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node.NodeView = _nodeViewFactory.CreateNodeView("");
            node.NodeID = "1";
            node.ParentNodeID = "2";

            _treeModel.AddNode(node);

            //assertion
            Assert.IsFalse(node.UnorphanedHistory, "Value is true but expected to be false");

            Niawa.TreeModelNodeControls.TreeModelNode node2 = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node2.NodeView = _nodeViewFactory.CreateNodeView("");
            node2.NodeID = "2";

            _treeModel.AddNode(node2);

            //assertion
            Assert.IsTrue(node.UnorphanedHistory, "Value is false but expected to be true");

            _treeModel.RemoveNode("1");
            _treeModel.RemoveNode("2");

        }

        [Test]
        public void Test_NTMNC_TMN_Prop_IsDisabled()
        {
            Niawa.TreeModelNodeControls.TreeModelNode node = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node.NodeView = _nodeViewFactory.CreateNodeView("");
            node.NodeID = "3";
            node.NodeText = "Test Node";
            
            _treeModel.AddNode(node);

            //assertion
            Assert.IsFalse(node.IsDisabled, "Value is true but expected to be false");
            
            Niawa.TreeModelNodeControls.TreeModelNode node2 = new Niawa.TreeModelNodeControls.TreeModelNode(_treeModel);
            node2.NodeView = _nodeViewFactory.CreateNodeView("");
            node2.NodeID = "4";
            //same name will cause the first node to be disabled
            node2.NodeText = "Test Node";
            
            _treeModel.AddNode(node2);

            //assertion
            Assert.IsTrue(node.IsDisabled, "Value is false but expected to be true");

            _treeModel.RemoveNode("3");
            _treeModel.RemoveNode("4");
                      
        }

    }
}
