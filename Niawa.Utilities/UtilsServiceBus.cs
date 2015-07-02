using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.Utilities
{
    public class UtilsServiceBus
    {
        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        
        /* Resources */
        private IdGeneratorUtils idGenerator = null;
        private SortedList<string, List<String>> valueRegistry = null;

        /* Locking */
        System.Threading.Semaphore lockSection;
        bool _locked = false;
        string _lockOwner = string.Empty;

        bool _internalLocked = false;
        string _internalLockOwner = string.Empty;

        System.Threading.Semaphore internalLockSection;
    
        /// <summary>
        /// 
        /// </summary>
        public UtilsServiceBus()
        {
            lockSection = new System.Threading.Semaphore(1, 1);
            internalLockSection = new System.Threading.Semaphore(1, 1);

            idGenerator = new IdGeneratorUtils();
            valueRegistry = new SortedList<string, List<string>>();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public SerialId InitializeSerialId(int root)
        {
            try
            {
                InternalAcquireLock("InitializeSerialId");

                return idGenerator.InitializeSerialId(root);
            }
            finally
            {
                if(IsInternalLocked) InternalReleaseLock();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetValueToRegistry(string category, string value)
        {
            try
            {
                InternalAcquireLock("SetValueToRegistry");

                if (valueRegistry.ContainsKey(category))
                {
                    //get the existing category list
                    List<string> valueList = valueRegistry[category];

                    if (valueList.Contains(value))
                    {
                        //nothing to do
                        return false;
                    }
                    else
                    {
                        logger.Info("[UtilsBus] Setting value to registry [" + category + "]:" + value);

                        //add the value to the existing list
                        valueList.Add(value);
                        return true;
                    }
                }
                else
                {
                    //create a new category list and add the value
                    List<string> valueList = new List<string>();
                    valueList.Add(value);

                    //add to registry
                    valueRegistry.Add(category, valueList);
                    return true;
                }

            }
            finally
            {
                if (IsInternalLocked) InternalReleaseLock();
            }

            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool RegistryContainsValue(string category, string value)
        {
            try
            {
                InternalAcquireLock("RegistryContainsValue");

                if (valueRegistry.ContainsKey(category))
                {
                    //get the existing category list
                    List<string> valueList = valueRegistry[category];

                    if (valueList.Contains(value))
                    {
                        //found value
                        return true;
                    }
                    else
                    {
                        //didn't find value
                        return false;
                    }

                }
                else
                {
                    //category doesn't exist
                    return false;
                }

            }
            finally
            {
                if (IsInternalLocked) InternalReleaseLock();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool RemoveValueFromRegistry(string category, string value)
        {
            try
            {
                InternalAcquireLock("RemoveValueFromRegistry");

                if (valueRegistry.ContainsKey(category))
                {
                    //get the existing category list
                    List<string> valueList = valueRegistry[category];

                    if (valueList.Contains(value))
                    {
                        logger.Info("[UtilsBus] Removing value from registry [" + category + "]:" + value);

                        //remove from value list
                        valueList.Remove(value);
                        return true;
                    }
                    else
                    {
                        //didn't find value
                        return false;
                    }
                }
                else
                {
                    //category doesn't exist
                    return false;
                }
            }
            finally
            {
                if (IsInternalLocked) InternalReleaseLock();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public bool RemoveAllValuesFromRegistry(string category)
        {
            try
            {
                InternalAcquireLock("RemoveAllValuesFromRegistry");

                if (valueRegistry.ContainsKey(category))
                {
                    logger.Info("[UtilsBus] Removing all values from registry [" + category + "]");

                    //get the existing category list
                    List<string> valueList = valueRegistry[category];

                    //clear the list
                    valueList.Clear();
                    return true;

                }
                else
                {
                    //category doesn't exist
                    return false;
                }
            }
            finally
            {
                if (IsInternalLocked) InternalReleaseLock();
            }
                        
        }


        /// <summary>
        /// Acquires a multithreading lock
        /// </summary>
        /// <returns></returns>
        public bool AcquireLock(string lockOwner)
        {
            return AcquireLock(lockOwner, 60000);
        }

        /// <summary>
        /// Acquires a multithreading lock
        /// </summary>
        /// <param name="timeoutMs"></param>
        /// <returns></returns>
        public bool AcquireLock(string lockOwner, int timeoutMs)
        {
            //attempt lock
            bool tryLock = lockSection.WaitOne(timeoutMs);
            if (!tryLock) throw new TimeoutException("Could not acquire lock [" + lockOwner + "]: lock is owned by [" + _lockOwner + "]");
            _locked = true;
            _lockOwner = lockOwner; 
            return true;

        }

        /// <summary>
        /// Releases a multithreading lock
        /// </summary>
        /// <returns></returns>
        public bool ReleaseLock()
        {
            //release lock
            _locked = false;
            _lockOwner = string.Empty;
            lockSection.Release();
            return true;
        }

        /// <summary>
        /// Returns a value indiating the locked status
        /// </summary>
        /// <returns></returns>
        public bool IsLocked
        {
            get { return _locked; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool InternalAcquireLock(string lockOwner)
        {
            //attempt lock
            bool tryLock = internalLockSection.WaitOne(60000);
            if (!tryLock) throw new TimeoutException("Could not acquire internal lock [" + lockOwner + "]: Lock is owned by [" + _internalLockOwner + "]");
            _internalLocked = true;
            _internalLockOwner = lockOwner;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool InternalReleaseLock()
        {
            //release lock
            _internalLocked = false;
            _internalLockOwner = string.Empty;
            internalLockSection.Release();
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool IsInternalLocked
        {
            get { return _internalLocked; }
        }

    }
}
