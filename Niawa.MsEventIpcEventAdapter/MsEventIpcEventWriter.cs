using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.MsEventIpcEventAdapter
{
    public class MsEventIpcEventWriter : IDisposable 
    {

        /* #threadPattern# */

        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        
        /* Globals */
        private string _description = string.Empty;

        /* Threading */
        private System.Threading.Thread t1;
        private Niawa.Threading.ThreadStatus _threadStatus = null;
        private bool _abortPendingWork;
        
        /* Resources */
        private Queue<Niawa.MsEventController.NiawaEventMessageArgs> _messages;
        
        /* Events */
        private SortedList<string, Niawa.IpcController.iEventWriter> _ipcEventWriters;
        private Niawa.MsEventController.EventConsumer _evtConsumer;

        
        /// <summary>
        /// MsEventIpcEventWriter consumes events (as raised by Niawa.MsEventController) and writes them to an IpcEventWriter (as implemented by Niawa.IpcController). 
        /// The event consumer is added (subscribed) to an event raiser by the caller to enable events to be processed in McEventIpcEventWriter.
        /// Multiple IpcEventWriters are registered here and events are written to the appropriate IpcEventWriter based on event type.
        /// </summary>
        public MsEventIpcEventWriter(Niawa.Utilities.UtilsServiceBus utilsBus)
        {

            try
            {
                _description = "MsEventIpcEventAdapter";
                _messages = new Queue<MsEventController.NiawaEventMessageArgs>();
                _evtConsumer = new MsEventController.EventConsumer(_messages);

                _ipcEventWriters = new SortedList<string, IpcController.iEventWriter>();
                _threadStatus = new Niawa.Threading.ThreadStatus(_description, 60, utilsBus.InitializeSerialId(Niawa.Utilities.IdGeneratorUtils.ID_ROOT_NIAWA_THREAD_ID).ToString(), string.Empty, null);

                //thread status
                _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_INITIALIZED;

            }
            catch (Exception ex)
            {
                logger.Error("[MsEventIpcEventAdapter-M] Error while instantiating: " + ex.Message, ex);
            }

        }

        /// <summary>
        /// Add an IpcEventWriter, to using for logging incoming events.  Multiple IpcEventWriters can be added, and the writer will be used according to the incoming event type.
        /// </summary>
        /// <param name="eventWriter"></param>
        /// <param name="description"></param>
        public void AddIpcEventWriter(Niawa.IpcController.iEventWriter eventWriter, string description)
        {
            try
            {
                logger.Info("[" + _description + "-M] Adding IpcEventWriter [" + description + "]");

                if (_ipcEventWriters.ContainsKey(description))
                    throw new Exception("[" + _description + "-M] There is already an event writer registered with description [" + description + "]");

                if (!eventWriter.IsActive) eventWriter.Start();
                             
                _ipcEventWriters.Add(description, eventWriter);

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while adding Ipc event writer: " + ex.Message, ex);
            }

        }

        /// <summary>
        /// Removes an IpcEventWriter.
        /// </summary>
        /// <param name="description"></param>
        public void RemoveIpcEventWriter(string description)
        {
            try
            {
                logger.Info("[" + _description + "-M] Removing IpcEventWriter [" + description + "]");

                if (!_ipcEventWriters.ContainsKey(description))
                    throw new Exception("[" + _description + "-M] Could not find an event writer registered with description [" + description + "]");

                //stop event writer
                Niawa.IpcController.iEventWriter eventWriter = _ipcEventWriters[description];
                if (eventWriter.IsActive) eventWriter.Stop();
                eventWriter.Dispose();

                //remove
                _ipcEventWriters.Remove(description);

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while removing Ipc event writer: " + ex.Message, ex);
            }


        }

        /// <summary>
        /// Start writing to Ipc event writers.  
        /// </summary>
        public void Start()
        {
            try
            {
                logger.Info("[" + _description + "-M] Starting");

                if (_threadStatus.IsThreadEnabled)
                {
                    logger.Warn("Could not start MsEventIpcEventAdapter: Thread was already active");
                }
                else
                {
                    _threadStatus.IsThreadEnabled = true;

                    _messages.Clear();

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
        /// Stop writing to Ipc event writers.
        /// </summary>
        /// <param name="abortPendingWork"></param>
        public void Stop(bool abortPendingWork)
        {

            try
            {
                logger.Info("[" + _description + "-M] Stopping");

                //stop client
                _abortPendingWork = abortPendingWork;
                _threadStatus.IsThreadEnabled = false;

                //wait for any pending work
                if (!abortPendingWork)
                {
                    int autoAbortCounter = 0;
                    while (_messages.Count > 0)
                    {
                        autoAbortCounter++;
                        //wait for work to finish
                        System.Threading.Thread.Sleep(100);
                        if (autoAbortCounter > 100)
                        {
                            logger.Warn("[" + _description + "-M] Could not finish pending work; aborting with [" + _messages.Count + "] item(s) in queue");
                            _abortPendingWork = true;
                            break;
                        }
                    }
                }

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
        /// Looping thread that checks for messages that have been queued by incoming events, and writes them to the appropriate Ipc event writer.
        /// </summary>
        /// <param name="data"></param>
        private void ListenThreadImpl(object data)
        {

            try
            {
                _threadStatus.IsThreadActive = true;
                logger.Info("[" + _description + "-T] Thread active");

                //active
                while (_threadStatus.IsThreadEnabled == true || (!_abortPendingWork && _messages.Count > 0))
                {
                    _threadStatus.ThreadHealthDate = DateTime.Now;

                    if (!_threadStatus.IsThreadEnabled && !_abortPendingWork && _messages.Count > 0)
                        logger.Info("[" + _description + "-T] Finishing pending work [" + _messages.Count + "]");

                    //writing
                    try
                    {
                        if (_messages.Count > 0)
                        {
                            //found item to write
                            _threadStatus.MessageCount += 1;

                            //dequeue item
                            MsEventController.NiawaEventMessageArgs message = _messages.Dequeue();
                            logger.Debug("[" + _description + "-T] Writing message " + message.SerialID + " [" + message.MessageType + "]");
                            
                            /************************
                            //write the message
                             ********************** */
                            WriteMessage(message);

                        }
                        else
                        {
                            //nothing to write
                            System.Threading.Thread.Sleep(50);

                        }

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
                        logger.Warn("[" + _description + "-T] Error while writing messages: " + ex.Message + "");
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

        /// <summary>
        /// Writes a message to the Ipc event writer.
        /// </summary>
        /// <param name="message"></param>
        private void WriteMessage(MsEventController.NiawaEventMessageArgs message)
        {
            if (_ipcEventWriters.ContainsKey(message.ApplicationGroup))
            {
                Niawa.IpcController.iEventWriter writer = _ipcEventWriters[message.ApplicationGroup];
                //writer.Write(message.MessageType, "[" + message.ApplicationInstance + "] " + message.Message + " " + message.MessageDetail);
                
                Niawa.IpcController.IpcEvent evt = new Niawa.IpcController.IpcEvent();
                evt.EventID = Guid.NewGuid();
                evt.EventDate = System.DateTime.Now;
                evt.ApplicationName = message.ApplicationName;
                evt.ApplicationInstance = message.ApplicationInstance;
                evt.EventType = message.MessageType;
                evt.EventMessage = message.Message;
                evt.EventMessageDetail = message.MessageDetail;
                evt.NodeID = message.NodeID;
                evt.ParentNodeID = message.ParentNodeID;

                //if(message.ApplicationInstance.Trim().Length > 0) evt.AddProperty("instance", message.ApplicationInstance);
                //if(message.MessageDetail.Trim().Length > 0) evt.AddProperty("detail", message.MessageDetail);

                writer.Write(evt);

            }
            else
            {
                logger.Warn("[MsEventIpcEventAdapter] Could not find an event writer registered for [" + message.ApplicationGroup + "]; discarding message " + message.SerialID);
            }

        }

        public Niawa.MsEventController.EventConsumer EvtConsumer
        {
            get { return _evtConsumer; }
        }

        public Niawa.Threading.ThreadStatus ThreadStatus
        {
            get { return _threadStatus; }
        }

        /*
        public bool IsThreadEnabled
        {
            get { return _threadStatus.ThreadEnabled; }
        }

        public bool IsThreadActive
        {
            get { return _threadStatus.ThreadActive; }
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
                _abortPendingWork = true;
                if (_threadStatus != null) _threadStatus.IsThreadActive = false;
                if (_threadStatus != null) _threadStatus.IsThreadEnabled = false;

                //stop all existing writers
                foreach (KeyValuePair<string, Niawa.IpcController.iEventWriter> kvp in _ipcEventWriters)
                {
                    kvp.Value.Stop();
                    kvp.Value.Dispose();
                }

                _ipcEventWriters.Clear();

                //thread status
                if (_threadStatus != null) _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_FINALIZED;

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while disposing: " + ex.Message, ex);
            }

        }
    }
}
