using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace Niawa.WebNotify.WebClient.TreeModel
{
    public class TreeNodeRenderContainer
    {
        bool _isRootNode = false;
        TreeNodeContainer _node = null;
        SortedList<string, TreeNodeRenderContainer> _childrenNodes;

        public TreeNodeRenderContainer(TreeNodeContainer node)
        {
            //The root node is a special empty node to hold children
            _node = node;
            _isRootNode = false;
            _childrenNodes = new SortedList<string, TreeNodeRenderContainer>();
        }

        public TreeNodeRenderContainer()
        {
            //The root node is a special empty node to hold children
            _node = null;
            _isRootNode = true;
            _childrenNodes = new SortedList<string, TreeNodeRenderContainer>();
        }

        public TreeNodeRenderContainer AddChildNode(string nodeText, string nodeID, TreeNodeContainer node)
        {
            //Add a child node
            TreeNodeRenderContainer renderNode = new TreeNodeRenderContainer(node);
            //Combine the node text and NodeID to sort the list alphabetically
            _childrenNodes.Add(nodeText + ":" + nodeID, renderNode);

            return renderNode;
        }

        public TreeNodeRenderContainer GetChildNode(string nodeText, string nodeID)
        {
            //Get a child node
            //Combine the node text and NodeID to sort the list alphabetically
            return _childrenNodes[nodeText + ":" + nodeID];

        }

        public bool IsRootNode()
        {
            return _isRootNode;
        }

        public TreeNodeContainer Node()
        {
            return _node;
        }

        public string ToHtmlUnorderedList()
        {

            try
            {
                //string htmlExisting = html;
                string htmlNew = string.Empty;

                //The root node is a special empty node to hold children
                //if not at the root level, get the current node as a list item
                if (!_isRootNode)
                    //htmlNew = htmlNew + "<li id='li:" + _node.NodeID + "'>" + _node.NodeText + "</li>";
                    htmlNew = htmlNew + "<li id='li:" + _node.NodeID + "'>" + @"<a href=""#"" onclick=""selectNode('" + _node.NodeID + @"')"">" + _node.NodeText + "</a></li>";

                //If the current node has children, create a list and add them
                if (_childrenNodes.Count > 0)
                {
                    //if the root, add a special ID. Otherwise, use the Node ID as the ID
                    if (_isRootNode)
                        htmlNew = htmlNew + "<ul id='ul:Root'>";
                    else
                        htmlNew = htmlNew + "<ul id='ul:" + _node.NodeID + "'>";

                    //iterate through the children nodes
                    string htmlAdd = "";
                    foreach (KeyValuePair<string, TreeNodeRenderContainer> kvp in _childrenNodes)
                    {
                        htmlAdd = htmlAdd + kvp.Value.ToHtmlUnorderedList(); //htmlAdd);
                    }

                    //add children node data and close the list
                    htmlNew = htmlNew + htmlAdd + "</ul>";
                }
                else
                {
                    //if no children nodes, and this is the root level, return an empty list
                    if (_isRootNode)
                        htmlNew = htmlNew + "<ul id='ul:Root'><li>(empty)</li></ul>";
                }

                //if(_isRootNode)
                //    Trace.TraceInformation("ToHtmlUL: Node [root] Build HTML [" + htmlExisting + "] adding [" + htmlNew + "]");
                //else
                //    Trace.TraceInformation("ToHtmlUL: Node [" + _node.NodeText + "][" + _node.NodeID + "] Build HTML [" + htmlExisting + "] adding [" + htmlNew + "]");

                //return HTML rendered as string
                return htmlNew;

            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeNodeRenderContainer Could not append generated HTML: " + ex.Message + ex.StackTrace);
                return "";
            }

        }


    }
}