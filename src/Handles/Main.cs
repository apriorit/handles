using Microsoft.Win32.SafeHandles;
using System;

namespace handles
{
    class MainClass
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: handles [pid]\n");
                return 1;
            }

            int pid = Int32.Parse(args[0]);
            using (SafeFileHandle processHandle = Native.OpenProcess(Native.PROCESS_DUP_HANDLE, false, pid))
            {
                if (processHandle.IsInvalid)
                {
                    Console.WriteLine("Could not open PID {0}! (Don't try to open a system process.)\n", pid);
                    return 1;
                }
                
                foreach (var systemHandle in ProcessHandles.getHandles(pid))
                {
                    SafeFileHandle dupHandle;

                    if (Native.NtDuplicateObject(
                        processHandle,
                        (IntPtr)systemHandle.Handle,
                        Native.GetCurrentProcess(),
                        out dupHandle,
                        0,
                        0,
                        0
                        ) != NtStatus.Success)
                    {
                        continue;
                    }

                    using (dupHandle)
                    {
                         Console.WriteLine("Handle: 0x{0:X}", systemHandle.Handle);

                         string objectType = ProcessHandles.getObjectTypeInformation(dupHandle);
                         if (!String.IsNullOrEmpty(objectType))
                         {
                             Console.WriteLine("objectType: " + objectType);
                         }

                         string objectName = ProcessHandles.getObjectNameInformation(dupHandle);
                         if (!String.IsNullOrEmpty(objectName))
                         {
                             Console.WriteLine("objectName: " + objectName);
                         }

                        Console.WriteLine();
                    }
                }
            }

            return 0;
        }
    }
}
