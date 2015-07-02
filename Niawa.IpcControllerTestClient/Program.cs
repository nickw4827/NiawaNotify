using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcControllerTestClient
{
    class Program
    {
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
       
        static void Main(string[] args)
        {
            
            /*
            //Niawa.IpcController.MmfWriter writer = new Niawa.IpcController.MmfWriter("Niawa.IpcEvent.TestEvent1", 131072, false);
            Niawa.IpcController.MmfWriter writer = new Niawa.IpcController.MmfWriter("Niawa.IpcEvent.TestEvent1", Niawa.IpcController.IpcEvent.IPC_EVENT_MMF_LENGTH, false);
            writer.StartWriting();
            
            while (1 == 1)
            {
                Niawa.IpcController.IpcEvent evt = new Niawa.IpcController.IpcEvent(Guid.NewGuid(), System.DateTime.Now, "app1", "testEvent1", "eventDetail");
                evt.AddProperty("propk1", "propv1");
                evt.AddProperty("propk2", "propv2");

                Niawa.IpcController.NiawaMmfContainer msg = new Niawa.IpcController.NiawaMmfContainer(System.DateTime.Now, "IpcEvent.Test1", evt.ToJson());

                logger.Info("Writing message [" + evt.EventID + "]");

                writer.WriteData(msg);

                System.Threading.Thread.Sleep(1500);

            }
            */

            //create utility bus
            Niawa.Utilities.UtilsServiceBus utilsBus = new Niawa.Utilities.UtilsServiceBus();

            Niawa.IpcController.iEventWriter evtWriter = Niawa.IpcController.IpcFactory.CreateIpcEventWriter("Niawa.IpcControllerTestClient", false, "TestGroup1", utilsBus);
            evtWriter.Start();

            int i = 0;

            while (1 == 1)
            {
                i++;

                string nodeID = "TestNodeID";
                string parentNodeID = "TestParentNodeID";

                Niawa.IpcController.IpcEvent evt = new Niawa.IpcController.IpcEvent(Guid.NewGuid(), System.DateTime.Now, "Niawa.IpcControllerTestClient", Environment.MachineName, "testEvent" + i, "eventMessage", "eventDetail", nodeID, parentNodeID);
                //evt.AddProperty("propk1", "propv1-" + i);
                //evt.AddProperty("propk2", "propv2-"+ i);

                logger.Info("Writing IPC test event" + i + "");

                evtWriter.Write(evt);

                System.Threading.Thread.Sleep(10);

                evtWriter.Write(Environment.MachineName, "testEvent2", "eventMessage2", "eventDetail2", nodeID, parentNodeID);

                System.Threading.Thread.Sleep(50);

            }



        }
    }
}
