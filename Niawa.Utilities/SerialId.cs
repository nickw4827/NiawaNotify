using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.Utilities
{
    public class SerialId
    {
        /* Parameters */
        private int _idRoot;
        private int _idSession;
        private int _idSerial;

        public SerialId()
        {
        }

        public SerialId(int IdRoot, int IdSession, int IdSerial)
        {
            _idRoot = IdRoot;
            _idSession = IdSession;
            _idSerial = IdSerial;
        }

        public int IdRoot
        {
            get { return _idRoot; }
            set { _idRoot = value; }
        }

        public int IdSession
        {
            get { return _idSession; }
            set { _idSession = value; }
        }

        public int IdSerial
        {
            get { return _idSerial; }
            set { _idSerial = value; }
        }

        public override string ToString()
        {
            return _idRoot.ToString() + "-" + _idSession.ToString() + "-" + _idSerial.ToString();
        }


    }
}
