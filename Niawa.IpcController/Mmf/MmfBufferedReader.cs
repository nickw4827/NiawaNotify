using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.MemoryMappedFiles;

namespace Niawa.IpcController
{
    /// <summary>
    /// Reads messages from a memory-mapped file in a continuous loop.
    /// New data is identified by an incremented message id and is added to a read queue.
    /// </summary>
    public class MmfBufferedReader : IDisposable 
    {
        /* #threadPattern# */
        
        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /* Parameters */
        private string _ipcType;
        private int _bufferLength;
        private bool _ignoreExceptions;

        /* Globals */
        private int _lastMsgID;
        private int _lastHeaderMsgID = 0;
        private NiawaMmfBufferHeader _previousCachedHeader;
        private NiawaMmfBufferHeader _cachedHeader;
        private string _description = string.Empty;

        /* Resources */
        private Queue<NiawaMmfContainer> _receiveQueue;
        private DateTime _minimumMessageDate = DateTime.MinValue;

        /* Threading */
        private System.Threading.Thread t1;
        private Niawa.Threading.ThreadStatus _threadStatus = null;
        private bool _abortPendingWork;
        
        /// <summary>
        /// Instantiates MmfBufferedReader.
        /// </summary>
        /// <param name="ipcType">Identifies memory mapped file and used to authenticate incoming data.</param>
        /// <param name="bufferLength">Length of data segment</param>
        /// <param name="ignoreExceptions">True if exceptions should be handled; false if they should be thrown to caller</param>
        public MmfBufferedReader(string ipcType, int bufferLength, Niawa.Utilities.UtilsServiceBus utilsBus, bool ignoreExceptions)
        {
            try
            {
                _receiveQueue = new Queue<NiawaMmfContainer>();
                
                _ipcType = ipcType;
                _description = "MmfReader " + _ipcType;
                _bufferLength = bufferLength;
                _ignoreExceptions = ignoreExceptions;

                _lastMsgID = 0;
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
        /// Start reading the memory mapped file in a loop thread.
        /// </summary>
        public void StartListening()
        {
            try
            {
                logger.Info("[" + _description + "-M] Starting");

                if (_threadStatus.IsThreadEnabled)
                {
                    logger.Warn("[" + _description + "-M] Could not start: Reader thread was already active");
                }
                else
                {
                   
                    //start thread
                    t1 = new System.Threading.Thread(ListenThreadImpl);

                    _receiveQueue.Clear();

                    _cachedHeader = InitializeCachedHeader(_cachedHeader);
                    _previousCachedHeader = InitializeCachedHeader(_previousCachedHeader);
                    _minimumMessageDate = DateTime.Now;

                    _threadStatus.IsThreadEnabled = true;
                
                    //start client
                    t1.Start();

                    //thread status
                    _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STARTED;

                }

                
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Exception during start listening: " + ex.Message, ex);
                if (!_ignoreExceptions)
                    throw ex;
            }
        }

        /// <summary>
        /// Stop loop thread that is reading the memory mapped file.
        /// </summary>
        public void StopListening(bool abortPendingWork)
        {
            try
            {
                logger.Info("[" + _description + "-M] Stopping");

                //stop client
                _abortPendingWork = abortPendingWork;
                _threadStatus.IsThreadEnabled = false;

                //thread status
                _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STOPPED;

                //wait for up to 10 seconds for thread to end, then abort if not finished
                int timeoutIx = 0;
                while (t1.IsAlive)
                {
                    timeoutIx ++;
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
                logger.Error("[" + _description + "-M] Exception during stop listening: " + ex.Message, ex);
                if (!_ignoreExceptions)
                    throw ex;
            }
        }

        /// <summary>
        /// Number of messages that are in the read buffer.
        /// </summary>
        /// <returns></returns>
        public int CountMessagesInBuffer()
        {
            try
            {
                //returns number of messages in queue
                return _receiveQueue.Count;
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Exception while counting messages in buffer: " + ex.Message, ex);
                if (!_ignoreExceptions)
                    throw ex;
                return -1;
            }
        }

        /// <summary>
        /// Dequeues the next message from the read buffer.
        /// </summary>
        /// <returns></returns>
        public NiawaMmfContainer GetNextMessageFromBuffer()
        {
            try
            {
                //dequeues next message
                return _receiveQueue.Dequeue();
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Exception while getting next message from buffer: " + ex.Message, ex);
                if (!_ignoreExceptions)
                    throw ex;
                return null;
            }
        }

        /// <summary>
        /// Loop thread that reads the memory mapped file and adds items to the read queue.
        /// </summary>
        /// <param name="data"></param>
        private void ListenThreadImpl(object data)
        {
            try
            {
                _threadStatus.IsThreadActive = true;
                logger.Info("[" + _description + "-T] Reader active");

                try
                {

                    while (_threadStatus.IsThreadEnabled == true)
                    {
                        _threadStatus.ThreadHealthDate = DateTime.Now;

                        DateTime errMsgThrottleDate = DateTime.MinValue;

                        //listening
                        try
                        {
                            //read from buffer header file
                            bool headerFileChanged = ReadFromMmfHeaderFile();
                            if (headerFileChanged)
                            {
                                //System.Diagnostics.Debug.WriteLine("MmfBufferedReader [Thread]: Buffer header file changed");

                                //move between the previous latest entry id and the current one.
                                //if the current one is less than the previous, move from 100 to 1 and continue
                                
                                int previousEntryID = _previousCachedHeader.LatestEntryID;
                                int newEntryID = _cachedHeader.LatestEntryID;
                                int numberOfIterations = 0;

                              
                                //determine number of iterations
                                if (previousEntryID > newEntryID) numberOfIterations = (newEntryID + 100) - previousEntryID;
                                if (previousEntryID <= newEntryID) numberOfIterations = (newEntryID - previousEntryID);

                                //System.Diagnostics.Debug.WriteLine("MmfBufferedReader [Thread]: PreviousEntryID: " + previousEntryID);
                                //System.Diagnostics.Debug.WriteLine("MmfBufferedReader [Thread]: NewEntryID: " + newEntryID);
                                //System.Diagnostics.Debug.WriteLine("MmfBufferedReader [Thread]: NumberOfIterations: " + numberOfIterations);

                                //validations
                                if (previousEntryID < 0 || previousEntryID > 100) throw new IndexOutOfRangeException("PreviousCachedHeader LatestEntryID value [" + previousEntryID + "] is invalid; expected value between 0 and 100");
                                if (newEntryID < 0 || newEntryID > 100) throw new IndexOutOfRangeException("CachedHeader LatestEntryID value [" + newEntryID + "] is invalid; expected value between 0 and 100");
                                if (numberOfIterations < 0 || numberOfIterations > 100) throw new IndexOutOfRangeException("Number of iterations calculated value [" + numberOfIterations + "] is invalid; expected value between 0 and 100");
                                
                                //set current esntry to starting number
                                int currentEntryId = previousEntryID + 1;

                                //System.Diagnostics.Debug.WriteLine("MmfBufferedReader [Thread]: CurrentEntryID: " + previousEntryID);

                                if (numberOfIterations == 0)
                                {
                                    //no work to do
                                    //System.Diagnostics.Debug.WriteLine("MmfBufferedReader [Thread]: There is no work to do");
                                }
                                else
                                {
                                    int ix = 0;
                                    while (ix < numberOfIterations)
                                    {
                                        //increase iteration count
                                        ix++;

                                        //reset current entry if over 100
                                        if (currentEntryId > 100) currentEntryId = 1;

                                        //System.Diagnostics.Debug.WriteLine("MmfBufferedReader [Thread]: Index: " + ix);

                                        //System.Diagnostics.Debug.WriteLine("MmfBufferedReader [Thread]: Reading from MMF File [" + currentEntryId + "]");

                                        //read from MMF file for this entry
                                        ReadFromMmfFile(currentEntryId);

                                        //increase current entry ID
                                        currentEntryId++;

                                        //if iterations are more than 100, there is an error
                                        if (ix > 100) throw new IndexOutOfRangeException("The number of iterations through the buffer header exceeded the max (100)");

                                    }

                                }


                            }
                            else
                            {
                                //buffer header file did not change
                                //System.Diagnostics.Debug.WriteLine("MmfBufferedReader [Thread]: Buffer header file did not change");
                            }

                        }
                        catch (System.IO.FileNotFoundException)
                        {
                            //file does not exist
                            System.Diagnostics.Debug.WriteLine("MmfBufferedReader [" + _ipcType + "][Thread]: File was not found");
                
                            //log message every 10 seconds
                            TimeSpan ts = DateTime.Now - errMsgThrottleDate;
                            if (ts.TotalSeconds > 10)
                            {
                                errMsgThrottleDate = DateTime.Now;
                                logger.Debug("[" + _description + "-T] Mmf not initiated by provider");
                            }

                            //sleep for 250 ms
                            System.Threading.Thread.Sleep(250);
                        }

                        catch (System.Threading.ThreadAbortException) // ex1)
                        {
                            //thread was aborted; re-enter loop
                            logger.Info("[" + _description + "-T] Thread aborted");
                            break;
                        }
                        catch (System.Threading.ThreadInterruptedException) // ex1)
                        {
                            //thread was interrupted; re-enter loop
                            logger.Info("[" + _description + "-T] Thread interrupted");
                            break;
                        }
                        catch (Exception ex)
                        {
                            //exception
                            logger.Error("[" + _description + "-T] Exception while listening for next message: " + ex.Message, ex);

                            _threadStatus.ErrorCount += 1;

                            System.Threading.Thread.Sleep(100);
                        }
                        System.Threading.Thread.Sleep(50);

                        
                    }
                    //done listening
                 
                }
                finally
                {
                    _threadStatus.IsThreadActive = false;
                    _threadStatus.IsThreadEnabled = false;
                    logger.Info("[" + _description + "-T] Reader inactive");
                }

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-T] Exception in listen thread: " + ex.Message, ex);
                if (!_ignoreExceptions)
                    throw ex;
            }
        }

        private NiawaMmfBufferHeader InitializeCachedHeader(NiawaMmfBufferHeader cachedHeader)
        {
            try
            { 
                //create a new buffer header
                cachedHeader = new NiawaMmfBufferHeader();
                SortedList<int, KeyValuePair<string, DateTime>> entries = new SortedList<int, KeyValuePair<string, DateTime>>();
                //populate the buffer header with blank values
                int ix = 1;
                while (ix <= 100)
                {
                    entries.Add(ix, new KeyValuePair<string, DateTime>(string.Empty, new DateTime(1900, 1, 1)));
                    ix++;
                }
                cachedHeader.Entries = entries;
                cachedHeader.LatestUpdateDate = new DateTime(1900, 1, 1);
                cachedHeader.LatestEntryID = 0;

                return cachedHeader;

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Exception while initializing buffer header: " + ex.Message, ex);
                if (!_ignoreExceptions)
                    throw ex;

                return null;
            }

        }

        /// <summary>
        /// Reads data from Mmf Header file into cached header object
        /// </summary>
        private bool ReadFromMmfHeaderFile()
        {
            
            /* Memory Mapped File:
            * accessor1Len:  bytes 0-4       total length of file
            * accessor2ID:   bytes 8-12      ID number of message
            * accessor3Data: variable bytes  payload
            * */

            int msgLength;
            int msgID;

            MemoryMappedFile mmf;

            //System.Diagnostics.Debug.WriteLine("MmfBufferedReader [Header]: Filename " + _ipcType + "_header");
        
            try
            {
                //open mmf
                mmf = MemoryMappedFile.OpenExisting(_ipcType + "_header");
            }
            catch (System.IO.FileNotFoundException ex)
            {

                System.Diagnostics.Debug.WriteLine("MmfBufferedReader [" + _ipcType + "][Header]: Header file was not found");
                throw ex;
            }
            
            MemoryMappedViewAccessor accessor1Len = mmf.CreateViewAccessor(0, 4);
            MemoryMappedViewAccessor accessor2ID = mmf.CreateViewAccessor(8, 4);
            MemoryMappedViewAccessor accessor3Data;

            //locked section
            using (new Niawa.Utilities.SingleGlobalInstance(1000, _ipcType)) //1000ms timeout on global lock
            {

                //get length, ID, and data
                msgLength = accessor1Len.ReadInt32(0);
                msgID = accessor2ID.ReadInt32(0);
                accessor3Data = mmf.CreateViewAccessor(16, msgLength);
            }

            //System.Diagnostics.Debug.WriteLine("MmfBufferedReader [Header]: HeaderID:" + msgID);
            //System.Diagnostics.Debug.WriteLine("MmfBufferedReader [Header]: LastHeaderID:" + _lastHeaderMsgID);

                
            //check if ID changed
            if (msgID == _lastHeaderMsgID)
            {
                //System.Diagnostics.Debug.WriteLine("MmfBufferedReader [Header]: There is no header work to do");

                //do nothing
                return false;
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine("MmfBufferedReader [Header]: Reading header");

                //set ID
                _lastHeaderMsgID = msgID;

                //get data
                byte[] buffer = new byte[msgLength];
                accessor3Data.ReadArray<byte>(0, buffer, 0, msgLength);

                NiawaMmfBufferHeader msg = new NiawaMmfBufferHeader(buffer);
                _previousCachedHeader = _cachedHeader;
                _cachedHeader = msg;

                return true;
            }
            
        }

        /// <summary>
        /// Reads data from Mmf file and inserts into read queue
        /// </summary>
        private void ReadFromMmfFile(int entryID)
        {
            /* Memory Mapped File:
            * accessor1Len:  bytes 0-4       total length of file
            * accessor2ID:   bytes 8-12      ID number of message
            * accessor3Data: variable bytes  payload
            * */

            int msgLength;
            int msgID;

            try
            {
                //System.Diagnostics.Debug.WriteLine("MmfBufferedReader [File " + entryID + "]: Filename " + _ipcType + "_" + entryID);

                //open mmf
                MemoryMappedFile mmf = MemoryMappedFile.OpenExisting(_ipcType + "_" + entryID);
                
                MemoryMappedViewAccessor accessor1Len = mmf.CreateViewAccessor(0, 4);
                MemoryMappedViewAccessor accessor2ID = mmf.CreateViewAccessor(8, 4);
                MemoryMappedViewAccessor accessor3Data;

                //locked section
                using (new Niawa.Utilities.SingleGlobalInstance(1000, _ipcType)) //1000ms timeout on global lock
                {

                    //get length, ID, and data
                    msgLength = accessor1Len.ReadInt32(0);
                    msgID = accessor2ID.ReadInt32(0);
                    accessor3Data = mmf.CreateViewAccessor(16, msgLength);
                }

        
                //System.Diagnostics.Debug.WriteLine("MmfBufferedReader [File " + entryID + "]: MsgID: " + msgID);
                //System.Diagnostics.Debug.WriteLine("MmfBufferedReader [File " + entryID + "]: LastMsgID: " + _lastMsgID);


                //check if ID changed
                if (msgID == _lastMsgID)
                {
                    //System.Diagnostics.Debug.WriteLine("MmfBufferedReader [File " + entryID + "]: There is no work to do");
                
                    //do nothing
                    System.Threading.Thread.Sleep(10);
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine("MmfBufferedReader [File " + entryID + "]: Reading message");

                    //set ID
                    _lastMsgID = msgID;

                    //check if message is too long
                    if (msgLength + 16 > _bufferLength)
                    {
                        logger.Error("[" + _description + "-T] Not reading message [" + msgID + "]: Message length [" + (msgLength + 16).ToString() + "] is greater than the buffer length [" + _bufferLength.ToString() + "]");
                        _threadStatus.MessageErrorCount += 1;
                    }
                    else
                    {
                        //get data
                        byte[] buffer = new byte[msgLength];
                        accessor3Data.ReadArray<byte>(0, buffer, 0, msgLength);

                        NiawaMmfContainer msg = new NiawaMmfContainer(buffer);

                        logger.Debug("[" + _description + "-T] Received data [" + msgID + "]: " + msg.SerialID);

                        _threadStatus.MessageCount += 1;

                        _receiveQueue.Enqueue(msg);
                    }

                }
            }
            catch (System.IO.FileNotFoundException)
            {

                System.Diagnostics.Debug.WriteLine("MmfBufferedReader [" + _ipcType + "][File " + entryID + "]: File was not found");
            }

        }

        /// <summary>
        /// ThreadStatus property
        /// </summary>
        public Niawa.Threading.ThreadStatus ThreadStatus
        {
            get { return _threadStatus; }
        }

        /// <summary>
        /// IpcType property
        /// </summary>
        public string IpcType
        {
            get 
            {
                return _ipcType; 
            }
        }

        /// <summary>
        /// Disposes object
        /// </summary>
        public void Dispose()
        {
            _abortPendingWork = true;
            if (_threadStatus != null) _threadStatus.IsThreadActive = false;
            if (_threadStatus != null) _threadStatus.IsThreadEnabled = false;

            //thread status
            if (_threadStatus != null) _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_FINALIZED;

        }
    }
}
