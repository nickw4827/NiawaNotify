using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcController
{
    /// <summary>
    /// Structure that contains data to send via memory mapped file.
    /// </summary>
    public class NiawaMmfContainer
    {

        /* Parameters */
        DateTime _refreshedDate;
        string _ipcType;
        string _ipcData;

        /* Globals */
        string _serialID;

        /// <summary>
        /// Instantiates empty NiawaMmfContainer. 
        /// </summary>
        public NiawaMmfContainer()
        {
        }

        /// <summary>
        /// Instantiates NiawaMmfContainer with parameters.
        /// </summary>
        /// <param name="refreshedDate">Date that container was created.</param>
        /// <param name="ipcType">Identifies and authenticates data for memory mapped file.</param>
        /// <param name="ipcData">Ascii payload</param>
        public NiawaMmfContainer(DateTime refreshedDate, string ipcType, string ipcData) 
        {
            _refreshedDate = refreshedDate;
            _ipcType = ipcType;
            _ipcData = ipcData;
        }

        /// <summary>
        /// Instantiates NiawaMmfContainer with a byte array.
        /// </summary>
        /// <param name="bytes">Byte array payload</param>
        public NiawaMmfContainer(Byte[] bytes)
        {
            //extract json from bytes
            String input = Niawa.Utilities.TransportUtils.GetString(bytes);

            //convert json to object
            NiawaMmfContainer tempObject = Newtonsoft.Json.JsonConvert.DeserializeObject<NiawaMmfContainer>(input);

            //fill parameters
            _refreshedDate = tempObject.RefreshedDate;
            _ipcType = tempObject.IpcType;
            _ipcData = tempObject.IpcData;
            _serialID = tempObject.SerialID;

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

        public DateTime RefreshedDate
        {
            get { return _refreshedDate; }
            set { _refreshedDate = value; }
        }

        public string IpcType
        {
            get { return _ipcType; }
            set { _ipcType = value; }
        }

        public string IpcData
        {
            get { return _ipcData; }
            set { _ipcData = value; }
        }

        public string SerialID
        {
            get { return _serialID; }
            set { _serialID = value; }
        }


    }
}
