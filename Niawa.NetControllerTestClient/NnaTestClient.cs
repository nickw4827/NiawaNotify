using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.NetControllerTestClient
{
    public class NnaTestClient
    {
        private int _udpPort;
        private string _ipAddress;
        private int _tcpPortRangeMin;
        private int _tcpPortRangeMax;
        private string _hostname;
        private string _applicationName;
        private string _webApiUrl;

        private int _totalMessageCounter;

        Niawa.NetController.NiawaAdHocNetworkAdapter nna;
        
        Niawa.WebNotify.IpcEventWebAPIAdapter.WebAPICommandReader webApiCmdReader = null;
        System.Threading.Thread t1 = null;

        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="udpPort"></param>
        /// <param name="ipAddress"></param>
        /// <param name="tcpPortRangeMin"></param>
        /// <param name="tcpPortRangeMax"></param>
        /// <param name="hostname"></param>
        /// <param name="applicationName"></param>
        /// <param name="webApiUrl"></param>
        public NnaTestClient(int udpPort, string ipAddress, int tcpPortRangeMin, int tcpPortRangeMax, string hostname, string applicationName, string webApiUrl)
        {
            _udpPort = udpPort;
            _ipAddress = ipAddress;
            _tcpPortRangeMin = tcpPortRangeMin;
            _tcpPortRangeMax = tcpPortRangeMax;
            _hostname = hostname;
            _applicationName = applicationName;
            _webApiUrl = webApiUrl;

            logger.Info(">>");
            logger.Info("Program started");
            

        }

        /// <summary>
        /// 
        /// </summary>
        public void ExecuteBasicTest()
        {
            Niawa.WebNotify.IpcEventWebAPIAdapter.IpcEventWebAPIWriter webApiWriter = null;

            try
            {
                //Ad-Hoc Network Adapter
                nna = new NetController.NiawaAdHocNetworkAdapter(_udpPort, _ipAddress, _tcpPortRangeMin, _tcpPortRangeMax, _hostname, _applicationName);

                //web notify IPC Event Web API Adapter
                if (_webApiUrl.Trim().Length > 0)
                {
                    logger.Info("Web API Writer is enabled: " + _webApiUrl);

                    Niawa.Utilities.UtilsServiceBus utilsBus = new Utilities.UtilsServiceBus();
                    //string webApiUrl = "http://localhost:3465";
                    webApiWriter = new WebNotify.IpcEventWebAPIAdapter.IpcEventWebAPIWriter(_webApiUrl, utilsBus);
                    webApiWriter.AddIpcEventReader("NiawaAdHocNetworkAdapter");
                    webApiWriter.AddIpcEventReader("TcpReceiver");
                    webApiWriter.AddIpcEventReader("TcpSession");
                    webApiWriter.AddIpcEventReader("TcpSessionManager");
                    webApiWriter.AddIpcEventReader("TcpTransmitter");
                    webApiWriter.AddIpcEventReader("UdpReceiver");
                    webApiWriter.AddIpcEventReader("UdpTransmitter");
                    webApiWriter.Start();

                    logger.Info("Web API Command Reader is enabled: " + _webApiUrl);
                    webApiCmdReader = new WebNotify.IpcEventWebAPIAdapter.WebAPICommandReader(_webApiUrl, 10000, utilsBus);
                    //subscribe to command for refreshing status(=1)
                    webApiCmdReader.CommandSubscribe(1);
                    webApiCmdReader.Start();
                    t1 = new System.Threading.Thread(WebApiCommandListenThreadImpl);
                    t1.Start();

                }
                else
                {
                    logger.Info("Web API Writer is disabled");
                }


                //start ad-hoc network adapter
                nna.Start();

                int every5Times = 0;
                int every25Times = 0;

                while (1 == 1)
                {
                    _totalMessageCounter++;

                    //send ping message every 5 times (50 seconds)
                    if (every5Times == 0)
                    {
                        nna.TransmitPingMessage(false, string.Empty);
                        //nna.TransmitHandshakeMessage();

                    }
                    every5Times++;

                    //send handshake message every 25 times (250 seconds)
                    if (every25Times == 0)
                    {
                        nna.TransmitHandshakeMessage();

                    }
                    every25Times++;

                    //send test message every time (10 seconds)
                    nna.TcpSessionManager.SendMessage(_applicationName, "TestMsgType" + _totalMessageCounter.ToString(), "TestMsg contents " + _totalMessageCounter.ToString());


                    //increase test counters
                    if (every5Times > 5)
                        every5Times = 0;

                    if (every25Times > 25)
                        every25Times = 0;

                    //sleep for 10 seconds
                    System.Threading.Thread.Sleep(10000);

                }

            }
            finally
            {
                if (t1 != null) t1.Abort();

                if (webApiWriter != null)
                    webApiWriter.Dispose();

                if (webApiCmdReader != null)
                    webApiCmdReader.Dispose();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void WebApiCommandListenThreadImpl(object data)
        {
            while (true)
            {
                if (webApiCmdReader.Messages.Count > 0)
                {
                    WebNotify.IpcEventWebAPIAdapter.NiawaWebMessage message = webApiCmdReader.Messages.Dequeue();
                    logger.Info("Received Web API Command message type [" + message.Id + "]");
            
                    switch(message.Id)
                    {
                        case 1:
                            //invoke a status update
                            logger.Info("Invoking a status update");
                            IpcController.IpcEvent evt = new IpcController.IpcEvent(Guid.NewGuid(), DateTime.Now, _applicationName, _hostname, "StatusReport", "", "", "", "");
                            nna.CmdReaderImpl(evt);

                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    //no new messages
                }

                System.Threading.Thread.Sleep(100);
            }

        }


    }
}
