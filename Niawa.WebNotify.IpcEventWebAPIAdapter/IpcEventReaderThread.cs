using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.WebNotify.IpcEventWebAPIAdapter
{
    public class IpcEventReaderThread : IDisposable 
    {
        /* #threadPattern# */

        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /* Threading */
        private System.Threading.Thread t1;
        private Niawa.Threading.ThreadStatus _threadStatus = null;
    
        /* Parameters */
        private string _description;
        private string _ipcType;

        /* Resources */
        private Niawa.IpcController.iEventReader evtReader;
        private IpcEventWebAPIWriter _webWriter;
        private Niawa.Utilities.UtilsServiceBus _utilsBus;

        /// <summary>
        /// Instantiates a new IpcEventReaderThread.
        /// </summary>
        /// <param name="ipcType"></param>
        /// <param name="webWriter"></param>
        /// <param name="utilsBus"></param>
        public IpcEventReaderThread(string ipcType, IpcEventWebAPIWriter webWriter, Utilities.UtilsServiceBus utilsBus)
        {
            try
            {
                _description = "IpcEventReaderThread-" + ipcType;

                _ipcType = ipcType;
                _webWriter = webWriter;
                _utilsBus = utilsBus;

                //event reader 
                evtReader = Niawa.IpcController.IpcFactory.CreateIpcEventReader(true, _ipcType, _utilsBus);
                
                //thread status
                _threadStatus = new Niawa.Threading.ThreadStatus(_description, 60, utilsBus.InitializeSerialId(Niawa.Utilities.IdGeneratorUtils.ID_ROOT_NIAWA_THREAD_ID).ToString(), string.Empty, null);

                //set thread status
                _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_INITIALIZED;
            }
            catch (Exception ex)
            {
                logger.Error("[IpcEventReaderThread-" + ipcType + "-M] Error while instantiating: " + ex.Message, ex);
            }
          
        }

        /// <summary>
        /// Starts reader to listen for IPC Events, and starts event handler thread.
        /// </summary>
        public void Start()
        {
            try
            {
                logger.Info("[" + _description + "-M] Starting");

                if (_threadStatus.IsThreadEnabled)
                {
                    logger.Warn("Could not start " + _description + ": Thread was already active");
                }
                else
                {
                    _threadStatus.IsThreadEnabled = true;

                    //start event reader
                    evtReader.Start();
                  
                    //start thread
                    t1 = new System.Threading.Thread(ListenThreadImpl);
                    t1.Start();

                    //thread status
                    _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STARTED;

                }
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while starting: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Stops listener from reading IPC events, and stops event handler thread.
        /// </summary>
        /// <param name="abortPendingWork"></param>
        public void Stop(bool abortPendingWork)
        {
            try
            {
                logger.Info("[" + _description + "-M] Stopping");

                //stop event reader
                evtReader.Stop();
                
                //stop client
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
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while stopping: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// IPC Event listen thread.  Listens for IPC events and inserts into queue to be picked up by Web API Writer.
        /// </summary>
        /// <param name="data"></param>
        private void ListenThreadImpl(object data)
        {
            try
            {
                _threadStatus.IsThreadActive = true;
                logger.Info("[" + _description + "-T] Thread active");

                //active
                while (_threadStatus.IsThreadEnabled == true)
                {
                    _threadStatus.ThreadHealthDate = DateTime.Now;

                    //reading
                    try
                    {

                        Niawa.IpcController.IpcEvent evt = evtReader.ReadNextEvent();

                        logger.Debug("[" + _description + "-T] Received event ID [" + evt.SerialID.ToString() + "] sender [" + evt.ApplicationInstance + "] type [" + evt.EventType + "]");

                        //add message to writer queue
                        if (evt == null)
                        {
                            logger.Debug("[" + _description + "-T] Message is malformed");
                        }
                        else
                        {
                            _webWriter.AddMessageToQueue(evt);
                        }

                        
                        System.Threading.Thread.Sleep(50);
            
                    }
                    catch (System.Threading.ThreadAbortException) // ex1)
                    {
                        //thread was aborted
                        logger.Debug("[" + _description + "-T] Thread aborted");
                        break;
                    }
                    catch (System.Threading.ThreadInterruptedException) // ex1)
                    {
                        //thread was interrupted
                        logger.Debug("[" + _description + "-T] Thread interrupted");
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("[" + _description + "-T] Error while reading messages: " + ex.Message + "");
                        _threadStatus.MessageErrorCount += 1;
                        _threadStatus.ErrorCount += 1;

                        System.Threading.Thread.Sleep(100);
                    }
                    System.Threading.Thread.Sleep(10);
                }
                //done with write thread (stopped)

                logger.Info("[" + _description + "-T] Thread inactive");

            }
            catch (Exception ex)
            {
                _threadStatus.ErrorCount += 1;

                logger.Error("[" + _description + "-T] Error in listen thread: " + ex.Message, ex);
            }
            finally
            {
                _threadStatus.IsThreadActive = false;
                _threadStatus.IsThreadEnabled = false;
            }
        }

        public void Dispose()
        {
            Stop(true);
            if (evtReader != null) evtReader.Dispose();

        }
    }
}
