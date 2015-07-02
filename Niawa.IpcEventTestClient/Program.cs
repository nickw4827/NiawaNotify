using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcEventTestClient
{
    public class Program
    {
        static void Main(string[] args)
        {
            MockNiawaAdHocNetworkAdapter adapter = new MockNiawaAdHocNetworkAdapter();
            adapter.Start();

        }
    }
}
