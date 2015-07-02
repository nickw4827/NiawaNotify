using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.NetControllerTestClient
{
    public class TcpTestClient
    {
        private int _port;
        private string _myAddress;
        private string _remoteAddress;
        private Random rnd;

        private int totalMessageCounter;

        Niawa.NetController.TcpReceiver receiver;
        Niawa.NetController.TcpTransmitter transmitter;

        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        
        public TcpTestClient() //int port, string myAddress, string remoteAddress)
        {
            logger.Info(">>");
            logger.Info("Program started");
            
            rnd = new Random();
                
        
        }

        public void Initialize()
        {
            //my IP address
            System.Net.IPAddress myAddress = Niawa.Utilities.NetUtils.FindLanAddress();
            int port = 2150;

            //remote client IP address
            System.Net.IPAddress remoteAddress = null;
            if (myAddress.ToString() == "192.168.2.14")
                remoteAddress = System.Net.IPAddress.Parse("192.168.2.6");
            else if (myAddress.ToString() == "192.168.2.6")
                remoteAddress = System.Net.IPAddress.Parse("192.168.2.14");
            else
                throw new Exception("The current IP address [" + myAddress.ToString() + "] doesn't have a configured test remote value");

            _port = port;
            _myAddress = myAddress.ToString();
            _remoteAddress = remoteAddress.ToString();

        }

        private const int START_LISTENING = 1;
        private const  int STOP_LISTENING = 2;

        private const int START_TRANSMITTING = 3;
        private const int STOP_TRANSMITTING = 4;
        

        public void Execute()
        {
            logger.Info("Executing TcpTestClient");

            //create utility bus
            Niawa.Utilities.UtilsServiceBus utilsBus = new Niawa.Utilities.UtilsServiceBus();

            //set up ipc logging
            Niawa.MsEventIpcEventAdapter.MsEventIpcEventWriter evtWriter = new Niawa.MsEventIpcEventAdapter.MsEventIpcEventWriter(utilsBus);
            evtWriter.Start();
            evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetControllerTestClient", true, "TcpReceiver", utilsBus), "TcpReceiver");
            //evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetControllerTestClient", true, "TcpReceiverMsg", utilsBus), "TcpReceiverMsg");
            evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetControllerTestClient", true, "TcpTransmitter", utilsBus), "TcpTransmitter");
            //evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetControllerTestClient", true, "TcpTransmitterMsg", utilsBus), "TcpTransmitterMsg");
            
            receiver = new Niawa.NetController.TcpReceiver(_myAddress, _port,  "(local)", evtWriter.EvtConsumer, utilsBus, "TestApp.TcpTestClient", null);
            transmitter = new Niawa.NetController.TcpTransmitter(_remoteAddress, _port, evtWriter.EvtConsumer, utilsBus, "TestApp.TcpTestClient", null);

            receiver.StartListening("TcpTestClient");
            transmitter.StartTransmitting("TcpTestClient");

            System.Threading.Thread t1 = new System.Threading.Thread(StateChangeThreadImpl);
            System.Threading.Thread t2 = new System.Threading.Thread(SendMessageThreadImpl);

            t1.Start();
            t2.Start();

        }

        private void StateChangeThreadImpl(object data)
        {

            //pick next action and duration randomly
            while (1 == 1)
            {
                logger.Info("Picking next test state change");

                int nextTRAction = rnd.Next(1, 5);
            
                switch (nextTRAction)
                {
                    case (START_LISTENING):
                        logger.Info("Next test state change: Start Listening");
                        receiver.StartListening("TcpTestClient");
                        break;

                    case (STOP_LISTENING):
                        logger.Info("Next test state change: Stop Listening");
                        receiver.StopListening("TcpTestClient", false);
                        break;

                    case (START_TRANSMITTING):
                        logger.Info("Next test state change: Start Transmitting");
                        transmitter.StartTransmitting("TcpTestClient");
                        break;

                    case (STOP_TRANSMITTING):
                        logger.Info("Next test state change: Stop Transmitting");
                        transmitter.StopTransmitting("TcpTestClient", false);
                        break;


                }

                //sleep for random specified time
                int trDuration = rnd.Next(100, 10000);
                logger.Info("Next test state change thread sleeping for [" + trDuration + "] ms");
                System.Threading.Thread.Sleep(trDuration);

            }

        }

        private void SendMessageThreadImpl(object data)
        {

            //pick next action and duration randomly
            while (1 == 1)
            {
                logger.Info("Picking next send message test action");

                //send messages
                int nextMsgAction = rnd.Next(0, 3);
                int msgCount = rnd.Next(5, 50);

                if (nextMsgAction > 0)
                {
                    logger.Info("Sending [" + msgCount + "] test messages");

                    int i = 0;
                    while (i < msgCount)
                    {
                        totalMessageCounter++;
                        try
                        {
                            logger.Info("Sending " + (i + 1) + " of " + msgCount + " (" + totalMessageCounter + " total)");

                            transmitter.SendMessage(new NetController.NiawaNetMessage(_myAddress, _port, "TestHost", _remoteAddress, _port, "TestHost", Guid.NewGuid(), "TestApp", "TestMsgType" + totalMessageCounter.ToString(), "TestMsg contents " + totalMessageCounter.ToString()));
                        }
                        catch (Niawa.NetController.MessageNotSentException ex)
                        {
                            logger.Error("Error sending message: " + ex.Message, ex);
                        }

                        i++;

                        int msgSentSleepDuration = rnd.Next(10, 1000);
                        System.Threading.Thread.Sleep(msgSentSleepDuration);
                    }

                }
                else
                    logger.Info("Sending no test messages");

                //sleep for random specified time
                int trDuration = rnd.Next(100, 10000);
                logger.Info("Send message thread sleeping for [" + trDuration + "] ms");
                System.Threading.Thread.Sleep(trDuration);

            }

        }
    }
}
