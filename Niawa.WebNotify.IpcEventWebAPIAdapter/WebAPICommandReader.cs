using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Niawa.WebNotify.IpcEventWebAPIAdapter
{
    public class WebAPICommandReader : IDisposable 
    {
        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /* Threading */
        private System.Threading.Thread t1;
        private Niawa.Threading.ThreadStatus _threadStatus = null;
        private bool _abortPendingWork;
        private string _description;

        /* Locking */
        System.Threading.Semaphore lockSection;
  
        /*Parameters*/
        private string _webApiUrl;
        private int _pollIntervalMs = 0;

        /* Resources */
        private Queue<NiawaWebMessage> _messages = null;
        private SortedList<int, NiawaWebMessage> _cachedMessages = null;
        private Niawa.Utilities.UtilsServiceBus _utilsBus;
        private List<int> _subscribedCommands;

        /// <summary>
        /// Instantiates a new WebAPICommandReader.
        /// </summary>
        /// <param name="webApiUrl"></param>
        /// <param name="utilsBus"></param>
        public WebAPICommandReader(string webApiUrl, int pollIntervalMs,  Niawa.Utilities.UtilsServiceBus utilsBus)
        {
            try 
            {
                _description = "WebAPICommandReader";
                _webApiUrl = webApiUrl;
                _pollIntervalMs = pollIntervalMs;
                if (pollIntervalMs < 1000)
                {
                    logger.Warn("[WebAPICommandReader-M] Warning: poll interval under minimum, setting to 1000 ms");
                    _pollIntervalMs = 1000;
                }
                if (pollIntervalMs > 60000)
                {
                    logger.Warn("[WebAPICommandReader-M] Warning: poll interval over maximum, setting to 60000 ms");
                    _pollIntervalMs = 60000;
                }


                lockSection = new System.Threading.Semaphore(1, 1);

                _utilsBus = utilsBus;
                _messages = new Queue<NiawaWebMessage>();
                _cachedMessages = new SortedList<int, NiawaWebMessage>();
                _subscribedCommands = new List<int>();

                _threadStatus = new Niawa.Threading.ThreadStatus(_description, 60, utilsBus.InitializeSerialId(Niawa.Utilities.IdGeneratorUtils.ID_ROOT_NIAWA_THREAD_ID).ToString(), string.Empty, null);
                //thread status
                _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_INITIALIZED;
  
            }
            catch (Exception ex)
            {
                logger.Error("[WebAPICommandReader-M] Error while instantiating: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Subscribe to a command.  The web reader will start polling for new messages for this ID.
        /// </summary>
        /// <param name="commandID"></param>
        public void CommandSubscribe(int commandID)
        {
            try
            {
                logger.Info("[" + _description + "-M] Subscribing to command [" + commandID + "]");

                //attempt lock
                bool tryLock = lockSection.WaitOne(60000);
                if (!tryLock) throw new TimeoutException("[" + _description + "-M] Could not obtain lock while adding message to queue");

                try
                {

                    if (_subscribedCommands.Contains(commandID))
                    {
                        //already subscribed
                    }
                    else
                    {
                        //add
                        _subscribedCommands.Add(commandID);
                    }

                }
                finally
                {
                    //release lock
                    lockSection.Release();
                }

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while subscribing to command: " + ex.Message, ex);
            }

        }

        /// <summary>
        /// Unsubscribe from a command.  The web reader will stop polling for new messages for this ID.
        /// </summary>
        /// <param name="commandID"></param>
        public void CommandUnsubscribe(int commandID)
        {
            try
            {
                logger.Info("[" + _description + "-M] Unsubscribing from command [" + commandID + "]");

                //attempt lock
                bool tryLock = lockSection.WaitOne(60000);
                if (!tryLock) throw new TimeoutException("[" + _description + "-M] Could not obtain lock while adding message to queue");

                try
                {
                    if (_subscribedCommands.Contains(commandID))
                    {
                        //remove
                        _subscribedCommands.Remove(commandID);
                    }
                    else
                    {
                        //already removed
                    }

                }
                finally
                {
                    //release lock
                    lockSection.Release();
                }
                
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while unsubscribing from command: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Start the Web API command reader.  Stops polling web resources for messages.
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
        /// Stop the Web API Command Reader.  Starts polling web resource in a separate thread.
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
        /// Poll web resource for subscribed commands.  If a command is found, add it to the queue.
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

                    try
                    {
                    
                        //read messages
                        ReadMessages();

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

        /// <summary>
        /// Read messages from web api resource
        /// </summary>
        private void ReadMessages()
        {
            try
            {
                //attempt lock
                bool tryLock = lockSection.WaitOne(60000);
                if (!tryLock) throw new TimeoutException("[" + _description + "-M] Could not obtain lock while adding message to queue");

                try
                {
                    foreach (int commandID in _subscribedCommands)
                    {
                        ReadMessage(commandID).Wait();
                    }


                    logger.Info("[" + _description + "-T] Sleeping for " + _pollIntervalMs + " ms");
                    System.Threading.Thread.Sleep(_pollIntervalMs);

                }
                finally
                {
                    //release lock
                    lockSection.Release();
                }

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Error while reading messages: " + ex.Message, ex);
            }
        }

        private async Task ReadMessage(int commandID)
        {
            //read message
            using (var client = new HttpClient())
            {

                // Establish connection:
                client.BaseAddress = new Uri(_webApiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue
                ("application/json"));

                //HTTP GET
                HttpResponseMessage response = await client.GetAsync("api/command/" + commandID);
                if (response.IsSuccessStatusCode)
                {
                    NiawaWebMessage message = await response.Content.ReadAsAsync<NiawaWebMessage>();
                    //Console.WriteLine("{0}\t${1}\t{2}", product.Name, product.Price, product.Category);
               
                    //compare
                    bool newMessage = false;
                    if (_cachedMessages.ContainsKey(commandID))
                    {
                        NiawaWebMessage cachedMessage = _cachedMessages[commandID];
                        if (cachedMessage.CreatedDate.Equals(message.CreatedDate))
                        {
                            //the date has not changed
                            newMessage = false;
                        }
                        else
                        {
                            //the date has changed
                            newMessage = true;
                        }

                    }
                    else
                    {
                        //don't have this message - it's new
                        newMessage = true;
                    }
                    if (newMessage)
                    {
                        //the message is new
                        if (message.Id != commandID)
                        {
                            //the message is invalid
                            logger.Error("[" + _description + "-T] Ignoring invalid message: the ID [" + message.Id + "] does not match the channel command ID [" + commandID + "]");
                                       }
                        else
                        {
                            //the message is valid
                            logger.Info("[" + _description + "-T] Received new message: command [" + message.Id + "]");

                            if (_cachedMessages.ContainsKey(commandID))
                                _cachedMessages.Remove(commandID);

                            //cache for later comparison
                            _cachedMessages.Add(commandID, message);

                            //add to queue for consumption
                            _messages.Enqueue(message);
                        }

                    }
                    else
                    {
                        //the message is not new
                    }


                }
                else
                {
                    logger.Warn("[" + _description + "-T] Failed to read message from HttpClient. HttpResponseMessage: " + response.ToString());
                }

            }
        }

        /// <summary>
        /// Messages property
        /// </summary>
        public Queue<NiawaWebMessage> Messages
        {
            get { return _messages; }
        }

        /// <summary>
        /// Dispose object
        /// </summary>
        public void Dispose()
        {
            if (_threadStatus != null) _threadStatus.IsThreadActive = false;
            if (_threadStatus != null) _threadStatus.IsThreadEnabled = false;

            Stop(true);

            //thread status
            if (_threadStatus != null) _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_FINALIZED;

        }
    }
}
