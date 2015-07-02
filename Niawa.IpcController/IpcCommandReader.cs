using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcController
{
    public class IpcCommandReader : IDisposable
    {
        /* #threadPattern# */
        
        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        
        /* Globals */
        private string _description = string.Empty;

        /* Parameters */
        private string _ipcEvent;
        private Func<Niawa.IpcController.IpcEvent, bool> _readerFunction;
        private bool _ignoreExceptions;

        /* Events */
        Niawa.IpcController.iEventReader evtReader;

        /* Threading */
        private System.Threading.Thread t1;
        private Niawa.Threading.ThreadStatus _threadStatus = null;
        
        /// <summary>
        /// Instantiates IpcCommandReader
        /// </summary>
        /// <param name="ipcEvent"></param>
        /// <param name="readerFunction"></param>
        public IpcCommandReader(string ipcEvent, Func<Niawa.IpcController.IpcEvent, bool> readerFunction, Niawa.Utilities.UtilsServiceBus utilsBus, bool ignoreExceptions)
        {
            try
            {
                _ignoreExceptions = ignoreExceptions;
                _ipcEvent = ipcEvent;
                _description = "IpcCommandReader " + _ipcEvent;
                _readerFunction = readerFunction;
                evtReader = Niawa.IpcController.IpcFactory.CreateIpcEventReader(true, _ipcEvent, utilsBus);

                _threadStatus = new Niawa.Threading.ThreadStatus(_description, 60, utilsBus.InitializeSerialId(Niawa.Utilities.IdGeneratorUtils.ID_ROOT_NIAWA_THREAD_ID).ToString(), string.Empty, null);

                //thread status
                _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_INITIALIZED;

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Exception while instantiating: " + ex.Message, ex);
                if (!ignoreExceptions)
                    throw ex;
            }
        }

        /// <summary>
        /// Starts IpcCommandReader
        /// </summary>
        public void Start()
        {
            try
            {

                logger.Info("[" + _description + "-M] Starting");

                if (_threadStatus.IsThreadEnabled)
                {
                    logger.Warn("[" + _description + "-M] Could not start: Command Reader thread was already active");
                }
                else
                {
                    _threadStatus.IsThreadEnabled = true;

                    evtReader.Start();
                    t1 = new System.Threading.Thread(ListenThreadImpl);
                    t1.Start();

                    //thread status
                    _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STARTED;

                }
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Exception while starting: " + ex.Message, ex);
                if (!_ignoreExceptions)
                    throw ex;
            }

        }

        /// <summary>
        /// Stops IpcCommandReader
        /// </summary>
        public void Stop()
        {
            try
            {
                logger.Info("[" + _description + "-M] Stopping");

                _threadStatus.IsThreadEnabled = false;

                evtReader.Stop();

                //thread status
                _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STOPPED;

                //wait for up to 2.5 seconds for thread to end, then abort if not finished
                int timeoutIx = 0;
                while (t1.IsAlive)
                {
                    timeoutIx++;
                    System.Threading.Thread.Sleep(50);
                    if (timeoutIx > 50) break;
                }

                //abort if thread is still alive
                if (t1.IsAlive)
                {
                    t1.Abort();
                }

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Exception while stopping: " + ex.Message, ex);
                if (!_ignoreExceptions)
                    throw ex;
            }
        }

        /// <summary>
        /// Loop thread that reads the next event from the Ipc Event Reader and 
        /// passes the event to the provided function.
        /// </summary>
        /// <param name="data"></param>
        private void ListenThreadImpl(object data)
        {
            try
            {
                _threadStatus.IsThreadActive = true;
                logger.Info("[" + _description + "-T] Command reader active");

                while (_threadStatus.IsThreadEnabled)
                {
                    _threadStatus.ThreadHealthDate = DateTime.Now;

                    try
                    {
                        //receiving event
                        Niawa.IpcController.IpcEvent evt = evtReader.ReadNextEvent();

                        _threadStatus.MessageCount += 1;

                        /*
                        string evtPropertiesSerialized = string.Empty;
                        if (evt.EventProperties != null && evt.EventProperties.Count > 0)
                            evtPropertiesSerialized = Newtonsoft.Json.JsonConvert.SerializeObject(evt.EventProperties);
                        */

                        //call the passed-in function
                        bool success = _readerFunction(evt);

                        System.Threading.Thread.Sleep(50);
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
                        logger.Error("[" + _description + "-T] Error listening for next event: " + ex.Message);

                        System.Threading.Thread.Sleep(100);
                    }
                }
                _threadStatus.IsThreadActive = false;
                _threadStatus.IsThreadEnabled = false;
                logger.Info("[" + _description + "-T] Command reader inactive");

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Exception in listen thread: " + ex.Message, ex);
                _threadStatus.ErrorCount += 1;
                if (!_ignoreExceptions)
                    throw ex;
            }


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
