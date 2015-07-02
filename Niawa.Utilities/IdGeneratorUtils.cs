using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.Utilities
{
    public class IdGeneratorUtils
    {
        /* Constants */
        public readonly static int ID_ROOT_NIAWA_MMF_CONTAINER = 101;
        public readonly static int ID_ROOT_NIAWA_EVENT_MESSAGE = 201;
        public readonly static int ID_ROOT_IPC_EVENT = 301;
        public readonly static int ID_ROOT_NIAWA_NET_DATAGRAM = 401;
        public readonly static int ID_ROOT_NIAWA_NET_MESSAGE = 501;
        public readonly static int ID_ROOT_NIAWA_THREAD_ID = 601;

        /* Resources */
        private Random rnd = null;

        public IdGeneratorUtils()
        {
            //initialize randomizer
            rnd = new Random();
        }

        public SerialId InitializeSerialId(int idRoot)
        {
           
            SerialId id = new SerialId();
            id.IdRoot = idRoot;

            
            //get random session ID
            int session = rnd.Next(1000, 9999);

            //get random starting serial ID
            int serial = rnd.Next(1, 100000);

            id.IdSession = session;
            id.IdSerial = serial;

            return id;


        }

        public static SerialId IncrementSerialId(SerialId id)
        {
            int serial = id.IdSerial;
            
            SerialId newId = new SerialId();

            //increment serial
            if (serial == Int32.MaxValue) serial = 0;
            serial++;

            newId.IdRoot = id.IdRoot;
            newId.IdSession = id.IdSession;
            newId.IdSerial = serial;

            return newId;
        }

    }
}
