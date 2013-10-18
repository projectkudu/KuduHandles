using System;
using System.Linq;

namespace KuduHandles
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length != 1)
            {
                PrintUsage();
                return;
            }

            try
            {
                foreach (var fileHandle in SystemUtility.GetHandles(Int32.Parse(args[0])).Where(handle => (handle.Type == HandleType.File)).ToList())
                {
                    if (fileHandle.DosFilePath != null)
                        Console.WriteLine(fileHandle.DosFilePath);
                }
            }
            catch
            {
                Console.WriteLine("[Error] Can't get open file handles for process {0}", args[0]);
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("[Usage] KuduHandles.exe <ProcessId>");
        }
    }
}
