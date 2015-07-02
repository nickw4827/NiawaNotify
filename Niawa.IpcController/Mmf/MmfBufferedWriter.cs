using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.MemoryMappedFiles;
using System.Security.AccessControl;    //MutexAccessRule
using System.Security.Principal;        //SecurityIdentifier

namespace Niawa.IpcController
{
    /// <summary>
    /// Writes messages to a memory-mapped file.
    /// Messages are queued and written to the file with an incrementing message id.
    /// </summary>
    public class MmfBufferedWriter : IDisposable 
    {
        /* #threadPattern# */
       
        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        Niawa.Utilities.SerialId id;

        /* Parameters */
        private string _ipcType;
        private int _bufferLength;
        private bool _ignoreExceptions;

        /* Resources */
        private Queue<NiawaMmfContainer> _sendQueue;
        private NiawaMmfBufferHeader _header;
        private int _currentEntryID = 0;

        /* Globals */
        private int _msgID;
        private string _description = string.Empty;

        /* Threading */
        private System.Threading.Thread t1;
        private Niawa.Threading.ThreadStatus _threadStatus = null;
        private bool _abortPendingWork;
        
        /// <summary>
        /// Instantiates MmfBufferedWriter.
        /// </summary>
        /// <param name="ipcType">Identifies memory mapped file and used to authenticate outgoing messages.</param>
        /// <param name="bufferLength">Length of data segment</param>
        /// <param name="ignoreExceptions">True if exceptions should be handled; false if they should be returned to the caller.</param>
        public MmfBufferedWriter(string ipcType, int bufferLength, Niawa.Utilities.UtilsServiceBus utilsBus, bool ignoreExceptions)
        {
            
            try
            {
                _sendQueue = new Queue<NiawaMmfContainer>();
                _header = new NiawaMmfBufferHeader();

                _ipcType = ipcType;
                _description = "MmfWriter " + _ipcType;
                _bufferLength = bufferLength;
                _ignoreExceptions = ignoreExceptions;

                //initialize serial ID generator
                id = utilsBus.InitializeSerialId(Niawa.Utilities.IdGeneratorUtils.ID_ROOT_NIAWA_MMF_CONTAINER);

                //get random starting message ID
                Random rnd = new Random();
                int startingMsgID = rnd.Next(0, 100000);
            
                _msgID = startingMsgID;
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
        /// Enables writing to the MmfWriter.
        /// </summary>
        public void StartWriting()
        {
            try
            {
                logger.Info("[" + _description + "-M] Starting");

                if (_threadStatus.IsThreadEnabled)
                {
                    logger.Warn("[" + _description + "-M] Could not start: writer thread was already active");
                }
                else
                {
                    //start thread
                    t1 = new System.Threading.Thread(WriteThreadImpl);

                    _sendQueue.Clear();
                    _currentEntryID = 0;
                    InitializeHeader();

                    //start client
                    _threadStatus.IsThreadEnabled = true;
                    t1.Start();

                    //thread status
                    _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STARTED;

                }

                
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Exception during start writing: " + ex.Message, ex);
                if (!_ignoreExceptions)
                    throw ex;
            }
        }

        /// <summary>
        /// Disables writing to the MmfWriter.
        /// </summary>
        public void StopWriting(bool abortPendingWork)
        {
            try
            {
                logger.Info("[" + _description + "-M] Stopping");

                //stop client
                _abortPendingWork = abortPendingWork;
                _threadStatus.IsThreadEnabled = false;

                //thread status
                _threadStatus.Status = Niawa.Threading.ThreadStatus.STATUS_STOPPED;

                //wait for any pending work
                if (!abortPendingWork)
                {
                    int autoAbortCounter = 0;
                    while (_sendQueue.Count > 0)
                    {
                        autoAbortCounter++;
                        //wait for work to finish
                        System.Threading.Thread.Sleep(100);
                        if (autoAbortCounter > 100)
                        {
                            logger.Warn("[" + _description + "-M] Could not finish pending work; aborting with [" + _sendQueue.Count + "] item(s) in queue");
                            _abortPendingWork = true;
                            break;
                        }
                    }
                }

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
                logger.Error("[" + _description + "-M] Exception during stop writing: " + ex.Message, ex);
                if (!_ignoreExceptions)
                    throw ex;
            }
        }

        /// <summary>
        /// Write a message to the MmfWriter.  Message is queued and sent in a thread loop.
        /// </summary>
        /// <param name="message">Message to write</param>
        public void WriteData(NiawaMmfContainer message)
        {
            try
            {
                if (!_threadStatus.IsThreadEnabled || !_threadStatus.IsThreadActive) { throw new Exception("Cannot write data - writer is not active"); };

                //get next serial ID
                id = Niawa.Utilities.IdGeneratorUtils.IncrementSerialId(id);
                message.SerialID = id.ToString();

                logger.Debug("[" + _description + "-M] Writing data: " + message.SerialID);

                _sendQueue.Enqueue(message);
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Exception while writing data: " + ex.Message, ex);
                if (!_ignoreExceptions)
                    throw ex;
            }
        }

        /// <summary>
        /// Initialize the buffer header file with empty values
        /// </summary>
        private void InitializeHeader()
        {
            try
            {
                //create a new buffer header
                _header = new NiawaMmfBufferHeader();
                SortedList<int, KeyValuePair<string, DateTime>> entries = new SortedList<int, KeyValuePair<string, DateTime>>();
                //populate the buffer header with blank values
                int ix = 1;
                while (ix <= 100)
                {
                    entries.Add(ix, new KeyValuePair<string, DateTime>(string.Empty, new DateTime(1900, 1, 1)));
                    ix++;
                }
                _header.Entries = entries;
                _header.LatestUpdateDate = new DateTime(1900, 1, 1);
                _header.LatestEntryID = 0;

                //convert buffer header to bytes
                byte[] buffer = _header.ToByteArray();

                //locked section
                using (new Niawa.Utilities.SingleGlobalInstance(1000, _ipcType + "_header")) //1000ms timeout on global lock
                {
                    int msgLength = buffer.Length;
                    int msgID = 0;
                    
                    //open mmf
                    MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen(_ipcType + "_header", _bufferLength);
                    MemoryMappedViewAccessor accessor1Len = mmf.CreateViewAccessor(0, 4);
                    MemoryMappedViewAccessor accessor2ID = mmf.CreateViewAccessor(8, 4);
                    MemoryMappedViewAccessor accessor3Data = mmf.CreateViewAccessor(16, msgLength);

                    //write data
                    accessor1Len.Write<Int32>(0, ref msgLength);
                    accessor2ID.Write<Int32>(0, ref msgID);
                    accessor3Data.WriteArray<byte>(0, buffer, 0, msgLength);

                } //end locked section

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Exception while initializing buffer header: " + ex.Message, ex);
                if (!_ignoreExceptions)
                    throw ex;
            }

        }

        /// <summary>
        /// Update the buffer header file to indicate a file has been written
        /// </summary>
        private void UpdateHeader(int entryID, DateTime entryDate, string entrySerialID, MemoryMappedFile mmfHeader)
        {
            try
            {
                //update buffer header
                KeyValuePair<string, DateTime> kvp = new KeyValuePair<string, DateTime>(entrySerialID, entryDate);

                if (_header.Entries.ContainsKey(entryID))
                {   //update existing entry
                    _header.Entries[entryID] = kvp;
                }
                else
                {   //add new entry
                    _header.Entries.Add(entryID, kvp);
                }
                _header.LatestEntryID = entryID;
                _header.LatestUpdateDate = entryDate;
                

                //convert buffer header to bytes
                byte[] buffer = _header.ToByteArray();

                int msgLength = buffer.Length;
                int msgID = entryID;

                ////locked section
                //using (new Niawa.Utilities.SingleGlobalInstance(1000, _ipcType + "_header")) //1000ms timeout on global lock
                //{

                //System.Diagnostics.Debug.WriteLine("MmfBufferedWriter [Header]: Filename " + _ipcType + "_header");

                //open mmf
                //MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen(_ipcType + "_header", _bufferLength);
                MemoryMappedViewAccessor accessor1Len = mmfHeader.CreateViewAccessor(0, 4);
                MemoryMappedViewAccessor accessor2ID = mmfHeader.CreateViewAccessor(8, 4);
                MemoryMappedViewAccessor accessor3Data = mmfHeader.CreateViewAccessor(16, msgLength);

                //write data
                accessor1Len.Write<Int32>(0, ref msgLength);
                accessor2ID.Write<Int32>(0, ref msgID);
                accessor3Data.WriteArray<byte>(0, buffer, 0, msgLength);
                //}

            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-M] Exception while updating header: " + ex.Message, ex);
                if (!_ignoreExceptions)
                    throw ex;
            }
        }

        /// <summary>
        /// Loop thread that reads from the write queue and writes data to the memory mapped file.
        /// </summary>
        /// <param name="data"></param>
        private void WriteThreadImpl(object data)
        {
            try
            {
                _threadStatus.IsThreadActive = true;
                logger.Info("[" + _description + "-T] Writer active");

                MemoryMappedFile mmfHeader;
                SortedList<int, MemoryMappedFile> mmfList = new SortedList<int, MemoryMappedFile>();

                try
                {
                    while (_threadStatus.IsThreadEnabled == true || (!_abortPendingWork && _sendQueue.Count > 0))
                    {
                        _threadStatus.ThreadHealthDate = DateTime.Now;

                        if (!_threadStatus.IsThreadEnabled && !_abortPendingWork && _sendQueue.Count > 0)
                            logger.Info("[" + _description + "-T] Finishing pending work [" + _sendQueue.Count + "]");
                        
                        //listening
                        try
                        {
                            if (_sendQueue.Count == 0)
                            {
                                //System.Diagnostics.Debug.WriteLine("MmfBufferedWriter [Thread]: There is no work to do");
                                
                                //no data to send
                                System.Threading.Thread.Sleep(10);
                            }
                            else
                            {
                                //send message
                                //System.Diagnostics.Debug.WriteLine("MmfBufferedWriter [Thread]: Writing message");

                                //dequeue
                                NiawaMmfContainer message = _sendQueue.Dequeue();

                                logger.Debug("[" + _description + "-T] Writing message " + message.SerialID);
                        
                                //increment ID
                                if (_msgID == Int32.MaxValue) _msgID = 0;
                                _msgID++;

                                //get bytes
                                byte[] buffer = message.ToByteArray();

                                /* Memory Mapped File:
                                 * accessor1Len:  bytes 0-4       total length of file
                                 * accessor2ID:   bytes 8-12      ID number of message
                                 * accessor3Data: variable bytes  payload
                                 * */

                                //locked section
                                using (new Niawa.Utilities.SingleGlobalInstance(1000, _ipcType)) //1000ms timeout on global lock
                                {
                                    int msgLength = buffer.Length;

                                    //check if message is too long
                                    if (msgLength + 16 > _bufferLength)
                                    {
                                        logger.Error("[" + _description + "-T] Not writing message [" + message.SerialID + "]: Message length [" + (msgLength + 16).ToString() + "] is greater than the buffer length [" + _bufferLength.ToString() + "]");
                                        _threadStatus.MessageErrorCount += 1;
                                    }
                                    else
                                    {
                                        //determine entry ID for buffer header
                                        if (_currentEntryID >= 100)
                                            _currentEntryID = 0;
                                        _currentEntryID++;

                                        //System.Diagnostics.Debug.WriteLine("MmfBufferedWriter [Thread]: CurrentEntryID: " + _currentEntryID);

                                        //update buffer header
                                        //persist in memory
                                        mmfHeader = MemoryMappedFile.CreateOrOpen(_ipcType + "_header", _bufferLength); 
                                        UpdateHeader(_currentEntryID, DateTime.Now, message.SerialID.ToString(), mmfHeader);

                                        //System.Diagnostics.Debug.WriteLine("MmfBufferedWriter [File " + _currentEntryID + "]: Filename " + _ipcType + "_" + _currentEntryID);

                                        //open mmf (determined by current buffer header entryID)
                                        MemoryMappedFile mmf;
                                        //persist in memory
                                        //check if already scoped
                                        if (mmfList.ContainsKey(_currentEntryID))
                                            mmf = mmfList[_currentEntryID];
                                        
                                        mmf = MemoryMappedFile.CreateOrOpen(_ipcType + "_" + _currentEntryID.ToString(), _bufferLength);
                                        
                                        //add to scope
                                        if(!mmfList.ContainsKey(_currentEntryID))
                                            mmfList.Add(_currentEntryID, mmf);

                                        MemoryMappedViewAccessor accessor1Len = mmf.CreateViewAccessor(0, 4);
                                        MemoryMappedViewAccessor accessor2ID = mmf.CreateViewAccessor(8, 4);
                                        MemoryMappedViewAccessor accessor3Data = mmf.CreateViewAccessor(16, msgLength);

                                        //write data
                                        accessor1Len.Write<Int32>(0, ref msgLength);
                                        accessor2ID.Write<Int32>(0, ref _msgID);
                                        accessor3Data.WriteArray<byte>(0, buffer, 0, msgLength);

                                        _threadStatus.MessageCount += 1;

                                    }
                                    
                                } //end locked section

                            }

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
                            logger.Error("[" + _description + "-T] Exception while writing message: " + ex.Message, ex);
                            _threadStatus.MessageErrorCount += 1;
                            _threadStatus.ErrorCount += 1;

                            System.Threading.Thread.Sleep(100);
                        }
                        System.Threading.Thread.Sleep(25);
                    }
                    //done writing

                }
                finally
                {

                    _threadStatus.IsThreadActive = false;
                    _threadStatus.IsThreadEnabled = false;
                    logger.Info("[" + _description + "-T] Writer inactive");
                }
            }
            catch (Exception ex)
            {
                logger.Error("[" + _description + "-T] Exception in write thread: " + ex.Message, ex);
                _threadStatus.ErrorCount += 1;
                if (!_ignoreExceptions)
                    throw ex;
            }

        }

        public int CountMessagesInSendQueue()
        {
            return _sendQueue.Count();
        }

        public Niawa.Threading.ThreadStatus ThreadStatus
        {
            get { return _threadStatus; }
        }

        public string IpcType
        {
            get 
            {
                return _ipcType;
            }
        }

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
