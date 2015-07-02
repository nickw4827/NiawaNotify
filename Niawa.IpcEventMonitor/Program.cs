using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.IpcEventMonitor
{
    class Program
    {

        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
       
        static void Main(string[] args)
        {
            logger.Info(">>");
            logger.Info("Program started");

            if( args.GetLength(0) != 1)
            {
                logger.Info("Cannot start IpcEventMonitor:  Exactly 1 argument must be supplied on startup.");
                return;
            }

            IpcEventMonitor eventMonitor = new IpcEventMonitor(args[0]);
            eventMonitor.Execute();

        }
    }
}
