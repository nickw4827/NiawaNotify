using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.NetController
{
    public class ThreadHealthMonitor : IDisposable 
    {
        /* #threadPattern# */

        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /* Globals s*/
        private string _description = string.Empty;

        /* Threading */
        private System.Threading.Thread t1;
        private Niawa.Threading.ThreadStatus _threadStatus = null;
        
        /* Resources */
        NiawaAdHocNetworkAdapter _nna = null;
        
        public ThreadHealthMonitor(NiawaAdHocNetworkAdapter nna)
        {
            _description = "ThreadHealthMonitor";
            _nna = nna;
            _threadStatus = new Niawa.Threading.ThreadStatus(_description, 60, nna.UtilsBus.InitializeSerialId(Niawa.Utilities.IdGeneratorUtils.ID_ROOT_NIAWA_THREAD_ID).ToString(), string.Empty, null);

            //thread status
            _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_INITIALIZED;

        }

        /// <summary>
        /// Start thread health monitor
        /// </summary>
        public void Start()
        {
            logger.Info("[" + _description + "-M] Starting");

            if (_threadStatus.IsThreadEnabled)
            {
                logger.Warn("[" + _description + "-M] Could not start: Thread was already active");
            }
            else
            {
                _threadStatus.IsThreadEnabled = true;

                t1 = new System.Threading.Thread(MonitorThreadImpl);
                t1.Start();

                //thread status
                _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STARTED;

            }

        }

        /// <summary>
        /// Stop thread health monitor
        /// </summary>
        public void Stop()
        {
            logger.Info("[" + _description + "-M] Stopping");

            _threadStatus.IsThreadEnabled = false;

            //thread status
            _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STOPPED;


            //wait for up to 10 seconds for thread to end, then abort if not finished
            int timeoutIx = 0;
            while (t1.IsAlive)
            {
                timeoutIx++;
                System.Threading.Thread.Sleep(50);
                if (timeoutIx > 200) break;
            }

            if (t1.IsAlive)
            {
                logger.Warn("[" + _description + "-M] Aborting unresponsive thread");
                t1.Abort();
            }
        }

        /// <summary>
        /// Loop thread that monitors thread health
        /// </summary>
        /// <param name="data"></param>
        private void MonitorThreadImpl(object data)
        {
            _threadStatus.IsThreadActive = true;
            logger.Info("[" + _description + "-T] Thread active");

            while (_threadStatus.IsThreadEnabled)
            {
                _threadStatus.ThreadHealthDate = DateTime.Now;

                //sleep for 55 seconds
                System.Threading.Thread.Sleep(55000);
    
                try
                {

                    /*NiawaNetworkAdapter*/
                    try
                    {
                        //raise thread health
                        if (_nna.ThreadStatus.IsEvtRaiserSet())
                            _nna.ThreadStatus.RaiseThreadHealth();
                        else
                            logger.Warn("Could not raise thread health for " + _nna.ThreadStatus.Description + ": Event Raiser is not instantiated.");

                        if(_nna.ThreadStatus.IsThreadHealthFailed)
                        {
                            //udp receiver listener thread implementation
                            //logger.Warn("[" + _description + "-T] Thread health failed [" + healthKvp.Key + "] last reported secs: " + Convert.ToInt32(healthKvp.Value.Key).ToString());
                            logger.Warn("[" + _description + "-T] Thread health failed [" + _nna.ThreadStatus.Description + "] last reported secs: " + _nna.ThreadStatus.CalculateThreadHealth().ToString("F"));

                            //stop and restart the entire adapter
                            logger.Info("[" + _description + "-T] Stopping [" + _nna.ThreadStatus.Description + "]");
                            _nna.Stop();
                            System.Threading.Thread.Sleep(5000);
                            logger.Info("[" + _description + "-T] Restarting [" + _nna.ThreadStatus.Description + "]");
                            _nna.Start();
                        }
                    }
                    catch (Exception ex11)
                    {
                        logger.Error("Could not raise thread health for " + _nna.ThreadStatus.Description + ": " + ex11.Message , ex11);
                    }
                   

                    /*NiawaNetworkAdapter.UdpReceiver*/
                    try
                    {
                        //raise thread health
                        if (_nna.UdpReceiver.ThreadStatus.IsEvtRaiserSet())
                            _nna.UdpReceiver.ThreadStatus.RaiseThreadHealth();
                        else
                            logger.Warn("Could not raise thread health for " + _nna.UdpReceiver.ThreadStatus.Description + ": Event Raiser is not instantiated.");

                        if (_nna.UdpReceiver.ThreadStatus.IsThreadHealthFailed)
                        {
                            //udp receiver listener thread implementation
                            logger.Warn("[" + _description + "-T] Thread health failed [" + _nna.UdpReceiver.ThreadStatus.Description + "] last reported secs: " + _nna.UdpReceiver.ThreadStatus.CalculateThreadHealth().ToString("F"));

                            //stop and restart the udp receiver
                            logger.Info("[" + _description + "-T] Stopping [" + _nna.UdpReceiver.ThreadStatus.Description + "]");
                            _nna.UdpReceiver.StopListening("ThreadHealthMonitor", true);
                            System.Threading.Thread.Sleep(5000);
                            logger.Info("[" + _description + "-T] Restarting [" + _nna.UdpReceiver.ThreadStatus.Description + "]");
                            _nna.UdpReceiver.StartListening("ThreadHealthMonitor");
                        }
                    }
                    catch (Exception ex11)
                    {
                        logger.Error("Could not raise thread health for " + _nna.UdpReceiver.ThreadStatus.Description + ": " + ex11.Message, ex11);
                    }
                    

                    /*NiawaNetworkAdapter.UdpTransmitter*/
                    try
                    {
                        //raise thread health
                        if (_nna.UdpTransmitter.ThreadStatus.IsEvtRaiserSet())
                            _nna.UdpTransmitter.ThreadStatus.RaiseThreadHealth();
                        else
                            logger.Warn("Could not raise thread health for " + _nna.UdpTransmitter.ThreadStatus.Description + ": Event Raiser is not instantiated.");

                        if (_nna.UdpTransmitter.ThreadStatus.IsThreadHealthFailed)
                        {
                            //udp transmitter listener thread implementation
                            logger.Warn("[" + _description + "-T] Thread health failed [" + _nna.UdpTransmitter.ThreadStatus.Description + "] last reported secs: " + _nna.UdpTransmitter.ThreadStatus.CalculateThreadHealth().ToString("F"));

                            //stop and restart the udp transmitter
                            logger.Info("[" + _description + "-T] Stopping [" + _nna.UdpTransmitter.ThreadStatus.Description + "]");
                            _nna.UdpTransmitter.StopTransmitting("ThreadHealthMonitor", true);
                            System.Threading.Thread.Sleep(5000);
                            logger.Info("[" + _description + "-T] Restarting [" + _nna.UdpTransmitter.ThreadStatus.Description + "]");
                            _nna.UdpTransmitter.StartTransmitting("ThreadHealthMonitor");
                        }
                    }
                    catch (Exception ex11)
                    {
                        logger.Error("Could not raise thread health for " + _nna.UdpTransmitter.ThreadStatus.Description + ": " + ex11.Message, ex11);
                    }
                    
 
                    /*NiawaNetworkAdapter.TcpSessionManager.HandshakeReceiver*/
                    try
                    {
                        //raise thread health
                        if (_nna.TcpSessionManager.HandshakeReceiverThreadStatus.IsEvtRaiserSet())
                            _nna.TcpSessionManager.HandshakeReceiverThreadStatus.RaiseThreadHealth();
                        else
                            logger.Warn("Could not raise thread health for " + _nna.TcpSessionManager.HandshakeReceiverThreadStatus.Description + ": Event Raiser is not instantiated.");

                        if (_nna.TcpSessionManager.HandshakeReceiverThreadStatus.IsThreadHealthFailed)
                        {
                            //handshake receiver listener thread implementation
                            logger.Warn("[" + _description + "-T] Thread health failed [" + _nna.TcpSessionManager.HandshakeReceiverThreadStatus.Description + "] last reported secs: " + _nna.TcpSessionManager.HandshakeReceiverThreadStatus.CalculateThreadHealth().ToString("F"));

                            //stop and restart the entire adapter
                            logger.Info("[" + _description + "-T] Stopping [" + _nna.ThreadStatus.Description + "]");
                            _nna.Stop();
                            System.Threading.Thread.Sleep(5000);
                            logger.Info("[" + _description + "-T] Restarting [" + _nna.ThreadStatus.Description + "]");
                            _nna.Start();
                        }          
                    }
                    catch (Exception ex11)
                    {
                        logger.Error("Could not raise thread health for " + _nna.TcpSessionManager.HandshakeReceiverThreadStatus.Description + ": " + ex11.Message, ex11);
                    }
                    

                    /*NiawaNetworkAdapter.TcpSessionManager.HandshakeReceiver.TcpReceiver*/
                    try
                    {
                        //raise thread health
                        if (_nna.TcpSessionManager.HandshakeReceiver.ThreadStatus.IsEvtRaiserSet())
                            _nna.TcpSessionManager.HandshakeReceiver.ThreadStatus.RaiseThreadHealth();
                        else
                            logger.Warn("Could not raise thread health for " + _nna.TcpSessionManager.HandshakeReceiver.ThreadStatus.Description + ": Event Raiser is not instantiated.");

                        if (_nna.TcpSessionManager.HandshakeReceiver.ThreadStatus.IsThreadHealthFailed)
                        {
                            //handshake receiver tcp receiver
                            logger.Warn("[" + _description + "-T] Thread health failed [" + _nna.TcpSessionManager.HandshakeReceiver.ThreadStatus.Description + "] last reported secs: " + _nna.TcpSessionManager.HandshakeReceiver.ThreadStatus.CalculateThreadHealth().ToString("F"));

                            //stop and restart the tcp receiver
                            logger.Info("[" + _description + "-T] Stopping [" + _nna.TcpSessionManager.HandshakeReceiver.ThreadStatus.Description + "]");
                            _nna.TcpSessionManager.HandshakeReceiver.StopListening("ThreadHealthMonitor", true);
                            System.Threading.Thread.Sleep(5000);
                            logger.Info("[" + _description + "-T] Restarting [" + _nna.TcpSessionManager.HandshakeReceiver.ThreadStatus.Description + "]");
                            _nna.TcpSessionManager.HandshakeReceiver.StartListening("ThreadHealthMonitor");
                        } 
                    }
                    catch (Exception ex11)
                    {
                        logger.Error("Could not raise thread health for " + _nna.TcpSessionManager.HandshakeReceiver.ThreadStatus.Description + ": " + ex11.Message, ex11);
                    }
                    

                    /*NiawaNetworkAdapter.TcpSessionManager.HandshakeReceiver*/
                    try
                    {
                        //raise thread health
                        if (_nna.TcpSessionManager.SessionReceiverListThreadStatus.IsEvtRaiserSet())
                            _nna.TcpSessionManager.SessionReceiverListThreadStatus.RaiseThreadHealth();
                        else
                            logger.Warn("Could not raise thread health for " + _nna.TcpSessionManager.HandshakeReceiver.ThreadStatus.Description + ": Event Raiser is not instantiated.");

                        if (_nna.TcpSessionManager.SessionReceiverListThreadStatus.IsThreadHealthFailed)
                        {
                            //handshake receiver listener thread implementation
                            logger.Warn("[" + _description + "-T] Thread health failed [" + _nna.TcpSessionManager.SessionReceiverListThreadStatus.Description + "] last reported secs: " + _nna.TcpSessionManager.SessionReceiverListThreadStatus.CalculateThreadHealth().ToString("F"));

                            //stop and restart the entire adapter
                            logger.Info("[" + _description + "-T] Stopping [" + _nna.ThreadStatus.Description + "]");
                            _nna.Stop();
                            System.Threading.Thread.Sleep(5000);
                            logger.Info("[" + _description + "-T] Restarting [" + _nna.ThreadStatus.Description + "]");
                            _nna.Start();
                        }
                    }
                    catch (Exception ex11)
                    {
                        logger.Error("Could not raise thread health for " + _nna.TcpSessionManager.SessionReceiverListThreadStatus.Description + ": " + ex11.Message, ex11);
                    }

                    /*NiawaNetworkAdapter.TcpSessionManager.TcpSessions*/
                    foreach (KeyValuePair<string, TcpSession> sessionKvp in _nna.TcpSessionManager.TcpSessions)
                    {
                        bool sessionThreadFailed = false;

                        try
                        {
                            /*TcpSession.TcpReceiver*/

                            //raise thread health
                            if (sessionKvp.Value.Receiver.ThreadStatus.IsEvtRaiserSet())
                                sessionKvp.Value.Receiver.ThreadStatus.RaiseThreadHealth();
                            else
                                logger.Warn("Could not raise thread health for " + sessionKvp.Value.Receiver.ThreadStatus.Description + ": Event Raiser is not instantiated.");

                            if (sessionKvp.Value.Receiver.ThreadStatus.IsThreadHealthFailed)
                            {
                                logger.Warn("[" + _description + "-T] Thread health failed [" + sessionKvp.Value.Receiver.ThreadStatus.Description + "] last reported secs: " + sessionKvp.Value.Receiver.ThreadStatus.CalculateThreadHealth().ToString("F"));
                                sessionThreadFailed = true;
                            }
                        }
                        catch (Exception ex11)
                        {
                            logger.Error("Could not raise thread health for " + sessionKvp.Value.Receiver.ThreadStatus.Description + ": " + ex11.Message, ex11);
                        }

                        try
                        {
                            /*TcpSession.TcpTransmitter*/

                            //raise thread health
                            if (sessionKvp.Value.Transmitter.ThreadStatus.IsEvtRaiserSet())
                                sessionKvp.Value.Transmitter.ThreadStatus.RaiseThreadHealth();
                            else
                                logger.Warn("Could not raise thread health for " + sessionKvp.Value.Transmitter.ThreadStatus.Description + ": Event Raiser is not instantiated.");

                            if (sessionKvp.Value.Transmitter.ThreadStatus.IsThreadHealthFailed)
                            {
                                logger.Warn("[" + _description + "-T] Thread health failed [" + sessionKvp.Value.Transmitter.ThreadStatus.Description + "] last reported secs: " + sessionKvp.Value.Transmitter.ThreadStatus.CalculateThreadHealth().ToString("F"));
                                sessionThreadFailed = true;
                            }

                            /*TcpSession*/
                            if (sessionThreadFailed)
                            {
                                //remove session
                                logger.Warn("[" + _description + "-T] Removing session [" + sessionKvp.Value.Transmitter.ThreadStatus.Description + "]");
                                try
                                {
                                    _nna.TcpSessionManager.RemoveSession(sessionKvp.Key, false);
                                }
                                catch (Exception ex)
                                {
                                    logger.Warn("[" + _description + "-T] Could not remove session [" + sessionKvp.Value.Transmitter.ThreadStatus.Description + "]: " + ex.Message);
                                }
                            }
                        }
                        catch (Exception ex11)
                        {
                            logger.Error("Could not raise thread health for " + sessionKvp.Value.Transmitter.ThreadStatus.Description + ": " + ex11.Message, ex11);
                        }
                    }            
                    //done with sessions

                }
                catch (System.Threading.ThreadAbortException) // ex1)
                {
                    //thread was aborted
                    break;
                }
                catch (System.Threading.ThreadInterruptedException) // ex1)
                {
                    //thread was interrupted
                    break;
                }
                catch (Exception ex)
                {
                    //exception
                    logger.Error("[" + _description + "-T] Error in thread: " + ex.Message);
                    _threadStatus.ErrorCount += 1;
                                

                    System.Threading.Thread.Sleep(10000);
                }
            }
            _threadStatus.IsThreadActive = false;
            _threadStatus.IsThreadEnabled = false;
            logger.Info("[" + _description + "-T] Thread inactive");


        }

        public Niawa.Threading.ThreadStatus ThreadStatus
        {
            get { return _threadStatus; }
        }

        /*
        public bool IsThreadActive
        {
            get { return _threadStatus.ThreadActive; }
        }

        public bool IsThreadEnabled
        {
            get { return _threadStatus.ThreadEnabled; }
        }

        public DateTime ThreadHealthDate
        {
            get
            {
                return _threadStatus.ThreadHealthDate;
            }
        }
        */

        public void Dispose()
        {
            try
            {
                if (_threadStatus != null) _threadStatus.IsThreadActive = false;
                if (_threadStatus != null) _threadStatus.IsThreadEnabled = false;

                //thread status
                if (_threadStatus != null) _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_FINALIZED;

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error during dispose: " + ex.Message, ex);
                throw ex;
            }


        }
    }
}
