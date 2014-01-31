using System;
using System.Linq;

namespace KuduHandles
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length < 1)
            {
                PrintUsage();
                return;
            }
            var closeMode = args[0].Equals("-c", StringComparison.OrdinalIgnoreCase);
            ushort handleToClose = 0;
            uint processId = 0;
            if (closeMode)
            {
                handleToClose = ushort.Parse(args[2]);
                processId = uint.Parse(args[1]);
            }
            else
            {
                processId = uint.Parse(args[0]);
            }
            try
            {
                foreach (var fileHandle in SystemUtility.GetHandles((int)processId).Where(handle => (handle.Type == HandleType.File)).ToList())
                {
                    if (fileHandle.DosFilePath != null)
                    {
                        if (closeMode && fileHandle.RawHandleValue == handleToClose)
                        {
                            SystemUtility.CloseHandle(processId, fileHandle);
                        }
                        else if (!closeMode)
                        {
                            Console.Out.WriteLine("[{0}] {1}", fileHandle.RawHandleValue, fileHandle.DosFilePath);
                        }
                    }
                }
            }
            catch
            {
                Console.Error.WriteLine("[Error] Can't get open file handles for process {0}", args[0]);
            }
        }

        private static void PrintUsage()
        {
            Console.Error.WriteLine("[Usage] KuduHandles.exe <ProcessId>");
            Console.Error.WriteLine("[Usage] KuduHandles.exe -c <ProcessId> <HandleId>");
        }
    }
}
