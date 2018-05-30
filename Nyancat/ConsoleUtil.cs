using System;

namespace Nyancat
{
    class ConsoleUtil
    {   
        private static bool isShuttingDown = false;

        public static void AttachCtrlcSigtermShutdown(Action shutDown)
        {
            Action ShutDown = () =>
            {
                if (!isShuttingDown)
                    shutDown();

                isShuttingDown = true;
            };

            AppDomain.CurrentDomain.ProcessExit += delegate { ShutDown(); };
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                ShutDown();

                //Don't terminate the process immediately, wait for the Main thread to exit gracefully.
                eventArgs.Cancel = true;
            };
        }

    }
}
