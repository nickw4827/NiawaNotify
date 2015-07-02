using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcController
{
    /// <summary>
    /// Writes IpcEvents by encapsulating MmfWriter to write to a memory mapped file.
    /// </summary>
    public class IpcEventWriter: iEventWriter
    {
        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        
        /* Resources */
        Niawa.Utilities.SerialId id;
        Niawa.IpcController.MmfBufferedWriter mmfWriter;
        
        /* Parameters */
        string _applicationName;
        string _ipcType;
        int _ipcEventMessageLength;
        bool _ignoreIpcExceptions;

        /* Status */        
        bool started;

        /* Locking */
        System.Threading.Semaphore lockSection;
        
        /// <summary>
        /// Instantiates IpcEventWriter by initializing MmfWriter.
        /// </summary>
        /// <param name="applicationName">Application name to use in MmfWriter</param>
        /// <param name="ipcType">Identifies and authenticates data for memory mapped file</param>
        /// <param name="ipcEventMessageLength">Length of data segment</param>
        /// <param name="ignoreIpcExceptions">True if write exceptions in MmfWriter should be handled by MmfWriter; false if they should be returned.</param>
        public IpcEventWriter(string applicationName, string ipcType, int ipcEventMessageLength, Niawa.Utilities.UtilsServiceBus utilsBus, bool ignoreIpcExceptions)
        {
            try
            {
                _applicationName = applicationName;
                _ipcType = ipcType;
                _ipcEventMessageLength = ipcEventMessageLength;
                _ignoreIpcExceptions = ignoreIpcExceptions;

                //initialize serial ID generator
                id = utilsBus.InitializeSerialId(Niawa.Utilities.IdGeneratorUtils.ID_ROOT_IPC_EVENT);

                //initialize MMF writer
                mmfWriter = new Niawa.IpcController.MmfBufferedWriter(_ipcType, _ipcEventMessageLength, utilsBus, _ignoreIpcExceptions);

                lockSection = new System.Threading.Semaphore(1, 1);

            }
            catch (Exception ex)
            {
                logger.Error("[IpcEventWriter " + _ipcType + "] Caught exception during IpcEventWriter instantiate: " + ex.Message, ex);

                if (ignoreIpcExceptions)
                { }
                else
                    throw ex;
            }
            
        }

        /// <summary>
        /// Writes an event by enqueueing in MmfWriter.
        /// </summary>
        /// <param name="ipcEvent">IpcEvent message to write</param>
        public void Write(IpcEvent ipcEvent)
        {
            try
            {
                //attempt lock
                bool tryLock = lockSection.WaitOne(60000);
                if (!tryLock) throw new TimeoutException("[IpcEventWriter " + _ipcType + "] Could not obtain lock while writing event");

                try
                {
                    if (!started)
                        throw new Exception("[IpcEventWriter " + _ipcType + "] Cannot write event " + ipcEvent.SerialID + ": IpcEventWriter is not started.");

                    //get next serial ID
                    id = Niawa.Utilities.IdGeneratorUtils.IncrementSerialId(id);
                    ipcEvent.SerialID = id.ToString();

                    //create MMF message
                    Niawa.IpcController.NiawaMmfContainer msg = new Niawa.IpcController.NiawaMmfContainer(System.DateTime.Now, _ipcType, ipcEvent.ToJson()); //, ipcEvent.EventID.ToString());

                    logger.Debug("[IpcEventWriter " + _ipcType + "] Writing IPC Event " + ipcEvent.SerialID + "");

                    //write MMF message
                    mmfWriter.WriteData(msg);
                }
                finally
                {
                    //release lock
                    lockSection.Release();
                }

            }
            catch (Exception ex)
            {
                logger.Error("[IpcEventWriter " + _ipcType + "] Caught exception during IpcEventWriter Write [" + ipcEvent.SerialID + "]: " + ex.Message, ex);

                if (_ignoreIpcExceptions)
                { }
                else
                    throw ex;
            }

        }

        /// <summary>
        /// Creates and writes an event by enqueueing in MmfWriter.
        /// </summary>
        ///<param name="applicationInstance">Instance of the application to create in IpcEvent</param>
        /// <param name="eventType">Event type of IpcEvent to create</param>
        ///<param name="eventMessage">Event message to create in IpcEvent</param>
        /// <param name="eventMessageDetail">Event message detail to create in IpcEvent</param>
        /// <param name="nodeID">Event's Node ID</param>
        /// <param name="parentNodeID">Event's Parent Node ID</param>
        public void Write(string applicationInstance, string eventType, string eventMessage, string eventMessageDetail, string nodeID, string parentNodeID)
        {
            try
            {
                Niawa.IpcController.IpcEvent evt = new Niawa.IpcController.IpcEvent(Guid.NewGuid(), System.DateTime.Now, _applicationName, applicationInstance, eventType, eventMessage, eventMessageDetail, nodeID, parentNodeID);

                Write(evt);
            }
            catch (Exception ex)
            {
                logger.Error("[IpcEventWriter " + _ipcType + "] Caught exception during IpcEventWriter Write: " + ex.Message, ex);

                if (_ignoreIpcExceptions)
                { }
                else
                    throw ex;
            }
            
        }

        /// <summary>
        /// Enables writing of IpcEvents by enabling MmfWriter.
        /// </summary>
        public void Start()
        {
            try
            {
                //attempt lock
                bool tryLock = lockSection.WaitOne(60000);
                if (!tryLock) throw new TimeoutException("[IpcEventWriter " + _ipcType + "] Could not obtain lock while starting");

                try
                {
                    mmfWriter.StartWriting();
                    started = true;
                }
                finally
                {
                    //release lock
                    lockSection.Release();
                }

            }
            catch (Exception ex)
            {
                logger.Error("[IpcEventWriter " + _ipcType + "] Caught exception during IpcEventWriter Start: " + ex.Message, ex);

                if (_ignoreIpcExceptions)
                { }
                else
                    throw ex;
            }
            
            
        }

        /// <summary>
        /// Disabling writing of IpcEvents by disabling MmfWriter.
        /// </summary>
        public void Stop()
        {
            try
            {
                //attempt lock
                bool tryLock = lockSection.WaitOne(60000);
                if (!tryLock) throw new TimeoutException("[IpcEventWriter " + _ipcType + "] Could not obtain lock while stopping");

                try
                {
                    mmfWriter.StopWriting(false);

                    started = false;
                }
                finally
                {
                    //release lock
                    lockSection.Release();
                }

            }
            catch (Exception ex)
            {
                logger.Error("[IpcEventWriter " + _ipcType + "] Caught exception during IpcEventWriter Stop: " + ex.Message, ex);

                if (_ignoreIpcExceptions)
                { }
                else
                    throw ex;
            }

        }

        public int CountMessagesInQueue()
        {
            return mmfWriter.CountMessagesInSendQueue();
        }


        public void Dispose()
        {
            if (mmfWriter != null) mmfWriter.Dispose();
        }


        public bool IsActive
        {
            get { return started; }
        }

        public bool IsStarted
        {
            get { return started; }
        }

        public string ApplicationName
        {
            get { return _applicationName; }
        }

        public string IpcType
        {
            get { return _ipcType; }
        }

    }
}
