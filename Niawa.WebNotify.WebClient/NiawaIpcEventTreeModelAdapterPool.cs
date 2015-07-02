using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace Niawa.WebNotify.WebClient
{
    public class NiawaIpcEventTreeModelAdapterPool : IDisposable
    {
        //private const int CHECK_FOR_INACTIVE_SESSIONS_EVERY_MINS = 15;
        //private const int USER_INACTIVITY_TIMEOUT_MINS = 120;
        //private const int SESSION_INACTIVITY_TIMEOUT_MINS = 12;

        private const int CHECK_FOR_INACTIVE_SESSIONS_EVERY_MINS = 15;
        private const int USER_INACTIVITY_TIMEOUT_MINS = 120;
        private const int SESSION_INACTIVITY_TIMEOUT_MINS = 12;
        
        private const int SESSION_MAXIMUM_COUNT = 25;
        

        //resources
        private SortedList<string, NiawaIpcEventTreeModelAdapter> _adapters = null;
        private NiawaIpcEventTreeModelCacheAdapter _cacheAdapter = null;

        /* Locking */
        System.Threading.Semaphore lockSection = new System.Threading.Semaphore(1, 1);

        private DateTime _checkedForInactiveAdaptersDate = DateTime.MinValue;

        /// <summary>
        /// Instantiates a NiawaIpcEventTreeModelAdapterPool
        /// </summary>
        public NiawaIpcEventTreeModelAdapterPool()
        {
            try
            {
                Trace.TraceInformation("TreeModelAdapterPool starting up");
                _adapters = new SortedList<string, NiawaIpcEventTreeModelAdapter>();
                _cacheAdapter = new NiawaIpcEventTreeModelCacheAdapter();
                _cacheAdapter.Start();
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelAdapterPool: Could not instantiate: " + ex.Message);
            }
        }

        /// <summary>
        /// Adds an adapter to the pool
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddAdapter(string key, NiawaIpcEventTreeModelAdapter value)
        {
            try
            {
                bool tryLock = lockSection.WaitOne(6000);
                try
                {
                    _adapters.Add(key, value);
                }
                finally
                {
                    lockSection.Release();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelAdapterPool: Could not add adapter: " + ex.Message);
            }

        }

        /// <summary>
        /// Checks if the pool contains an adapter
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsAdapter(string key)
        {
            try
            {
                bool tryLock = lockSection.WaitOne(6000);
                try
                {
                    if(_adapters.ContainsKey(key))
                        return true;
                    else
                        return false;
                }
                finally
                {
                    lockSection.Release();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelAdapterPool: Could not remove adapter: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Gets an adapter from the pool
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public NiawaIpcEventTreeModelAdapter GetAdapter(string key)
        {
            try
            {
                if (_adapters.ContainsKey(key))
                    return _adapters[key];
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelAdapterPool: Could not get adapter: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Removes an adapter from the pool
        /// </summary>
        /// <param name="key"></param>
        public void RemoveAdapter(string key)
        {
            try
            {
                bool tryLock = lockSection.WaitOne(6000);
                try
                {
                    _adapters.Remove(key);
                }
                finally
                {
                    lockSection.Release();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelAdapterPool: Could not remove adapter: " + ex.Message);
            }
            
        }

        /// <summary>
        /// Shuts down and disposes the specified adapter
        /// </summary>
        /// <param name="key"></param>
        public void KillAdapter(string key, string stopReason)
        {
            try
            {
                NiawaIpcEventTreeModelAdapter adapter = null;

                if (_adapters.ContainsKey(key))
                    adapter = _adapters[key];

                if (adapter != null)
                {
                    adapter.Stop(stopReason);
                    adapter.Dispose();
                }
                else
                {
                    Trace.TraceWarning("TreeModelAdapterPool could not kill adapter: Adapter was not found for " + key);
                }

            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelAdapterPool: Could not kill adapter: " + ex.Message);
            }

        }
        
        /// <summary>
        /// Returns a list of cached node statuses
        /// </summary>
        /// <returns></returns>
        public List<Niawa.IpcController.IpcEvent> CachedNodeStatusList()
        {
            try
            {
                return _cacheAdapter.NodeStatusList();
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelAdapterPool: Could not return list of cached node statuses: " + ex.Message);
                return null;
            }
        }


        /// <summary>
        /// Adds a message to all adapters registered with the pool
        /// </summary>
        /// <param name="evt"></param>
        public void AddMessage(Niawa.IpcController.IpcEvent evt)
        {
            try
            {
                bool tryLock = lockSection.WaitOne(6000);
                try
                {
                    //add message to cache adapter
                    _cacheAdapter.AddMessage(evt);

                    //add message to session adapters
                    foreach (KeyValuePair<string, NiawaIpcEventTreeModelAdapter> kvp in _adapters)
                    {
                        kvp.Value.AddMessage(evt);
                    }

                }
                finally
                {
                    lockSection.Release();
                }

                //check for inactive sessions every xx mins, or next time a message arrives after that
                TimeSpan timespan = (DateTime.Now - _checkedForInactiveAdaptersDate);
                double timePassed = 0;
                timePassed = timespan.TotalMinutes;

                if (timePassed > CHECK_FOR_INACTIVE_SESSIONS_EVERY_MINS)
                {
                    _checkedForInactiveAdaptersDate = DateTime.Now;
                    RemoveInactiveAdapters();
                }

            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelAdapterPool: Could not add message: " + ex.Message);
            }

        }

        /// <summary>
        /// Removes inactive adapters from the pool.
        /// </summary>
        public void RemoveInactiveAdapters()
        {
            try
            {
                bool tryLock = lockSection.WaitOne(6000);
                try
                {
                    List<string> killList = new List<string>();

                    //look for inactive sessions
                    if (_adapters.Count > 0)
                        Trace.TraceInformation("TreeModelAdapterPool: Removing inactive sessions");

                    foreach (KeyValuePair<string, NiawaIpcEventTreeModelAdapter> kvp in _adapters)
                    {
                        //user inactivity
                        TimeSpan timespan = (DateTime.Now - kvp.Value.LastUserActivity);
                        double timePassed = 0;
                        timePassed = timespan.TotalMinutes;

                        if (timePassed > USER_INACTIVITY_TIMEOUT_MINS)
                        {
                            Trace.TraceInformation("TreeModelAdapterPool: Removing inactive TreeModelAdapter session (user inactive): " + kvp.Key);
                            KillAdapter(kvp.Key, "user inactive");
                            //_adapters.Remove(kvp.Key);
                            killList.Add(kvp.Key);
                        }
                        else
                        {
                            //session inactivity (not polling)
                            TimeSpan timespan2 = (DateTime.Now - kvp.Value.LastSessionPoll);
                            double timePassed2 = 0;
                            timePassed2 = timespan2.TotalMinutes;

                            if (timePassed2 > SESSION_INACTIVITY_TIMEOUT_MINS)
                            {
                                Trace.TraceInformation("TreeModelAdapterPool: Removing inactive TreeModelAdapter session (client session poll inactive): " + kvp.Key);
                                KillAdapter(kvp.Key, "session poll inactive");
                                //_adapters.Remove(kvp.Key);
                                killList.Add(kvp.Key);
                            }
                        }
                    } //~foreach

                    //remove items marked for deletion
                    foreach (string key in killList)
                    {
                        _adapters.Remove(key);
                    }
                    
                }
                finally
                {
                    lockSection.Release();
                }

            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelAdapterPool: Could not remove inactive adapters: " + ex.Message);
            }

           
        }

        /// <summary>
        /// Removes the oldest adapters from the pool when there are more than allowed
        /// </summary>
        public void RemoveOverflowAdapters()
        {
            try
            {
                //remove sessions when more than xx
                if (_adapters.Count > SESSION_MAXIMUM_COUNT)
                {
                    Trace.TraceWarning("TreeModelAdapterPool: There are more than the maximum allowed TreeModelAdapter sessions, removing the oldest session");

                    bool tryLock = lockSection.WaitOne(6000);
                    try
                    {
                        string earliestCallerSessionID = string.Empty;
                        DateTime earliestDate = DateTime.Now;

                        //find the earliest session
                        foreach (KeyValuePair<string, NiawaIpcEventTreeModelAdapter> kvp in _adapters)
                        {
                            if (kvp.Value.LastUserActivity < earliestDate)
                            {
                                earliestDate = kvp.Value.LastUserActivity;
                                earliestCallerSessionID = kvp.Key;
                            }

                        }

                        //remove the earliest session
                        if (earliestCallerSessionID.Trim().Length > 0)
                        {
                            Trace.TraceWarning("TreeModelAdapterPool: Removing oldest session: " + earliestCallerSessionID);
                            KillAdapter(earliestCallerSessionID, "server full");
                            _adapters.Remove(earliestCallerSessionID);
                        }
                        else
                        {
                            Trace.TraceError("TreeModelAdapterPool: Failed to remove oldest session: could not retrieve caller handle");
                        }

                    }
                    finally
                    {
                        lockSection.Release();
                    }

                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("TreeModelAdapterPool: Could not remove overflow adapters: " + ex.Message);
            }
           
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int AdapterCount()
        {
            return _adapters.Count;
        }


        public void Dispose()
        {
            _cacheAdapter.Stop();
            _cacheAdapter.Dispose();

            foreach (KeyValuePair<string, NiawaIpcEventTreeModelAdapter> kvp in _adapters)
            {
                kvp.Value.Stop("Server shutting down");
                kvp.Value.Dispose();
            }

        }

    }
}