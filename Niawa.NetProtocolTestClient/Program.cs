using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Niawa.NetProtocolTestClient
{
    class Program
    {

        static void Main(string[] args)
        {
            ConvertGUID();
            ConvertGUID();
            ConvertGUID();
            ConvertGUID();
            ConvertGUID();

        }

        static void ConvertGUID()
        {
            Guid myGuid = Guid.NewGuid();
            Console.WriteLine(myGuid.ToString());
            
            String myGuidToNumeric = ConvertGUIDtoInt(myGuid);
            Console.WriteLine(myGuidToNumeric);
            UInt64 myGuidToInt = Convert.ToUInt64(myGuidToNumeric.Substring(0, 18));
            Console.WriteLine(myGuidToInt.ToString());
            Console.WriteLine("-----");
        }

        static string ConvertGUIDtoInt(Guid myGuid)
        {
            var bytes = Guid.NewGuid().ToByteArray();
            Array.Resize(ref bytes, 17);
            var bigInt = new BigInteger(bytes);
            var value = bigInt.ToString();
            return value;

        }

    }
}
