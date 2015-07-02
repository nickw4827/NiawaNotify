using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcEventTestClient
{
    public class MockNiawaAdHocNetworkAdapter
    {
        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        //threading
        private System.Threading.Thread t1;
        private bool threadActive = false;
        
        /// <summary>
        /// 
        /// </summary>
        public MockNiawaAdHocNetworkAdapter()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            logger.Info("MockNiawaAdHocNetworkAdapter: Starting");

            t1.Start();

        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            logger.Info("MockNiawaAdHocNetworkAdapter: Stopping");

            threadActive = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        private void ThreadImpl(object data)
        {
            threadActive = true;
            logger.Info("MockNiawaAdHocNetworkAdapter: Thread started");

            try
            {
                
                while (threadActive)
                {

                    try
                    {
                        /*
                        _treeModelAdapter.AddIpcEvent("NiawaAdHocNetworkAdapter");
                        _treeModelAdapter.AddIpcEvent("TcpReceiver");
                        _treeModelAdapter.AddIpcEvent("TcpSession");
                        _treeModelAdapter.AddIpcEvent("TcpSessionManager");
                        _treeModelAdapter.AddIpcEvent("TcpTransmitter");
                        _treeModelAdapter.AddIpcEvent("UdpReceiver");
                        _treeModelAdapter.AddIpcEvent("UdpTransmitter");
                        */

                    }
                    catch (Exception ex)
                    {
                        logger.Error("MockNiawaAdHocNetworkAdapter: Thread error: " + ex.Message);

                    }

                    System.Threading.Thread.Sleep(10);
                }

            }
            catch (Exception ex2)
            {
                logger.Error("MockNiawaAdHocNetworkAdapter: Unrecovered thread error: " + ex2.Message);
                throw ex2;
            }

            logger.Info("MockNiawaAdHocNetworkAdapter: Thread stopped");

        }


    }
}
