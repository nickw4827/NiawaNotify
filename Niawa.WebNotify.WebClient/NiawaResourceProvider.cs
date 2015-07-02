using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using Niawa.WebNotify.WebClient.Models;

namespace Niawa.WebNotify.WebClient
{
    public class NiawaResourceProvider
    {
        static NiawaSRHub _hub = null;
        //static NiawaIpcEventTreeModelAdapter _adapter = null;

        //static SortedList<string, NiawaIpcEventTreeModelAdapter> _adapterList = new SortedList<string, NiawaIpcEventTreeModelAdapter>();
        static NiawaIpcEventTreeModelAdapterPool _adapterPool = null;

        static MessageRepository _commandRepository = null;

        /// <summary>
        /// Registers NiawaSRHub with the provider
        /// </summary>
        /// <param name="hub"></param>
        public static void RegisterNiawaSRHub(NiawaSRHub hub)
        {
            _hub = hub;
        }

        /// <summary>
        /// Retrieves the registered NiawaSRHub at the provider
        /// </summary>
        /// <returns></returns>
        public static NiawaSRHub RetrieveNiawaSRHub()
        {
            return _hub;
        }
              
        /// <summary>
        /// Registers NiawaTreeModelAdapter with the provider
        /// </summary>
        /// <param name="adapter"></param>
        public static void RegisterNiawaTreeModelAdapter(NiawaIpcEventTreeModelAdapter adapter, string callerSessionID)
        {
            try
            {
                //_adapter = adapter;
                
                //initialize adapter pool if null
                if (_adapterPool == null) InitializeNiawaTreeModelAdapterPool();

                //remove from pool if already exists
                if (_adapterPool.ContainsAdapter(callerSessionID))
                    _adapterPool.RemoveAdapter(callerSessionID);

                //add to pool
                _adapterPool.AddAdapter(callerSessionID, adapter);

                //remove inactive sessions
                _adapterPool.RemoveInactiveAdapters();

                //remove oldest sessions when there are too many
                _adapterPool.RemoveOverflowAdapters();

            }
            catch (Exception ex)
            {
                Trace.TraceError("NiawaResourceProvider: Could not register Tree Model Adapter: " + ex.Message);
            }

        }

        /// <summary>
        /// Retrieves the specified registered NiawaTreeModelAdapter at the provider
        /// </summary>
        /// <returns></returns>
        public static NiawaIpcEventTreeModelAdapter RetrieveNiawaTreeModelAdapter(string callerSessionID)
        {
            try
            {
                //initialize adapter pool if null
                if (_adapterPool == null) InitializeNiawaTreeModelAdapterPool();
                
                return _adapterPool.GetAdapter(callerSessionID);
                //return _adapter;

            }
            catch (Exception ex)
            {
                Trace.TraceError("NiawaResourceProvider: Could not retrieve Tree Model Adapter: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Initializes adapter pool 
        /// </summary>
        private static void InitializeNiawaTreeModelAdapterPool()
        {
            _adapterPool = new NiawaIpcEventTreeModelAdapterPool();
        }

        /// <summary>
        /// Registers a NiawaIpcEventTreeModelAdapterPool with the provider
        /// </summary>
        /// <param name="adapterPool"></param>
        public static void RegisterNiawaTreeModelAdapterPool(NiawaIpcEventTreeModelAdapterPool adapterPool)
        {
            try
            {
                _adapterPool = adapterPool;
            }
            catch (Exception ex)
            {
                Trace.TraceError("NiawaResourceProvider: Could not retrieve Tree Model Adapter Pool: " + ex.Message);
            }
            
        }

        /// <summary>
        /// Returns the NiawaIpcEventTreeModelAdapterPool registered with the provider
        /// </summary>
        /// <returns></returns>
        public static NiawaIpcEventTreeModelAdapterPool RetrieveNiawaTreeModelAdapterPool()
        {
            //initialize adapter pool if null
            if (_adapterPool == null) InitializeNiawaTreeModelAdapterPool();
            
            return _adapterPool;
        }

    }
}