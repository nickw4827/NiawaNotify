using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.NetControllerTestClient
{
    public class UdpTestClient
    {
        private int _port;
        private Random rnd;

        private int totalMessageCounter;

        Niawa.NetController.UdpReceiver receiver;
        Niawa.NetController.UdpTransmitter transmitter;

        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        
        public UdpTestClient(int port)
        {
            _port = port;

            logger.Info(">>");
            logger.Info("Program started");
            
            rnd = new Random();
                
        
        }

        private const int START_LISTENING = 1;
        private const int SUSPEND_LISTENING = 2;
        private const int RESUME_LISTENING = 3;
        private const  int STOP_LISTENING = 4;

        private const int START_TRANSMITTING = 5;
        private const int SUSPEND_TRANSMITTING = 6;
        private const  int RESUME_TRANSMITTING = 7;
        private const int STOP_TRANSMITTING = 8;
        

        public void Execute()
        {
            logger.Info("Executing UdpTestClient");

            //create utility bus
            Niawa.Utilities.UtilsServiceBus utilsBus = new Niawa.Utilities.UtilsServiceBus();

            //set up ipc logging
            Niawa.MsEventIpcEventAdapter.MsEventIpcEventWriter evtWriter = new Niawa.MsEventIpcEventAdapter.MsEventIpcEventWriter(utilsBus);
            evtWriter.Start();
            evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetControllerTestClient", true, "UdpReceiver", utilsBus), "UdpReceiver");
            //evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetControllerTestClient", true, "UdpReceiverMsg", utilsBus), "UdpReceiverMsg");
            evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetControllerTestClient", true, "UdpTransmitter", utilsBus), "UdpTransmitter");
            //evtWriter.AddIpcEventWriter(Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.NetControllerTestClient", true, "UdpTransmitterMsg", utilsBus), "UdpTransmitterMsg");

            transmitter = new Niawa.NetController.UdpTransmitter(_port, evtWriter.EvtConsumer, utilsBus, "TestApp.UdpTestClient", null);
            receiver = new Niawa.NetController.UdpReceiver(_port, evtWriter.EvtConsumer, utilsBus, "TestApp.UdpTestClient", null, true, transmitter);
            
            receiver.StartListening("UdpTestClient");
            transmitter.StartTransmitting("UdpTestClient");

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

                int nextTRAction = rnd.Next(1, 9);
            
                switch (nextTRAction)
                {
                    case (START_LISTENING):
                        logger.Info("Next test state change: Start Listening");
                        receiver.StartListening("UdpTestClient");
                        break;

                    case (SUSPEND_LISTENING):
                        logger.Info("Next test state change: Suspend Listening");
                        receiver.SuspendListening("UdpTestClient");
                        break;

                    case (RESUME_LISTENING):
                        logger.Info("Next test state change: Resume Listening");
                        receiver.ResumeListening("UdpTestClient");
                        break;

                    case (STOP_LISTENING):
                        logger.Info("Next test state change: Stop Listening");
                        receiver.StopListening("UdpTestClient", false);
                        break;

                    case (START_TRANSMITTING):
                        logger.Info("Next test state change: Start Transmitting");
                        transmitter.StartTransmitting("UdpTestClient");
                        break;

                    case (SUSPEND_TRANSMITTING):
                        logger.Info("Next test state change: Suspend Transmitting");
                        transmitter.SuspendTransmitting("UdpTestClient");
                        break;

                    case (RESUME_TRANSMITTING):
                        logger.Info("Next test state change: Resume Transmitting");
                        transmitter.ResumeTransmitting("UdpTestClient");
                        break;

                    case (STOP_TRANSMITTING):
                        logger.Info("Next test state change: Stop Transmitting");
                        transmitter.StopTransmitting("UdpTestClient", false);
                        break;


                }

                //sleep for random specified time
                int trDuration = rnd.Next(100, 10000);
                logger.Info("State change thread sleeping for [" + trDuration + "] ms");
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

                            transmitter.SendMessage(new NetController.NiawaNetDatagram(Niawa.Utilities.NetUtils.FindLanAddress().ToString(), _port, Guid.NewGuid(), "TestHost", "TestApp", "TestMsgType" + totalMessageCounter.ToString(), "TestMsg contents " + totalMessageCounter.ToString()));
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
