using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Diagnostics;

namespace Niawa.WebNotify.WebClient
{
    public class NiawaSRHub : Hub
    {
        //NiawaIpcEventTreeModelAdapter _treeModelAdapter = null;

        /// <summary>
        /// Instantiate a NiawaSRHub.
        /// </summary>
        public NiawaSRHub()
        {
            try
            {
                //register with resource provider so other classes can locate it
                Trace.TraceInformation("NiawaSRHub: Registering NiawaSRHub with provider");
                NiawaResourceProvider.RegisterNiawaSRHub(this);

                /*
                //check if there is an existing Tree Model Adapter
                NiawaIpcEventTreeModelAdapter treeModelAdapter = NiawaResourceProvider.RetrieveNiawaTreeModelAdapter();
                if (treeModelAdapter == null)
                {

                    //create tree model adapter
                    Trace.TraceInformation("NiawaSRHub: Creating Tree Model Adapter");
                    _treeModelAdapter = new NiawaIpcEventTreeModelAdapter(this);

                    //start tree model adapter
                    Trace.TraceInformation("NiawaSRHub: Starting Tree Model Adapter");
                    _treeModelAdapter.Start();

                    //register it with the hub
                    Trace.TraceInformation("NiawaSRHub: Registering Tree Model Adapter with provider");
                    NiawaResourceProvider.RegisterNiawaTreeModelAdapter(_treeModelAdapter);

                }
                else
                {
                    //use existing tree model adapter
                    Trace.TraceInformation("NiawaSRHub: Using existing Tree Model Adapter");
                    _treeModelAdapter = treeModelAdapter;
                }
                */
                

            }
            catch (Exception ex)
            {
                Trace.TraceError("NiawaSRHub Failed to instantiate: " + ex.Message);
            }


        }

        /// <summary>
        /// Retrieves or creates a TreeModelAdapter for the specified caller
        /// </summary>
        /// <param name="callerSessionID"></param>
        /// <returns></returns>
        private NiawaIpcEventTreeModelAdapter RetrieveTreeModelAdapter(string callerSessionID)
        {
            
               //check if there is an existing Tree Model Adapter
               NiawaIpcEventTreeModelAdapter treeModelAdapter = NiawaResourceProvider.RetrieveNiawaTreeModelAdapter(callerSessionID);
               
               if (treeModelAdapter == null)
               {
                   //get Tree Model Adapter Pool
                   NiawaIpcEventTreeModelAdapterPool pool = NiawaResourceProvider.RetrieveNiawaTreeModelAdapterPool();

                   //create tree model adapter
                   Trace.TraceInformation("NiawaSRHub [" + callerSessionID + "]: Creating Tree Model Adapter");
                   treeModelAdapter = new NiawaIpcEventTreeModelAdapter(this, pool, callerSessionID);

                   //start tree model adapter
                   Trace.TraceInformation("NiawaSRHub [" + callerSessionID + "]: Starting Tree Model Adapter");
                   treeModelAdapter.Start(true);

                   //register it with resource provider
                   Trace.TraceInformation("NiawaSRHub [" + callerSessionID + "]: Registering Tree Model Adapter with provider");
                   NiawaResourceProvider.RegisterNiawaTreeModelAdapter(treeModelAdapter, callerSessionID);

               }
               else
               {
                   //use existing tree model adapter
                   Trace.TraceInformation("NiawaSRHub [" + callerSessionID + "]: Using existing Tree Model Adapter");
               }

               return treeModelAdapter;
   
        }

        /// <summary>
        /// Remove all rows at the client
        /// </summary>
        public void RemoveRows(string callerSessionID)
        {
            try
            {
                Trace.TraceInformation("NiawaSRHub [" + callerSessionID + "]: Removing rows");
                //Clients.All.removeRows();
                Clients.Caller.removeRows(callerSessionID);
            }
            catch (Exception ex)
            {
                Trace.TraceError("NiawaSRHub [" + callerSessionID + "]: Failed to RemoveRows: " + ex.Message);
            }
        }

