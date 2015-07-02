using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.NetControllerTestClient
{
    class TestClient
    {

        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        
        public TestClient()
        {
        }


        public void ExecuteTcpUnitTest0()
        {
            logger.Info(">>");
            logger.Info("Program started");

            //my IP address
            System.Net.IPAddress myAddress = Niawa.Utilities.NetUtils.FindLanAddress();
            int myPort = 2150;

            //remote client IP address
            System.Net.IPAddress remoteAddress = System.Net.IPAddress.Parse("192.168.2.6");
            int remotePort = 2150;

            //create utility bus
            Niawa.Utilities.UtilsServiceBus utilsBus = new Niawa.Utilities.UtilsServiceBus();

            //set up ipc logging
            Niawa.MsEventIpcEventAdapter.MsEventIpcEventWriter evtWriter = new Niawa.MsEventIpcEventAdapter.MsEventIpcEventWriter(utilsBus);
            evtWriter.Start();
            evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetControllerTestClient", true, "TcpReceiver", utilsBus), "TcpReceiver");
            //evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetControllerTestClient", true, "TcpReceiverMsg", utilsBus), "TcpReceiverMsg");
            evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetControllerTestClient", true, "TcpTransmitter", utilsBus), "TcpTransmitter");
            //evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetControllerTestClient", true, "TcpTransmitterMsg", utilsBus), "TcpTransmitterMsg");

            //receiver
            logger.Info("Test receiver");
            Niawa.NetController.TcpReceiver receiver = new Niawa.NetController.TcpReceiver(myAddress.ToString(), myPort, "(local)", evtWriter.EvtConsumer, utilsBus, "TestApp.TestClient", null);

            receiver.StartListening("TestClient");
            System.Threading.Thread.Sleep(1500);

            //transmitter
            logger.Info("Test transmitter");
            Niawa.NetController.TcpTransmitter transmitter = new Niawa.NetController.TcpTransmitter(remoteAddress.ToString(), remotePort, evtWriter.EvtConsumer, utilsBus, "TestApp.TestClient", null);

            transmitter.StartTransmitting("TestClient");
            System.Threading.Thread.Sleep(1500);

            //send test message
            transmitter.SendMessage(new NetController.NiawaNetMessage(myAddress.ToString(), myPort, "TestHost", remoteAddress.ToString(), remotePort, "TestHost", Guid.NewGuid(), "testApp", "testMsgType1", "testMsg contents 1"));
            System.Threading.Thread.Sleep(500);

            //send test message
            transmitter.SendMessage(new NetController.NiawaNetMessage(myAddress.ToString(), myPort, "TestHost", remoteAddress.ToString(), remotePort, "TestHost", Guid.NewGuid(), "testApp", "testMsgType2", "testMsg contents 2"));
            System.Threading.Thread.Sleep(1500);

            //send test message
            transmitter.SendMessage(new NetController.NiawaNetMessage(myAddress.ToString(), myPort, "TestHost", remoteAddress.ToString(), remotePort, "TestHost", Guid.NewGuid(), "testApp", "testMsgType3", "testMsg contents 3"));
            System.Threading.Thread.Sleep(500);

            //send test message
            transmitter.SendMessage(new NetController.NiawaNetMessage(myAddress.ToString(), myPort, "TestHost", remoteAddress.ToString(), remotePort, "TestHost", Guid.NewGuid(), "testApp", "testMsgType4", "testMsg contents 4"));
            System.Threading.Thread.Sleep(500);

            //send test message
            transmitter.SendMessage(new NetController.NiawaNetMessage(myAddress.ToString(), myPort, "TestHost", remoteAddress.ToString(), remotePort, "TestHost", Guid.NewGuid(), "testApp", "testMsgType5", "testMsg contents 5"));
            System.Threading.Thread.Sleep(1500);

            //send test message
            transmitter.SendMessage(new NetController.NiawaNetMessage(myAddress.ToString(), myPort, "TestHost", remoteAddress.ToString(), remotePort, "TestHost", Guid.NewGuid(), "testApp", "testMsgType6", "testMsg contents 6"));
            System.Threading.Thread.Sleep(1500);

            //send test message
            transmitter.SendMessage(new NetController.NiawaNetMessage(myAddress.ToString(), myPort, "TestHost", remoteAddress.ToString(), remotePort, "TestHost", Guid.NewGuid(), "testApp", "testMsgType7", "testMsg contents 7"));
            System.Threading.Thread.Sleep(1500);

            //send test message
            transmitter.SendMessage(new NetController.NiawaNetMessage(myAddress.ToString(), myPort, "TestHost", remoteAddress.ToString(), remotePort, "TestHost", Guid.NewGuid(), "testApp", "testMsgType8", "testMsg contents 8"));
            System.Threading.Thread.Sleep(500);

            //send test message
            transmitter.SendMessage(new NetController.NiawaNetMessage(myAddress.ToString(), myPort, "TestHost", remoteAddress.ToString(), remotePort, "TestHost", Guid.NewGuid(), "testApp", "testMsgType9", "testMsg contents 9"));
            System.Threading.Thread.Sleep(1500);

            //send test message
            transmitter.SendMessage(new NetController.NiawaNetMessage(myAddress.ToString(), myPort, "TestHost", remoteAddress.ToString(), remotePort, "TestHost", Guid.NewGuid(), "testApp", "testMsgType10", "testMsg contents 10"));
            System.Threading.Thread.Sleep(500);



            System.Threading.Thread.Sleep(3000);

            transmitter.StopTransmitting("TestClient", false);
            System.Threading.Thread.Sleep(1000);

            receiver.StopListening("TestClient", false);
            System.Threading.Thread.Sleep(1000);

            transmitter = null;
            receiver = null;

            logger.Info("Program stopped <<");
        }

        public void ExecuteTcpUnitTest1()
        {
            logger.Info(">>");
            logger.Info("Program started");

            //my IP address
            System.Net.IPAddress myAddress = Niawa.Utilities.NetUtils.FindLanAddress();
            int myPort = 2150;

            //remote client IP address
            System.Net.IPAddress remoteAddress = null;
            if (myAddress.ToString() == "192.168.2.14")
                remoteAddress = System.Net.IPAddress.Parse("192.168.2.6");
            else if (myAddress.ToString() == "192.168.2.6")
                remoteAddress = System.Net.IPAddress.Parse("192.168.2.14");
            else
                throw new Exception("The current IP address [" + myAddress.ToString() + "] doesn't have a configured test remote value");

            int remotePort = 2150;

            //create utility bus
            Niawa.Utilities.UtilsServiceBus utilsBus = new Niawa.Utilities.UtilsServiceBus();

            //set up ipc logging
            Niawa.MsEventIpcEventAdapter.MsEventIpcEventWriter evtWriter = new Niawa.MsEventIpcEventAdapter.MsEventIpcEventWriter(utilsBus);
            evtWriter.Start();
            evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetControllerTestClient", true, "TcpReceiver", utilsBus), "TcpReceiver");
            //evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetControllerTestClient", true, "TcpReceiverMsg", utilsBus), "TcpReceiverMsg");
            evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetControllerTestClient", true, "TcpTransmitter", utilsBus), "TcpTransmitter");
            //evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetControllerTestClient", true, "TcpTransmitterMsg", utilsBus), "TcpTransmitterMsg");

            //receiver
            logger.Info("Test receiver");
            Niawa.NetController.TcpReceiver receiver = new Niawa.NetController.TcpReceiver(myAddress.ToString(), myPort, "(local)", evtWriter.EvtConsumer, utilsBus, "TestApp.TestClient", null);

            receiver.StartListening("TestClient");
            //System.Threading.Thread.Sleep(1500);

            //transmitter
            logger.Info("Test transmitter");
            Niawa.NetController.TcpTransmitter transmitter = new Niawa.NetController.TcpTransmitter(remoteAddress.ToString(), remotePort, evtWriter.EvtConsumer, utilsBus, "TestApp.TestClient", null);

            transmitter.StartTransmitting("TestClient");
            System.Threading.Thread.Sleep(1500);

            bool doneTesting = false;

            int ix = 0;
            while (!doneTesting)
            {
                ix++;
                //send test message
                transmitter.SendMessage(new NetController.NiawaNetMessage(myAddress.ToString(), myPort, "TestHost", remoteAddress.ToString(), remotePort, "TestHost", Guid.NewGuid(), "testApp", "testMsgType" + ix, "testMsg contents " + ix));
                System.Threading.Thread.Sleep(2500);

            }

            transmitter.StopTransmitting("TestClient", false);
            System.Threading.Thread.Sleep(1000);

            receiver.StopListening("TestClient", false);
            System.Threading.Thread.Sleep(1000);

            transmitter = null;
            receiver = null;

            logger.Info("Program stopped <<");
        }

        public void ExecuteUdpUnitTest1()
        {
            logger.Info(">>");
            logger.Info("Program started");

            //create utility bus
            Niawa.Utilities.UtilsServiceBus utilsBus = new Niawa.Utilities.UtilsServiceBus();

            //set up ipc logging
            Niawa.MsEventIpcEventAdapter.MsEventIpcEventWriter evtWriter = new Niawa.MsEventIpcEventAdapter.MsEventIpcEventWriter(utilsBus);
            evtWriter.Start();
            evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetControllerTestClient", true, "UdpReceiver", utilsBus), "UdpReceiver");
            //evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetControllerTestClient", true, "TcpReceiverMsg", utilsBus), "TcpReceiverMsg");
            evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetControllerTestClient", true, "UdpTransmitter", utilsBus), "UdpTransmitter");
            //evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetControllerTestClient", true, "TcpTransmitterMsg", utilsBus), "TcpTransmitterMsg");
            
            Niawa.NetController.UdpTransmitter transmitter = new Niawa.NetController.UdpTransmitter(5001, evtWriter.EvtConsumer, utilsBus, "TestApp.TestClient",  null);
            transmitter.StartTransmitting("TestClient");

            Niawa.NetController.UdpReceiver receiver = new Niawa.NetController.UdpReceiver(5001, evtWriter.EvtConsumer, utilsBus, "TestApp.TestClient", null, true, transmitter);
            receiver.StartListening("TestClient");

            bool active = true;
            int ix = 0;
            while (active)
            {
                string lanAddress = Niawa.Utilities.NetUtils.FindLanAddress().ToString();
                transmitter.SendMessage(new NetController.NiawaNetDatagram(Niawa.Utilities.NetUtils.FindLanAddress().ToString(), 5001, Guid.NewGuid(), "testHost", "testApp", "testMsgType", "testMsg " + lanAddress + " contents " + ix));

                System.Threading.Thread.Sleep(1500);

                ix++;
            }
            /*
            receiver.SuspendListening("TestClient");
            transmitter.SendMessage(new NetController.NiawaNetDatagram(Niawa.Utilities.NetUtils.FindLanAddress().ToString(), 5001, Guid.NewGuid(), "testHost", "testApp", "testMsgType", "testMsg contents 1"));
            receiver.ResumeListening("TestClient");
            transmitter.SendMessage(new NetController.NiawaNetDatagram(Niawa.Utilities.NetUtils.FindLanAddress().ToString(), 5001, Guid.NewGuid(), "testHost", "testApp", "testMsgType", "testMsg contents 1"));
            receiver.ResumeListening("TestClient");
            transmitter.SendMessage(new NetController.NiawaNetDatagram(Niawa.Utilities.NetUtils.FindLanAddress().ToString(), 5001, Guid.NewGuid(), "testHost", "testApp", "testMsgType", "testMsg contents 1"));
            receiver.ResumeListening("TestClient");
            transmitter.SendMessage(new NetController.NiawaNetDatagram(Niawa.Utilities.NetUtils.FindLanAddress().ToString(), 5001, Guid.NewGuid(), "testHost", "testApp", "testMsgType", "testMsg contents 1"));
            receiver.ResumeListening("TestClient");
            transmitter.SendMessage(new NetController.NiawaNetDatagram(Niawa.Utilities.NetUtils.FindLanAddress().ToString(), 5001, Guid.NewGuid(), "testHost", "testApp", "testMsgType", "testMsg contents 1"));
            receiver.ResumeListening("TestClient");
            transmitter.SendMessage(new NetController.NiawaNetDatagram(Niawa.Utilities.NetUtils.FindLanAddress().ToString(), 5001, Guid.NewGuid(), "testHost", "testApp", "testMsgType", "testMsg contents 1"));
            receiver.ResumeListening("TestClient");
            transmitter.SendMessage(new NetController.NiawaNetDatagram(Niawa.Utilities.NetUtils.FindLanAddress().ToString(), 5001, Guid.NewGuid(), "testHost", "testApp", "testMsgType", "testMsg contents 1"));
            receiver.ResumeListening("TestClient");
            transmitter.SendMessage(new NetController.NiawaNetDatagram(Niawa.Utilities.NetUtils.FindLanAddress().ToString(), 5001, Guid.NewGuid(), "testHost", "testApp", "testMsgType", "testMsg contents 1"));
            receiver.ResumeListening("TestClient");

            transmitter.SendMessage(new NetController.NiawaNetDatagram(Niawa.Utilities.NetUtils.FindLanAddress().ToString(), 5001, Guid.NewGuid(), "testHost", "testApp", "testMsgType", "testMsg contents 1"));
            receiver.ResumeListening("TestClient");
            transmitter.SendMessage(new NetController.NiawaNetDatagram(Niawa.Utilities.NetUtils.FindLanAddress().ToString(), 5001, Guid.NewGuid(), "testHost", "testApp", "testMsgType", "testMsg contents 1"));
            receiver.ResumeListening("TestClient");
            transmitter.SendMessage(new NetController.NiawaNetDatagram(Niawa.Utilities.NetUtils.FindLanAddress().ToString(), 5001, Guid.NewGuid(), "testHost", "testApp", "testMsgType", "testMsg contents 1"));
            receiver.ResumeListening("TestClient");
            transmitter.SendMessage(new NetController.NiawaNetDatagram(Niawa.Utilities.NetUtils.FindLanAddress().ToString(), 5001, Guid.NewGuid(), "testHost", "testApp", "testMsgType", "testMsg contents 1"));
            receiver.ResumeListening("TestClient");
            transmitter.SendMessage(new NetController.NiawaNetDatagram(Niawa.Utilities.NetUtils.FindLanAddress().ToString(), 5001, Guid.NewGuid(), "testHost", "testApp", "testMsgType", "testMsg contents 1"));
            receiver.ResumeListening("TestClient");
            transmitter.SendMessage(new NetController.NiawaNetDatagram(Niawa.Utilities.NetUtils.FindLanAddress().ToString(), 5001, Guid.NewGuid(), "testHost", "testApp", "testMsgType", "testMsg contents 1"));
            receiver.ResumeListening("TestClient");
            transmitter.SendMessage(new NetController.NiawaNetDatagram(Niawa.Utilities.NetUtils.FindLanAddress().ToString(), 5001, Guid.NewGuid(), "testHost", "testApp", "testMsgType", "testMsg contents 1"));
            receiver.ResumeListening("TestClient");
            transmitter.SendMessage(new NetController.NiawaNetDatagram(Niawa.Utilities.NetUtils.FindLanAddress().ToString(), 5001, Guid.NewGuid(), "testHost", "testApp", "testMsgType", "testMsg contents 1"));
            receiver.ResumeListening("TestClient");
            */
            System.Threading.Thread.Sleep(1500);

            logger.Info("Program stopped <<");
        
        }

        public void ExecuteUdpUnitTest0()
        {
            logger.Info(">>");
            logger.Info("Program started");

            //create utility bus
            Niawa.Utilities.UtilsServiceBus utilsBus = new Niawa.Utilities.UtilsServiceBus();

            //set up ipc logging
            Niawa.MsEventIpcEventAdapter.MsEventIpcEventWriter evtWriter = new Niawa.MsEventIpcEventAdapter.MsEventIpcEventWriter(utilsBus);
            evtWriter.Start();
            evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetControllerTestClient", true, "UdpReceiver", utilsBus), "UdpReceiver");
            //evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetControllerTestClient", true, "TcpReceiverMsg", utilsBus), "TcpReceiverMsg");
            evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetControllerTestClient", true, "UdpTransmitter", utilsBus), "UdpTransmitter");
            //evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetControllerTestClient", true, "TcpTransmitterMsg", utilsBus), "TcpTransmitterMsg");

            logger.Info("Test transmitter");
            Niawa.NetController.UdpTransmitter transmitter = new Niawa.NetController.UdpTransmitter(5001, evtWriter.EvtConsumer, utilsBus, "TestApp.TestClient", null);

            transmitter.StartTransmitting("TestClient");
            System.Threading.Thread.Sleep(1500);

            //send test message
            transmitter.SendMessage(new NetController.NiawaNetDatagram(Niawa.Utilities.NetUtils.FindLanAddress().ToString(), 5001, Guid.NewGuid(), "testHost", "testApp", "testMsgType", "testMsg contents 1"));
            System.Threading.Thread.Sleep(500);

            transmitter.SuspendTransmitting("TestClient");
            System.Threading.Thread.Sleep(1500);

            transmitter.ResumeTransmitting("TestClient");
            System.Threading.Thread.Sleep(1500);


            logger.Info("Test receiver");
            Niawa.NetController.UdpReceiver receiver = new Niawa.NetController.UdpReceiver(5001, evtWriter.EvtConsumer, utilsBus, "TestApp.TestClient", null, true, transmitter);

            receiver.StartListening("TestClient");
            System.Threading.Thread.Sleep(1500);

            receiver.SuspendListening("TestClient");
            System.Threading.Thread.Sleep(1500);

            receiver.ResumeListening("TestClient");
            System.Threading.Thread.Sleep(1500);
            
            //send test message
            transmitter.SendMessage(new NetController.NiawaNetDatagram(Niawa.Utilities.NetUtils.FindLanAddress().ToString(), 5001, Guid.NewGuid(), "testHost", "testApp", "testMsgType", "testMsg contents 1"));
            System.Threading.Thread.Sleep(500);


            System.Threading.Thread.Sleep(3000);

            transmitter.StopTransmitting("TestClient", false);
            System.Threading.Thread.Sleep(1000);

            receiver.StopListening("TestClient", false);
            System.Threading.Thread.Sleep(1000);
            

            logger.Info("Program stopped <<");
        }

    }
}
