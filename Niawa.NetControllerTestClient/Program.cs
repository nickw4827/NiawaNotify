using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.NetControllerTestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            /***************/
            /* UDP testing */
            /***************/
            
            //TestClient tc1 = new TestClient();
            //tc1.ExecuteUdpUnitTest0();

            //TestClient tc1 = new TestClient();
            //tc1.ExecuteUdpUnitTest1();

            /***************/
            /*UDP randomized test suite*/
            /***************/
            
            //UdpTestClient utc1 = new UdpTestClient(5001);
            //utc1.Execute();
           

            /***************/
            /* TCP testing */
            /***************/
            
            //TestClient ttc1 = new TestClient();
            //tc1.ExecuteTcpUnitTest0();

            //TestClient tc1 = new TestClient();
            //tc1.ExecuteTcpUnitTest1();

            /***************/
            /*TCP randomized test suite*/
            /***************/
            /*
            TcpTestClient ttc1 = new TcpTestClient();
            ttc1.Initialize();
            ttc1.Execute();
            */

            /***************/
            /* Niawa Ad-Hoc Network Adapter test */
            /***************/
            
            System.Net.IPAddress myAddress = Niawa.Utilities.NetUtils.FindLanAddress();

            string ipAddress = "0.0.0.0";

            if(myAddress != null)
                ipAddress = myAddress.ToString();

            string hostname = System.Environment.MachineName;
            string applicationName = "TestApp";
            //string webApiUrl = "http://localhost:3465";
            //string webApiUrl = "http://localhost:2775";
            //string webApiUrl = "http://niawanotifytest03.azurewebsites.net";
            string webApiUrl = string.Empty;

            if (args.Length > 0)
                webApiUrl = args[0];

            NnaTestClient nnatc1 = new NnaTestClient(5001, ipAddress, 15000, 16000, hostname, applicationName, webApiUrl);

            nnatc1.ExecuteBasicTest();
            

        }
    }
}
