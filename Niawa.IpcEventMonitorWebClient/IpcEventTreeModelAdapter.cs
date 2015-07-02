using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcEventMonitorWebClient
{
    public class IpcEventTreeModelAdapter : IDisposable
    {
        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        //treemodel implementation
        private TreeModel.TreeModelViewImpl _view = null;
        private TreeModel.TreeModelNodeViewFactoryImpl _nodeViewFactory = null;

        //treemodel impl-specific objects
        private System.Windows.Forms.TreeView _formTreeView = null;
        private System.Windows.Forms.WebBrowser _webBrowser = null;
        
        //forms
        private Forms.IpcTreeWebWindow _formWindow = null;
        //private Forms.MessageLogWindow _messageLogWindow = null;
        private Forms.IpcEventMonitorMdiWindow _mdiWindow = null;

        //treemodel controller
        private Niawa.TreeModelNodeControls.TreeModelController _treeModelController = null;

        //thread
        private bool active = false;
        
        //resources
        private SortedList<string, IpcEventTreeModelAdapterThread> threads = new SortedList<string, IpcEventTreeModelAdapterThread>();
        private Niawa.Utilities.UtilsServiceBus _utilsBus = null;
        private Niawa.MsEventIpcEventAdapter.MsEventIpcEventWriter _evtWriter = null;

        private string _webURL = string.Empty;

        /// <summary>
        /// Instantiates a new IpcEventTreeModelAdapter.
        /// </summary>
        /// <param name="formWindow"></param>
        /// <param name="formTreeView"></param>
        public IpcEventTreeModelAdapter(Forms.IpcTreeWebWindow formWindow, Forms.IpcEventMonitorMdiWindow mdiWindow, System.Windows.Forms.TreeView formTreeView)
        {

            _webURL = "file:///{0}/Html/IpcEventMonitorPane.html";

            _formWindow = formWindow;
            _mdiWindow = mdiWindow;
            _formTreeView = formTreeView;
            _webBrowser = _formWindow.WebBrowser1;

            //create utility bus
            _utilsBus = new Niawa.Utilities.UtilsServiceBus();

            //set up ipc logging for this class (to log events that occur in the tree model)
            _evtWriter = new Niawa.MsEventIpcEventAdapter.MsEventIpcEventWriter(_utilsBus);
            _evtWriter.Start();
            _evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("IpcEventMonitor", true, "TreeModel", _utilsBus), "TreeModel");
         
            //instantiate view
            _view = new TreeModel.TreeModelViewImpl(_formWindow, _formTreeView);
            
            //instantiate node view factory
            _nodeViewFactory = new TreeModel.TreeModelNodeViewFactoryImpl(_webBrowser, _webURL);

            //instantiate tree model controller
            _treeModelController = new TreeModelNodeControls.TreeModelController(_view, _nodeViewFactory, _evtWriter.EvtConsumer, "IpcEventMonitor", string.Empty);



        }

        /// <summary>
        /// Add IPC event to monitor and insert into Tree Model
        /// </summary>
        /// <param name="ipcType"></param>
        public void AddIpcEvent(string ipcType)
        {
            System.Windows.Forms.ToolStripStatusLabel tsslStatus = _formWindow.TsslStatus;

            //add thread holder to list
            threads.Add(ipcType, new IpcEventTreeModelAdapterThread(ipcType, _treeModelController, tsslStatus, _utilsBus, this));
        }

        /// <summary>
        /// Start monitoring IPC events to insert into Tree Model
        /// </summary>
        public void Start()
        {
            InitializeWebBrowser(_webURL);
            _treeModelController.Start();
            
            foreach (KeyValuePair<string, IpcEventTreeModelAdapterThread> kvp in threads)
            {
                kvp.Value.Initialize();
                kvp.Value.Start();
            }

        }

        /// <summary>
        /// Stop monitoring IPC events
        /// </summary>
        public void Stop()
        {
            _treeModelController.Stop();
            foreach (KeyValuePair<string, IpcEventTreeModelAdapterThread> kvp in threads)
            {
                kvp.Value.Stop();
            }
        }

        public void ShowLogMessage(string message)
        {
            _mdiWindow.ShowLogMessage(message);
        }

        /// <summary>
        /// Initializes the web browser to the provided URL
        /// </summary>
        private void InitializeWebBrowser(string webURL)
        {
            string curDir = System.IO.Directory.GetCurrentDirectory();
            _webBrowser.Url = new Uri(String.Format(webURL, curDir));

            while (_webBrowser.ReadyState != System.Windows.Forms.WebBrowserReadyState.Complete)
            {
                System.Windows.Forms.Application.DoEvents();
            }

        }

        /// <summary>
        /// Sets the active view according to the Node ID supplied.
        /// </summary>
        /// <param name="nodeID"></param>
        public void SetActiveView(string nodeID)
        {
            //_view.SelectNode(nodeID);
            _treeModelController.TreeModel.SelectNode(nodeID);
        }

        /// <summary>
        /// Dispose IpcEventTreeModelAdapter
        /// </summary>
        public void Dispose()
        {
            _treeModelController.Dispose();
            _evtWriter.Stop(true);
            _evtWriter.Dispose();

            foreach (KeyValuePair<string, IpcEventTreeModelAdapterThread> kvp in threads)
            {
                kvp.Value.Dispose();
            }
        }
    }
}
