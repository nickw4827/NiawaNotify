using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Niawa.WebNotify.Common;

namespace Niawa.WebNotify.IpcEventWebAPIAdapter
{
    public class IpcEventWebAPIWriter : IDisposable 
    {
        /* #threadPattern# */

        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /* Threading */
        private System.Threading.Thread t1;
        private Niawa.Threading.ThreadStatus _threadStatus = null;
        private bool _abortPendingWork;

        /* Locking */
        System.Threading.Semaphore lockSection;
      
        /* Parameters */
        private string _description;
        private string _webApiUrl;

        /* Resources */
        private Queue<Niawa.IpcController.IpcEvent> _messages;
        private Queue<Niawa.IpcController.IpcEvent> _messagesSyncd;

        private SortedList<string, IpcEventReaderThread> _eventReaders;
        private Niawa.Utilities.UtilsServiceBus _utilsBus;

        /// <summary>
        /// Instantiates a new IpcEventWebApiWriter.
        /// </summary>
        /// <param name="webApiUrl"></param>
        /// <param name="utilsBus"></param>
        public IpcEventWebAPIWriter(string webApiUrl, Niawa.Utilities.UtilsServiceBus utilsBus)
        {
            try
            {
                _description = "IpcEventWebAPIWriter";
                _webApiUrl = webApiUrl;

                _utilsBus = utilsBus;
                _messages = new Queue<IpcController.IpcEvent>();
                _eventReaders = new SortedList<string, IpcEventReaderThread>();
                lockSection = new System.Threading.Semaphore(1, 1);

                _threadStatus = new Niawa.Threading.ThreadStatus(_description, 60, utilsBus.InitializeSerialId(Niawa.Utilities.IdGeneratorUtils.ID_ROOT_NIAWA_THREAD_ID).ToString(), string.Empty, null);

                //thread status
                _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_INITIALIZED;
            }
            catch (Exception ex)
            {
                logger.Error("[IpcEventWebAPIWriter-M] Error while instantiating: " + ex.Message, ex);
            }

        }

        /// <summary>
        /// Adds an IPC Event reader for the specified IPC Type and starts listening for messages.
        /// When a message is received, the message is queued to be written by the Web API Writer.
        /// </summary>
        /// <param name="ipcType"></param>
        public void AddIpcEventReader(string ipcType)
        {
            try
            {
                logger.Info("[" + _description + "-M] Adding IpcEventReader [" + ipcType + "]");
                
                if (_eventReaders.ContainsKey(ipcType))
                    throw new Exception("[" + _description + "-M] There is already an ipc event reader registered for IPC Type [" + ipcType + "]");

                IpcEventReaderThread readerThread = new IpcEventReaderThread(ipcType, this, _utilsBus);
                
                //add
                _eventReaders.Add(ipcType, readerThread);

                //start event reader
                readerThread.Start();
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while adding Ipc event reader: " + ex.Message, ex);
            }

        }

        /// <summary>
        /// Removes and IPC Event reader.
        /// </summary>
        /// <param name="ipcType"></param>
        public void RemoveIpcEventReader(string ipcType)
        {
            try
            {

                logger.Info("[" + _description + "-M] Removing IpcEventReader [" + ipcType + "]");
            
                if (!_eventReaders.ContainsKey(ipcType))
                    throw new Exception("[" + _description + "-M] Could not find ipc event reader registered for IPC Type [" + ipcType + "]");
            
                IpcEventReaderThread readerThread = _eventReaders[ipcType];

                //stop event reader
                readerThread.Stop(false);
                readerThread.Dispose();

                //remove
                _eventReaders.Remove(ipcType);

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while removing Ipc event reader: " + ex.Message, ex);
            }


        }

        /// <summary>
        /// Removes all IPC Event Readers.
        /// </summary>
        public void RemoveAllIpcEventReaders()
        {
            try
            {
                logger.Info("[" + _description + "-M] Removing all Ipc Event Readers");

                foreach (KeyValuePair<string, IpcEventReaderThread> kvp in _eventReaders)
                {
                    RemoveIpcEventReader(kvp.Key);
                }
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while removing all Ipc event readers: " + ex.Message, ex);
            }

        }