        /// <summary>
        /// Add a row at the client
        /// </summary>
        /// <param name="message"></param>
        public void AddRow(string message, string callerSessionID)
        {
            try
            {
                Trace.TraceInformation("NiawaSRHub [" + callerSessionID + "]: Adding row");
                //Clients.All.addRow(message);
                Clients.Caller.addRow(message, callerSessionID);
            }
            catch (Exception ex)
            {
                Trace.TraceError("NiawaSRHub [" + callerSessionID + "]: Failed to AddRow [" + message + "]: " + ex.Message);
            }
        }

        /// <summary>
        /// Populate the status block at the client
        /// </summary>
        /// <param name="message"></param>
        public void PopulateStatusBlock(string message, string callerSessionID)
        {
            try
            {
                Trace.TraceInformation("NiawaSRHub [" + callerSessionID + "]: Populating status block");
                //Clients.All.populateStatusBlock(message);
                Clients.Caller.populateStatusBlock(message, callerSessionID);
            }
            catch (Exception ex)
            {
                Trace.TraceError("NiawaSRHub [" + callerSessionID + "]: Failed to PopulateStatusBlock [" + message + "]: " + ex.Message);
            }
        }

        /// <summary>
        /// Refresh the tree view at the client
        /// </summary>
        /// <param name="nodeID"></param>
        public void TreeViewRefresh(string html, string description, string callerSessionID)
        {
            try
            {
                Trace.TraceInformation("NiawaSRHub [" + callerSessionID + "]: TreeView Refreshing [" + description + "]");
                //Clients.All.treeViewRefresh(html);
                Clients.Caller.treeViewRefresh(html, description, callerSessionID);
            }
            catch (Exception ex)
            {
                Trace.TraceError("NiawaSRHub [" + callerSessionID + "]: Failed to execute TreeViewRefresh: " + ex.Message);
            }

        }

        /// <summary>
        /// The client calls this function to select a node for the active view
        /// </summary>
        /// <param name="nodeID"></param>
        public void SelectNode(string nodeID, string callerSessionID)
        {
            Trace.TraceInformation("NiawaSRHub [" + callerSessionID + "]: User selected node");
            //get tree model adapter
            NiawaIpcEventTreeModelAdapter treeModelAdapter = RetrieveTreeModelAdapter(callerSessionID);

            //set this node as the active view in the controller
            treeModelAdapter.SetActiveView(nodeID);

        }

        /// <summary>
        /// The client calls this function to request a refresh of the tree view (i.e. when the page first loads)
        /// </summary>
        public void RefreshTreeView(string callerSessionID)
        {
            try
            {
                Trace.TraceInformation("NiawaSRHub [" + callerSessionID + "]: User requested treeview refresh");

                //get tree model adapter
                NiawaIpcEventTreeModelAdapter treeModelAdapter = RetrieveTreeModelAdapter(callerSessionID);
                
                //Clients.All.treeViewRefresh(treeModelAdapter.View.ToHtmlUnorderedList());
                Clients.Caller.treeViewRefresh(treeModelAdapter.View.ToHtmlUnorderedList(), "Initialize", callerSessionID);
            }
            catch (Exception ex)
            {
                Trace.TraceError("NiawaSRHub [" + callerSessionID + "]: Failed to execute RefreshTreeView: " + ex.Message);
            }

        }

        /// <summary>
        /// Server initiates the session poll with the client
        /// </summary>
        /// <param name="callerSessionID"></param>
        public void SessionConnected(string callerSessionID)
        {
            try
            {
                Trace.TraceInformation("NiawaSRHub [" + callerSessionID + "]: Client session connected");

                //poll client
                Clients.Caller.sessionConnected(callerSessionID);
            }
            catch (Exception ex)
            {
                Trace.TraceError("NiawaSRHub [" + callerSessionID + "]: Failed to execute SessionConnected: " + ex.Message);
            }

        }

