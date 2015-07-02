using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcController
{
    /// <summary>
    /// Creates IpcEventReaders and IpcEventWriters
    /// </summary>
    public class IpcFactory
    {
        /* Logging */
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Creates an IpcEventReader
        /// </summary>
        /// <param name="ignoreIpcExceptions">True if read exceptions should be handled by the reader; false if read exceptions should be returned to the caller.</param>
        /// <param name="ipcEventGroup">Used to create the IpcType, which is used to identify and authenticate IPC data.</param>
        /// <returns></returns>
        public static iEventReader CreateIpcEventReader(bool ignoreIpcExceptions, string ipcEventGroup, Niawa.Utilities.UtilsServiceBus utilsBus)
        {
            try
            {
                return new IpcEventReader("Niawa.IpcEvent." + ipcEventGroup, IpcEvent.IPC_EVENT_MMF_LENGTH, utilsBus, ignoreIpcExceptions);
            }
            catch (Exception ex)
            {
                logger.Error("Caught exception during CreateIpcEventReader: " + ex.Message, ex);

                if (ignoreIpcExceptions)
                    return new NullIpcEventReader();
                else
                    throw ex;
            }

            
        }

        /// <summary>
        /// Creates an IpcEventWriter
        /// </summary>
        /// <param name="applicationName">Name of application writing Ipc events</param>
        /// <param name="applicationInstance">Instance of the application write Ipc events</param>
        /// <param name="ignoreIpcExceptions">True if write exceptions should be handled by the writer; false if writer exceptions should be returned to the caller.</param>
        /// <param name="ipcEventGroup">Used to create the IpcType, which is used to identify and authenticate IPC data.</param>
        /// <returns></returns>
        public static iEventWriter CreateIpcEventWriter(string applicationName, bool ignoreIpcExceptions, string ipcEventGroup, Niawa.Utilities.UtilsServiceBus utilsBus)
        {
            try
            {
                return new IpcEventWriter(applicationName, "Niawa.IpcEvent." + ipcEventGroup, IpcEvent.IPC_EVENT_MMF_LENGTH, utilsBus, ignoreIpcExceptions);
            }
            catch (Exception ex)
            {
                logger.Error("Caught exception during CreateIpcEventReader: " + ex.Message, ex);
                
                if (ignoreIpcExceptions)
                    return new NullIpcEventWriter();
                else
                    throw ex;
            }

            
        }


    }
}
