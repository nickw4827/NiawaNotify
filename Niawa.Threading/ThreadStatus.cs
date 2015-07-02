using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.Threading
{
    public class ThreadStatus
    {

        /* Logging */
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
       
        /* Constants */
        public readonly static string STATUS_UNKNOWN = "unknown";
        public readonly static string STATUS_INITIALIZED = "initialized";
        public readonly static string STATUS_STARTED = "started";
        public readonly static string STATUS_SUSPENDED = "suspended";
        public readonly static string STATUS_STOPPED = "stopped";
        public readonly static string STATUS_FINALIZED = "finalized";
        
        /* Parameters */
        ThreadStatusContainer statusContainer = null;
        Niawa.MsEventController.IEventRaiser _evtRaiser = null;
        int _threadHealthTimeoutSecs = 0;

        SortedList<string, string> _electiveProperties = new SortedList<string,string>();

        //public ThreadStatus(string description, int threadHealthTimeoutSecs, Niawa.Utilities.SerialId threadId)
        //{

        //    Instantiate(description, threadHealthTimeoutSecs, threadId, null, null);

        //}

        //public ThreadStatus(string description, int threadHealthTimeoutSecs, Niawa.Utilities.SerialId threadId, Niawa.Utilities.SerialId parentThreadId)
        //{
        //    Instantiate(description, threadHealthTimeoutSecs, threadId, parentThreadId, null);

        //}

        public ThreadStatus(string description, int threadHealthTimeoutSecs, string nodeID, string parentNodeID, Niawa.MsEventController.IEventRaiser evtRaiser)
        {
            Instantiate(description, threadHealthTimeoutSecs, nodeID, parentNodeID, evtRaiser);
        
        }

        private void Instantiate(string description, int threadHealthTimeoutSecs, string nodeID, string parentNodeID, Niawa.MsEventController.IEventRaiser evtRaiser)
        {
            //initialize serial ID generator
            //Niawa.Utilities.SerialId id = utilsBus.InitializeSerialId(Niawa.Utilities.IdGeneratorUtils.ID_ROOT_NIAWA_NET_MESSAGE);

            statusContainer = new ThreadStatusContainer();
            _evtRaiser = evtRaiser;
            _threadHealthTimeoutSecs = threadHealthTimeoutSecs;

            statusContainer.Description = description;
            statusContainer.NodeID = nodeID;
            statusContainer.ParentNodeID = parentNodeID;

            statusContainer.Initialized = true;
            statusContainer.InitializedDate = DateTime.Now;
            statusContainer.Status = STATUS_UNKNOWN;

        }

        public void RaiseStatusReport(string requestor)
        {
            try
            {

                logger.Info("[" + statusContainer.Description + "] Raising thread status report (requested by " + requestor + ")");

                if (_evtRaiser == null) throw new System.Exception("Event Raiser has not been set or is invalid (null).");
                SortedList<string, string> evtList = new SortedList<string, string>();

                //add thread status
                evtList.Add("ThreadStatus", this.ToJson());

                //add elective properties, if any
                foreach (KeyValuePair<string, string> item in _electiveProperties)
                {
                    evtList.Add("str:" + item.Key, item.Value);
                }

                evtList.Add("str:Requestor", requestor);

                //raise status
                _evtRaiser.RaiseEvent("StatusReport", statusContainer.Status, evtList, NodeID, ParentNodeID);

            }
            catch (Exception ex)
            {
                //logger.ErrorException("[" + _description + "-M] Error while raising status report: " + ex.Message, ex);
                throw new System.Exception("Error while raising status: " + ex.Message);
            }
        }

        /// <summary>
        /// Raises event with after changing status.  To be used after status changes only
        /// </summary>
        /// <returns></returns>
        private void RaiseStatus()
        {
            try
            {

                logger.Info("[" + statusContainer.Description + "] Raising thread status");

                if (_evtRaiser == null) throw new System.Exception("Event Raiser has not been set or is invalid (null).");
                SortedList<string, string> evtList = new SortedList<string, string>();

                //add thread status
                evtList.Add("ThreadStatus", this.ToJson());
                
                //add elective properties, if any
                foreach(KeyValuePair<string, string> item in _electiveProperties )
                {
                    evtList.Add("str:" + item.Key, item.Value);
                }
                
                //raise status
                _evtRaiser.RaiseEvent("Status", statusContainer.Status, evtList, NodeID, ParentNodeID);

            }
            catch (Exception ex)
            {
                //logger.ErrorException("[" + _description + "-M] Error while raising status report: " + ex.Message, ex);
                throw new System.Exception("Error while raising status: " + ex.Message);
            }


        }

        public void RaiseThreadHealth()
        {
            try
            {
                logger.Info("[" + statusContainer.Description + "] Raising thread health");

                if (_evtRaiser == null) throw new System.Exception("Event Raiser has not been set or is invalid (null).");
                SortedList<string, string> evtList = new SortedList<string, string>();
                evtList.Add("str:ThreadHealth", CalculateThreadHealth().ToString());
                evtList.Add("str:ThreadHealthFailed", CalculateThreadHealthFailed().ToString());
                _evtRaiser.RaiseEvent("ThreadHealth", "Thread Health", evtList, NodeID, ParentNodeID);
            }
            catch (Exception ex)
            {
                //logger.ErrorException("[" + _description + "-M] Error while raising status report: " + ex.Message, ex);
                throw new System.Exception("Error while raising thread health: " + ex.Message);
            }

        }

        /// <summary>
        /// Add an elective property that will be sent when the status is raised
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddElectiveProperty(string key, string value)
        {
            if (_electiveProperties.ContainsKey(key))
                _electiveProperties.Remove(key);

            _electiveProperties.Add(key, value);
        }

        /// <summary>
        /// Remove an elective property.  Elective properties are sent when the status is raised
        /// </summary>
        /// <param name="key"></param>
        public void RemoveElectiveProperty(string key)
        {
            if (_electiveProperties.ContainsKey(key))
                _electiveProperties.Remove(key);

        }

        /// <summary>
        /// Calculates the thread health by subtracting the thread health datetime from the current system datetime.
        /// Returns a thread health value.
        /// </summary>
        /// <param name="threadHealthDate"></param>
        /// <returns></returns>
        public double CalculateThreadHealth()
        {
            double threadHealth = 0;
            if (statusContainer.ThreadHealthDate > DateTime.MinValue)
            {
                TimeSpan timespan = (DateTime.Now - statusContainer.ThreadHealthDate);
                threadHealth = timespan.TotalSeconds + 0.01; //adding 0.01 so good thread health isn't confused with 0 (0 means invalid date)
            }

            return threadHealth;

        }

        public bool CalculateThreadHealthFailed()
        {
            //if timeout set to zero, thread health never fails
            if(_threadHealthTimeoutSecs==0) return false;

            bool threadHealthFailed = false;

            double health = CalculateThreadHealth();
            if (health - _threadHealthTimeoutSecs > 0)
            {
                //upper limit has been breached
                //health failed
                threadHealthFailed = true;
            }
            else
            {
                //upper limit has not been breached
                //health is not failed
                threadHealthFailed = false;

            }

            return threadHealthFailed;

        }

        /*
        /// <summary>
        /// Raises an event with thread health information passed in by the caller.  This function is utilized in the ThreadHealthMonitor object.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="threadHealthValue"></param>
        /// <param name="healthFailed"></param>
        public void RaiseThreadHealth(string description, double threadHealthValue, bool healthFailed)
        {
            try
            {
                _evtRaiser.RaiseEvent("ThreadHealth", "Description:" + description + ";TheadHealth:" + threadHealthValue.ToString("F") + ";HealthFailed:" + healthFailed.ToString(), string.Empty);
            }
            catch (Exception ex)
            {
                //logger.ErrorException("[" + _description + "-M] Error while raising thread health: " + ex.Message, ex);
                throw new System.Exception("Error while raising thread health: " + ex.Message);
            }


        }
        */
        
        /*
        /// <summary>
        /// Calculates the thread health by subtracting the thread health datetime from the current system datetime.  
        /// Returns in a key value pair that includes the supplied description and the thread health value.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="threadHealthDate"></param>
        /// <returns></returns>
        public KeyValuePair<string, double> CalculateThreadHealth(string description, DateTime threadHealthDate)
        {
            return new KeyValuePair<string, double>(description, CalculateThreadHealth(threadHealthDate));
        }

        /// <summary>
        /// Calculate sthe thread health by subtracting the thread health datetime from the current system datetime.
        /// Return in a keyvaluepair that includes the supplied description, the thread health value, and a boolean value indicating if the upper limit has been breached.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="upperLimit"></param>
        /// <param name="threadHealthDate"></param>
        /// <returns></returns>
        public KeyValuePair<string, KeyValuePair<double, bool>> CalculateThreadHealth(string description, double upperLimit, DateTime threadHealthDate)
        {
            double health = CalculateThreadHealth(threadHealthDate);
            if (health - upperLimit > 0)
            {
                //upper limit has been breached
                //health failed
                return new KeyValuePair<string, KeyValuePair<double, bool>>(description, new KeyValuePair<double, bool>(health, true));

            }
            else
            {
                //upper limit has not been breached
                //health passed
                return new KeyValuePair<string, KeyValuePair<double, bool>>(description, new KeyValuePair<double, bool>(health, false));

            }

        }
        */

        public string Description
        {
            get { return statusContainer.Description; }
            set { statusContainer.Description = value; }
        }

        public string Status
        {
            get { return statusContainer.Status; }
            set
            {
                if (value != statusContainer.Status)
                {
                    statusContainer.Status = value;
                    statusContainer.StatusDate = DateTime.Now;

                    if (_evtRaiser != null) RaiseStatus();
                }
            }
        }

        public DateTime StatusDate
        {
            get { return statusContainer.StatusDate; }
            //set { statusContainer.StatusDate = value; }
        }

        public bool IsInitialized
        {
            get { return statusContainer.Initialized; }
            set { 
                statusContainer.Initialized = value;
                if (value == true) statusContainer.InitializedDate = DateTime.Now;
            }
        }

        public DateTime InitializedDate
        {
            get { return statusContainer.InitializedDate; }
            //set { statusContainer.InitializedDate = value; }
        }

        public bool IsFinalized
        {
            get { return statusContainer.Finalized; }
            set { 
                statusContainer.Finalized = value;
                if (value == true) statusContainer.FinalizedDate = DateTime.Now;
            }
        }

        public DateTime FinalizedDate
        {
            get { return statusContainer.FinalizedDate; }
            //set { statusContainer.FinalizedDate = value; }
        }

        public bool IsThreadHealthFailed
        {
            get {
                bool threadHealthFailed = CalculateThreadHealthFailed();
                if (threadHealthFailed != statusContainer.ThreadHealthFailed) statusContainer.ThreadHealthFailed = threadHealthFailed;

                return statusContainer.ThreadHealthFailed; 
            }
            //set { statusContainer.ThreadHealthFailed = value; }
        }
        
        public DateTime ThreadHealthDate
        {
            get { return statusContainer.ThreadHealthDate; }
            set { statusContainer.ThreadHealthDate = value; }
        }

        public bool IsThreadActive
        {
            get { return statusContainer.ThreadActive; }
            set { statusContainer.ThreadActive = value; }
        }

        public bool IsThreadEnabled
        {
            get { return statusContainer.ThreadEnabled; }
            set { statusContainer.ThreadEnabled = value; }
        }

        public int ErrorCount
        {
            get { return statusContainer.ErrorCount; }
            set
            {
                statusContainer.ErrorCount = value;
                statusContainer.LastErrorDate = DateTime.Now;
            }
        }

        public int MessageCount
        {
            get { return statusContainer.MessageCount; }
            set
            {
                statusContainer.MessageCount = value;
                statusContainer.LastMessageDate = DateTime.Now;
            }
        }

        public int MessageErrorCount
        {
            get { return statusContainer.MessageErrorCount; }
            set { 
                statusContainer.MessageErrorCount = value;
                statusContainer.LastMessageErrorDate = DateTime.Now;
            }
        }

        public DateTime LastErrorDate
        {
            get { return statusContainer.LastErrorDate; }
            //set { statusContainer.LastErrorDate = value; }
        }

        public DateTime LastMessageDate
        {
            get { return statusContainer.LastMessageDate; }
            //set { statusContainer.LastMessageDate = value; }
        }

        public DateTime LastMessageErrorDate
        {
            get { return statusContainer.LastMessageErrorDate; }
            //set { statusContainer.LastMessageErrorDate = value; }
        }

        //public string ThreadIdStr
        //{
        //    get
        //    {
        //        if (statusContainer.ThreadId == null)
        //            return string.Empty;
        //        else
        //            return statusContainer.ThreadId.ToString();
        //    }
        //}

        //public string ParentThreadIdStr
        //{
        //    get
        //    {
        //        if (statusContainer.ParentThreadId == null)
        //            return string.Empty;
        //        else
        //            return statusContainer.ParentThreadId.ToString();
        //    }
        //}

        public string NodeID
        {
            get { return statusContainer.NodeID; }
        }

        public string ParentNodeID
        {
            get { return statusContainer.ParentNodeID; }
        }

        public bool IsEvtRaiserSet()
        {
            if (_evtRaiser == null)
                return false;
            else
                return true;
        }

        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(statusContainer);
        }
    }
}
