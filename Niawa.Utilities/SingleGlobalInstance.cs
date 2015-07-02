using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.AccessControl;    //MutexAccessRule
using System.Security.Principal;        //SecurityIdentifier
using System.Threading;

namespace Niawa.Utilities
{
    /// <summary>
    /// Uses Mutex to lock a shared resource with other processes.
    /// </summary>
    public class SingleGlobalInstance : IDisposable
    {
        /*  Globals */
        bool hasHandle = false;

        /* Resources */
        Mutex mutex;

        private void InitMutex(string id)
        {
            //string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();
            //string mutexId = string.Format("Global\\{{{0}}}", appGuid);
            string mutexId = string.Format("Global\\Mutex_{{{0}}}", id);
            mutex = new Mutex(false, mutexId);

            var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);
            mutex.SetAccessControl(securitySettings);
        }

        /// <summary>
        /// Locks the shared resource.
        /// </summary>
        /// <param name="timeout">Timeout before releasing lock</param>
        /// <param name="id">ID of shared resource</param>
        public SingleGlobalInstance(int timeout, string id)
        {
            InitMutex(id);
            try
            {
                if (timeout < 0)
                    hasHandle = mutex.WaitOne(Timeout.Infinite, false);
                else
                    hasHandle = mutex.WaitOne(timeout, false);

                if (hasHandle == false)
                    throw new TimeoutException("Timeout waiting for exclusive access on SingleInstance");
            }
            catch (AbandonedMutexException)
            {
                hasHandle = true;
            }
        }


        public void Dispose()
        {
            if (mutex != null)
            {
                if (hasHandle)
                    mutex.ReleaseMutex();
                mutex.Dispose();
            }
        }
    }
}