        /// <summary>
        /// Server initiates the session poll with the client
        /// </summary>
        /// <param name="callerSessionID"></param>
        public void SessionDisconnected(string description, string callerSessionID)
        {
            try
            {
                Trace.TraceInformation("NiawaSRHub [" + callerSessionID + "]: Client session disconnected (" + description + ")");

                //poll client
                Clients.Caller.sessionDisconnected(description, callerSessionID);
            }
            catch (Exception ex)
            {
                Trace.TraceError("NiawaSRHub [" + callerSessionID + "]: Failed to execute SessionDisconnected: " + ex.Message);
            }

        }

        /// <summary>
        /// Server initiates the session poll with the client
        /// </summary>
        /// <param name="callerSessionID"></param>
        public void PollSession(string callerSessionID)
        {
            try
            {
                Trace.TraceInformation("NiawaSRHub [" + callerSessionID + "]: Server polling session with the client");

                //poll client
                Clients.Caller.pollSession(callerSessionID);
            }
            catch (Exception ex)
            {
                Trace.TraceError("NiawaSRHub [" + callerSessionID + "]: Failed to execute PollSession: " + ex.Message);
            }

        }

        /// <summary>
        /// Client polls the session at the server (or responds to request)
        /// </summary>
        /// <param name="callerSessionID"></param>
        public void PollSessionClient(string callerSessionID)
        {
            try
            {
                Trace.TraceInformation("NiawaSRHub [" + callerSessionID + "]: Client polling session with the server");

                //update session as still valid
                //get tree model adapter
                NiawaIpcEventTreeModelAdapter treeModelAdapter = RetrieveTreeModelAdapter(callerSessionID);

                treeModelAdapter.LastSessionPoll = DateTime.Now;
            
            }
            catch (Exception ex)
            {
                Trace.TraceError("NiawaSRHub [" + callerSessionID + "]: Failed to execute PollSessionClient: " + ex.Message);
            }
        }

        /// <summary>
        /// Select the active view in the tree at the client
        /// </summary>
        /// <param name="nodeID"></param>
        public void TreeViewNodeSelected(string nodeID, string callerSessionID)
        {
            try
            {
                Trace.TraceInformation("NiawaSRHub [" + callerSessionID + "]: TreeView Selecting Node [" + nodeID + "]");
                //Clients.All.treeViewNodeSelected(nodeID);
                Clients.Caller.treeViewNodeSelected(nodeID);
            }
            catch (Exception ex)
            {
                Trace.TraceError("NiawaSRHub [" + callerSessionID + "]: Failed to execute TreeViewNodeSelected [" + nodeID + "]: " + ex.Message);
            }

        }

        /// <summary>
        /// Send a message to all clients. (OBSOLETE)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        //public void Send(int id, string sender, string message)
        //{
        //    try
        //    {

        //        // Call the broadcastMessage method to update clients.
        //        Trace.TraceInformation("NiawaSRHub Sending message");
        //        Clients.All.broadcastMessage(id, sender, message);
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.TraceError("NiawaSRHub Failed to Send message: " + ex.Message);
        //    }

