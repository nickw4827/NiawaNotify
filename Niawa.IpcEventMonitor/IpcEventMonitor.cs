using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcEventMonitor
{
    /// <summary>
    /// Monitors IpcEvents for specified IpcGroup by instantiating an IpcEventReader and reading events in a loop.
    /// </summary>
    public class IpcEventMonitor
    {
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private string _ipcGroup;
        private Niawa.Utilities.UtilsServiceBus _utilsBus;

        /// <summary>
        /// Instantiates IpcEventMonitor
        /// </summary>
        /// <param name="ipcGroup">IpcGroup to monitor for IpcEvents</param>
        public IpcEventMonitor(string ipcGroup)
        {
            _ipcGroup = ipcGroup;

            //create utility bus
            _utilsBus = new Niawa.Utilities.UtilsServiceBus();
        }

        /// <summary>
        /// Start monitoring for IpcEvents.
        /// </summary>
        public void Execute()
        {
            logger.Info("Monitoring IPC event group [" + _ipcGroup + "]");

            Niawa.IpcController.iEventReader evtReader = Niawa.IpcController.IpcFactory.CreateIpcEventReader(false, _ipcGroup, _utilsBus);
            evtReader.Start();

            while (1==1)
            {
                Niawa.IpcController.IpcEvent evt = evtReader.ReadNextEvent();

                logger.Info("");
                logger.Info("======================= Received IPC Event ======================");
                logger.Info(" [" + evt.SerialID + "]  [" + evt.EventDate + "]");
                logger.Info(" Event ID [" + evt.EventID + "]");
                logger.Info(" Node ID [" + evt.NodeID + "]  Parent Node ID [" + evt.ParentNodeID + "]");
                logger.Info(" App [" + evt.ApplicationName + "]  Type [" + evt.EventType + "]");
                logger.Info(" Message [" + evt.EventMessage + "]");
                logger.Info(" Detail [" + evt.EventMessageDetail + "]");
                /*
                if (evt.EventProperties != null)
                {
                    logger.Info(" Properties (" + evt.EventProperties.Count + ") -----------------------------------");
                    foreach (KeyValuePair<string, string> kvp in evt.EventProperties)
                    {
                        logger.Info("    " + kvp.Key + ": " + kvp.Value);
                    }
                }
                 * */
                logger.Info("=================================================================");
                logger.Info("");
            }

        }

    }
    
}
