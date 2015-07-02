using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcController
{
    /// <summary>
    /// Reads IpcEvents by encapsulating MmfReader to read from a memory mapped file.
    /// </summary>
    public class IpcEventReader: iEventReader
    {
        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /* Parameters */
        string _ipcType;
        int _ipcEventMessageLength;
        bool _ignoreIpcExceptions;

        /* Resources */
        Niawa.IpcController.MmfBufferedReader mmfReader;

        /* Status */
        bool started;

        /// <summary>
        /// Instantiates IpcEventReader by initializing MmfReader.
        /// </summary>
        /// <param name="ipcType">Identifies and authenticates data for memory mapped file</param>
        /// <param name="ipcEventMessageLength">Length of data segment</param>
        /// <param name="ignoreIpcExceptions">True if read exceptions in MmfReader should be handled by MmfReader; false if they should be returned.</param>
        public IpcEventReader(string ipcType, int ipcEventMessageLength, Niawa.Utilities.UtilsServiceBus utilsBus, bool ignoreIpcExceptions)
        {
            
            try
            {
                _ipcType = ipcType;
                _ipcEventMessageLength = ipcEventMessageLength;
                _ignoreIpcExceptions = ignoreIpcExceptions;

                mmfReader = new Niawa.IpcController.MmfBufferedReader(_ipcType, _ipcEventMessageLength, utilsBus, _ignoreIpcExceptions);
            }
            catch (Exception ex)
            {
                logger.Error("[IpcEventReader " + _ipcType + "] Caught exception during IpcEventReader instantiate: " + ex.Message, ex);

                if (ignoreIpcExceptions)
                { }
                else
                    throw ex;
            }
            
        }

        /// <summary>
        /// Reads the next event from the MmfReader message queue.
        /// </summary>
        /// <returns></returns>
        public IpcEvent ReadNextEvent()
        {
            try
            {
                if (!started)
                    throw new Exception("[IpcEventReader " + _ipcType + "] Cannot read next IPC Event: IpcEventReader is not started.");

                while (1 == 1)
                {
                    if (mmfReader.CountMessagesInBuffer() > 0)
                    {
                        Niawa.IpcController.NiawaMmfContainer msg = mmfReader.GetNextMessageFromBuffer();

                        if (msg.IpcType == _ipcType)
                        {
                            Niawa.IpcController.IpcEvent evt = new Niawa.IpcController.IpcEvent(msg.IpcData);

                            logger.Debug("[IpcEventReader " + _ipcType + "] Reading IPC Event [" + evt.SerialID + "] from message [" + msg.SerialID + "]");
                            
                            /*
                            //inject SerialID into IpcEvent
                            evt.SerialID = msg.SerialID;
                            */

                            return evt;
                        }
                        else
                        {
                            //not the ipc type we're looking for, keep looking
                            logger.Warn("[IpcEventReader " + _ipcType + "] Not processing message [" + msg.SerialID + "]: NiawaMmfContainer IpcType [" + msg.IpcType + "] does not match IpcEventReader IpcType [" + _ipcType + "]");
                            System.Threading.Thread.Sleep(500);
                        }
                    }
                    else
                    {
                        //no messages in buffer
                        System.Threading.Thread.Sleep(50);
                    }

                }

            }
            catch (Exception ex)
            {
                logger.Error("[IpcEventReader " + _ipcType + "] Caught exception during IpcEventReader ReadNextEvent: " + ex.Message, ex);

                if (_ignoreIpcExceptions)
                { return null; }
                else
                    throw ex;
            }
            
        }

        /// <summary>
        /// Start listening for Ipc events.
        /// </summary>
        public void Start()
        {
            try
            {
                mmfReader.StartListening();
                started = true;
            }
            catch (Exception ex)
            {
                logger.Error("[IpcEventReader " + _ipcType + "] Caught exception during IpcEventReader Start: " + ex.Message, ex);

                if (_ignoreIpcExceptions)
                { }
                else
                    throw ex;
            }
            
        }

        /// <summary>
        /// Stop listening for Ipc events.
        /// </summary>
        public void Stop()
        {
            try
            {
                mmfReader.StopListening(false);
                started = false;
            }
            catch (Exception ex)
            {
                logger.Error("[IpcEventReader " + _ipcType + "] Caught exception during IpcEventReader Stop: " + ex.Message, ex);

                if (_ignoreIpcExceptions)
                { }
                else
                    throw ex;
            }
            
        }

        public int CountMessagesInQueue()
        {
            return mmfReader.CountMessagesInBuffer();
        }

        public bool IsActive
        {
            get { return started; }
        }

        public string IpcType
        {
            get { return _ipcType; }
        }

        public bool IsStarted
        {
            get { return started; }
        }

        public void Dispose()
        {
            if (mmfReader != null) mmfReader.Dispose();
        }

    }
}
