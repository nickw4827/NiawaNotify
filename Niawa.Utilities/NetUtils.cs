using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Niawa.Utilities
{
    public class NetUtils
    {


        public static bool CheckIfPortFree(int port)
        {
            bool isAvailable = true;

            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            System.Net.NetworkInformation.IPGlobalProperties ipGlobalProperties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();
            System.Net.NetworkInformation.TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (System.Net.NetworkInformation.TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == port)
                {
                    isAvailable = false;
                    break;
                }
            }

            // At this point, if isAvailable is true, we can proceed accordingly.
            return isAvailable;

        }

        /// <summary>
        /// Find the IP address of the primary LAN device.
        /// </summary>
        /// <returns></returns>
        public static IPAddress FindLanAddress()
        {
            IPHostEntry host;
            IPAddress localIP = null;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIP = ip; //ip.ToString();
                }
            }
            return localIP;
        }

        /// <summary>
        /// Find the IP address of the primary LAN device.
        /// </summary>
        /// <returns></returns>
        public static IPAddress FindLanAddress2()
        {
            IPAddress gateway = FindGetGatewayAddress();
            if (gateway == null)
                return null;

            IPAddress[] pIPAddress = Dns.GetHostAddresses(Dns.GetHostName());

            foreach (IPAddress address in pIPAddress)
            {
                if (IsAddressOfGateway(address, gateway))
                    return address;
            }

            return null;
        }

        static bool IsAddressOfGateway(IPAddress address, IPAddress gateway)
        {
            if (address != null && gateway != null)
                return IsAddressOfGateway(address.GetAddressBytes(), gateway.GetAddressBytes());
            return false;
        }

        static bool IsAddressOfGateway(byte[] address, byte[] gateway)
        {
            if (address != null && gateway != null)
            {
                int gwLen = gateway.Length;
                if (gwLen > 0)
                {
                    if (address.Length == gateway.Length)
                    {
                        --gwLen;
                        int counter = 0;
                        for (int i = 0; i < gwLen; i++)
                        {
                            if (address[i] == gateway[i])
                                ++counter;
                        }
                        return (counter == gwLen);
                    }
                }
            }
            return false;

        }

        static IPAddress FindGetGatewayAddress()
        {
            System.Net.NetworkInformation.IPGlobalProperties ipGlobProps = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();

            foreach (System.Net.NetworkInformation.NetworkInterface ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                System.Net.NetworkInformation.IPInterfaceProperties ipInfProps = ni.GetIPProperties();
                foreach (System.Net.NetworkInformation.GatewayIPAddressInformation gi in ipInfProps.GatewayAddresses)
                    return gi.Address;
            }
            return null;
        }


    }
}
