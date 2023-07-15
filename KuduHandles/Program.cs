using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var verboseMode = args[0].Equals("-v", StringComparison.OrdinalIgnoreCase);
            var listMode = args[0].Equals("-l", StringComparison.OrdinalIgnoreCase);
            var typeMode = args[0].Equals("-s", StringComparison.OrdinalIgnoreCase);
            var defaultMode = false;
            ushort handleToClose = 0;
            uint processId = 0;
            if (closeMode)
            {
                handleToClose = ushort.Parse(args[2]);
                processId = uint.Parse(args[1]);
            }
            else if (verboseMode || listMode || typeMode)
            {
                processId = uint.Parse(args[1]);
            }
            else
            {
                processId = uint.Parse(args[0]);
                defaultMode = true;
            }
            try
            {

                if (typeMode)
                {
                    var handles = SystemUtility.GetHandles((int)processId);
                    var typeCounts = handles
                        .GroupBy(handle => string.IsNullOrWhiteSpace(handle.TypeString) ? handle.RawType.ToString() : handle.TypeString)
                        .Select(x => new KeyValuePair<string, int>(x.Key, x.Count()))
                        .OrderByDescending(x => x.Value);

                    var totalHandles = handles.Count();
                    Console.Out.WriteLine("{0} : {1}", "Total", totalHandles);
                    foreach (var typeCount in typeCounts)
                    {
                        string type = typeCount.Key;
                        int count = typeCount.Value;
                        Console.Out.WriteLine("{0} : {1}", type, count);
                    }

                }
                else if (listMode)
                {
                    var handles = SystemUtility.GetHandles((int)processId);
                    if (!string.IsNullOrWhiteSpace(args[2]))
                    {
                        var type = args[2].Trim();
                        handles = handles.Where(handle => string.Equals(type, handle.TypeString) || string.Equals(type, handle.RawType));
                    }

                    if(!handles.Any())
                    {
                        Console.Out.WriteLine("Cannot find any handle");
                    }

                    foreach (var handle in handles)
                    {
                        Console.Out.WriteLine("[{0}] {1} {2}", handle.RawHandleValue, string.IsNullOrWhiteSpace(handle.TypeString) ? handle.RawType.ToString() : handle.TypeString, handle.DosFilePath);
                    }
                }

                else if (verboseMode)
                {
                    var fileHandles = SystemUtility.GetHandles((int)processId).Where(handle => (handle.Type == HandleType.File));
                    foreach (var fileHandle in fileHandles)
                    {
                        if (fileHandle.DosFilePath != null)
                        {
                            Console.Out.WriteLine("[{0}] {1}", fileHandle.RawHandleValue, fileHandle.DosFilePath);
                        }
                    }
                }
                else if (closeMode)
                {
                    var fileHandles = SystemUtility.GetHandles((int)processId).Where(handle => (handle.Type == HandleType.File));
                    foreach (var fileHandle in fileHandles)
                    {
                        if (fileHandle.DosFilePath != null && fileHandle.RawHandleValue == handleToClose)
                        {
                            SystemUtility.CloseHandle(processId, fileHandle);
                        }

                    }
                }
                else if (defaultMode)
                {
                    var fileHandles = SystemUtility.GetHandles((int)processId).Where(handle => (handle.Type == HandleType.File));
                    foreach (var fileHandle in fileHandles)
                    {
                        if (fileHandle.DosFilePath != null)
                        {
                            Console.Out.WriteLine("{0}", fileHandle.DosFilePath);
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
            Console.Error.WriteLine("[Usage] KuduHandles.exe -l <ProcessId>");
            Console.Error.WriteLine("[Usage] KuduHandles.exe -l <ProcessId> <HandleType>");
            Console.Error.WriteLine("[Usage] KuduHandles.exe -s <ProcessId>");
            Console.Error.WriteLine("[Usage] KuduHandles.exe -v <ProcessId>");
            Console.Error.WriteLine("[Usage] KuduHandles.exe -c <ProcessId> <HandleId>");
        }
    }
}