        //}

        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="treeNodeCtr"></param>
        public void TreeViewAddToRoot(TreeModel.TreeNodeContainer treeNodeCtr)
        {
            try
            {
                Trace.TraceInformation("NiawaSRHub TreeView Adding Node [" + treeNodeCtr.NodeID + "] to Root");
                Clients.All.treeViewAddToRoot(treeNodeCtr.NodeID, treeNodeCtr.NodeText);
                Trace.TraceInformation("NiawaSRHub TreeView Sorting nodes at Root"); 
                Clients.All.treeViewSortNodesAtRoot();
            }
            catch (Exception ex)
            {
                Trace.TraceError("NiawaSRHub Failed to execute TreeViewAddToRoot [" + treeNodeCtr.NodeID + "]: " + ex.Message);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="treeNodeCtr"></param>
        /// <param name="parentTreeNodeCtr"></param>
        public void TreeViewAddToNode(TreeModel.TreeNodeContainer treeNodeCtr, TreeModel.TreeNodeContainer parentTreeNodeCtr)
        {

            try
            {
                Trace.TraceInformation("NiawaSRHub TreeView Adding Node [" + treeNodeCtr.NodeID + "] to parent node [" + parentTreeNodeCtr.NodeID + "]");
                Clients.All.treeViewAddToNode(treeNodeCtr.NodeID, treeNodeCtr.NodeText, parentTreeNodeCtr.NodeID);
                Trace.TraceInformation("NiawaSRHub TreeView Sorting nodes at parent node [" + parentTreeNodeCtr.NodeID + "]");
                Clients.All.treeViewSortNodes(parentTreeNodeCtr.NodeID);
            }
            catch (Exception ex)
            {
                Trace.TraceError("NiawaSRHub Failed to execute TreeViewAddToNode [" + treeNodeCtr.NodeID + "]: " + ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="treeNodeCtr"></param>
        public void TreeViewRemoveFromRoot(TreeModel.TreeNodeContainer treeNodeCtr)
        {
            
            try
            {
                Trace.TraceInformation("NiawaSRHub TreeView Removing Node [" + treeNodeCtr.NodeID + "] from Root");
                Clients.All.treeViewRemoveFromRoot(treeNodeCtr.NodeID, treeNodeCtr.NodeText);
            }
            catch (Exception ex)
            {
                Trace.TraceError("NiawaSRHub Failed to execute TreeViewRemoveFromRoot [" + treeNodeCtr.NodeID + "]: " + ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="treeNodeCtr"></param>
        /// <param name="parentTreeNodeCtr"></param>
        public void TreeViewRemoveFromNode(TreeModel.TreeNodeContainer treeNodeCtr, TreeModel.TreeNodeContainer parentTreeNodeCtr)
        {
            try
            {
                Trace.TraceInformation("NiawaSRHub TreeView Removing Node [" + treeNodeCtr.NodeID + "] from parent node [" + parentTreeNodeCtr.NodeID + "]");
                Clients.All.treeViewRemoveFromNode(treeNodeCtr.NodeID, parentTreeNodeCtr.NodeID);
            }
            catch (Exception ex)
            {
                Trace.TraceError("NiawaSRHub Failed to execute TreeViewRemoveFromNode [" + treeNodeCtr.NodeID + "]: " + ex.Message);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="treeNodeCtr"></param>
        /// <param name="nodeText"></param>
        public void TreeViewUpdateTextNodeAtRoot(TreeModel.TreeNodeContainer treeNodeCtr, string nodeText)
        {
            try
            {
                Trace.TraceInformation("NiawaSRHub TreeView Updating node text Node [" + treeNodeCtr.NodeID + "] at Root");
                Clients.All.treeViewUpdateTextNodeAtRoot(treeNodeCtr.NodeID, treeNodeCtr.NodeText);
                Trace.TraceInformation("NiawaSRHub TreeView Sorting nodes at Root");
                Clients.All.treeViewSortNodesAtRoot();
            }
            catch (Exception ex)
            {
                Trace.TraceError("NiawaSRHub Failed to execute TreeViewUpdateTextNodeAtRoot [" + treeNodeCtr.NodeID + "]: " + ex.Message);
            } 
            
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="treeNodeCtr"></param>
        /// <param name="nodeText"></param>
        /// <param name="parentTreeNodeCtr"></param>
        public void TreeViewUpdateTextNode(TreeModel.TreeNodeContainer treeNodeCtr, string nodeText, TreeModel.TreeNodeContainer parentTreeNodeCtr)
        {
            try
            {
                Trace.TraceInformation("NiawaSRHub TreeView Updating node text Node [" + treeNodeCtr.NodeID + "] at parent node [" + parentTreeNodeCtr.NodeID + "]");
                Clients.All.treeViewUpdateTextNode(treeNodeCtr.NodeID, treeNodeCtr.NodeText, parentTreeNodeCtr.NodeID);
                Trace.TraceInformation("NiawaSRHub TreeView Sorting nodes at parent node [" + parentTreeNodeCtr.NodeID + "]");
                Clients.All.treeViewSortNodes(parentTreeNodeCtr.NodeID);
            }
            catch (Exception ex)
            {
                Trace.TraceError("NiawaSRHub Failed to execute TreeViewUpdateTextNode [" + treeNodeCtr.NodeID + "]: " + ex.Message);
            }

        }
        */



    }
}