        /// <summary>
        /// Starts Web API writer.
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
        /// Stops Web API writer and removes all Ipc Event readers.
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

                //remove all IPC Event readers
                RemoveAllIpcEventReaders();

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while stopping: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Adds a message to the Web API Queue to be written.
        /// </summary>
        /// <param name="message"></param>
        public void AddMessageToQueue(Niawa.IpcController.IpcEvent message)
        {
            try
            {
                //attempt lock
                bool tryLock = lockSection.WaitOne(60000);
                if (!tryLock) throw new TimeoutException("[" + _description + "-M] Could not obtain lock while adding message to queue");

                try
                {
                    //add message to queue
                    _messages.Enqueue(message);
                }
                finally
                {
                    //release lock
                    lockSection.Release();
                }

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while adding message to queue: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Looping thread that checks for messages from the IPC Event Readers, and writes them out to the Web API Writer.
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
                            IpcController.IpcEvent message = _messages.Dequeue();
                            if (message == null || message.SerialID == null || message.EventType == null)
                            {
                                logger.Warn("[" + _description + "-T] Message is malformed");
                            }
                            else
                            {
                                logger.Debug("[" + _description + "-T] Writing message " + message.SerialID + " [" + message.EventType + "]");
                            }

                            
                            /************************
                            //write the message
                             ********************** */
                            WriteMessage(message).Wait();

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
        /// Writes a message to the Web API Writer.
        /// </summary>
        /// <param name="message"></param>
        private async Task WriteMessage(IpcController.IpcEvent message)
        {
            try
            {
                //write message
                using (var client = new HttpClient())
                {

                    // Establish connection:
                    client.BaseAddress = new Uri(_webApiUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue
                    ("application/json"));

                    // HTTP POST
                    var newMessage = new NiawaWebMessage() { Sender = Environment.MachineName, Message = message.ToJson() };
                    HttpResponseMessage response = await client.PostAsJsonAsync("api/message", newMessage);

                    if (response.IsSuccessStatusCode)
                    {
                        // Get the URI of the created resource.
                        Uri messageUrl = response.Headers.Location;
                    }
                    else
                    {
                        logger.Error("[" + _description + "-T] Failed to write message to HttpClient. HttpResponseMessage: " + response.ToString());
                    }

                }

            }
            catch (Exception ex)
            {
                _threadStatus.ErrorCount += 1;
                _threadStatus.MessageErrorCount += 1;

                logger.Error("[" + _description + "-T] Error writing mesage to web client: " + ex.Message, ex);
            }


            //original code
            /*
            // Establish connection:
            client.BaseAddress = new Uri("http://localhost:3465/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue
            ("application/json"));

                
            // Get message:
            HttpResponseMessage response = await client.GetAsync("api/message/1");
            if (response.IsSuccessStatusCode)
            {
                NiawaWebMessage message = await response.Content.ReadAsAsync<NiawaWebMessage>();
                Console.WriteLine("{0}\t{1}\t{2}", message.Id, message.Sender, message.Message);
            }
                

            // HTTP POST
            var newMessage = new NiawaWebMessage() { Sender = Environment.MachineName, Message = "Test message " + DateTime.Now.ToString() };
            response = await client.PostAsJsonAsync("api/message", newMessage);
            if (response.IsSuccessStatusCode)
            {
                // Get the URI of the created resource.
                Uri messageUrl = response.Headers.Location;
            }

                
            // Get message just posted:
            HttpResponseMessage response2 = await client.GetAsync("api/message/2");
            if (response2.IsSuccessStatusCode)
            {
                NiawaWebMessage message2 = await response.Content.ReadAsAsync<NiawaWebMessage>();
                Console.WriteLine("{0}\t{1}\t{2}", message2.Id, message2.Sender, message2.Message);
            }
            */
           
        }


        public void Dispose()
        {
            _abortPendingWork = true;
            if (_threadStatus != null) _threadStatus.IsThreadActive = false;
            if (_threadStatus != null) _threadStatus.IsThreadEnabled = false;

            Stop(true);
            RemoveAllIpcEventReaders();

            _eventReaders.Clear();

            //thread status
            if (_threadStatus != null) _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_FINALIZED;

        }
    }


}
