using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcController
{
    public class NiawaMmfBufferHeader
    {
        /* Parameters */
        DateTime _latestUpdateDate = DateTime.MinValue;
        int _latestEntryID = 0;
        SortedList<int, KeyValuePair<string, DateTime>> _entries;

        /// <summary>
        /// Instantiates empty NiawaMmfBufferHeader. 
        /// </summary>
        public NiawaMmfBufferHeader()
        {
            _entries = new SortedList<int, KeyValuePair<string,DateTime>>();
        }

        /// <summary>
        /// Instantiates NiawaMmfBufferHeader with parameters.
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="latestUpdateDate"></param>
        /// <param name="latestEntryID"></param>
        public NiawaMmfBufferHeader(SortedList<int, KeyValuePair<string, DateTime>> entries, DateTime latestUpdateDate, int latestEntryID) 
        {
            _entries = entries;
            _latestUpdateDate = latestUpdateDate;
            _latestEntryID = latestEntryID;
        }

        /// <summary>
        /// Instantiates NiawaMmfBufferHeader with a byte array.
        /// </summary>
        /// <param name="bytes">Byte array payload</param>
        public NiawaMmfBufferHeader(Byte[] bytes)
        {
            //extract json from bytes
            String input = Niawa.Utilities.TransportUtils.GetString(bytes);

            //convert json to object
            NiawaMmfBufferHeader tempObject = Newtonsoft.Json.JsonConvert.DeserializeObject<NiawaMmfBufferHeader>(input);

            //fill parameters
            _entries = tempObject.Entries;
            _latestUpdateDate = tempObject.LatestUpdateDate;
            _latestEntryID = tempObject.LatestEntryID;

        }

        /// <summary>
        /// Converts object into byte array to send.  Serialized object to json then converts to byte array.
        /// </summary>
        /// <returns></returns>
        public Byte[] ToByteArray() 
        {
            //create json
            String output = Newtonsoft.Json.JsonConvert.SerializeObject(this);

            Byte[] outputBytes = Niawa.Utilities.TransportUtils.GetBytes(output);

            //convert json to byte array
            return outputBytes;

        }

        public SortedList<int, KeyValuePair<string, DateTime>> Entries
        {
            get { return _entries; }
            set { _entries = value; }
        }

        public DateTime LatestUpdateDate
        {
            get { return _latestUpdateDate; }
            set { _latestUpdateDate = value; }
        }

        public int LatestEntryID
        {
            get { return _latestEntryID; }
            set { _latestEntryID = value; }
        }

    }
}
