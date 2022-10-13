using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace R3xLib
{
    internal class RxInjection
    {
        public enum RxInjectionResult
        {
            R_DllNotFound,
            R_GameNotFound,
            R_Failed,
            R_Success,
        }

        public sealed class DllInjector
        {
            private static readonly IntPtr INTPTR_ZERO = (IntPtr)0;
            private static DllInjector _instance;
            public static DllInjector GetInstance
            {
                get
                {
                    if (_instance == null) _instance = new DllInjector();
                    return _instance;
                }
            }

            private DllInjector()
            {
            }

            #region Injector
            private bool bInject(uint pToBeInjected, string sDllPath)
            {
                IntPtr num1 = OpenProcess(1082U, 1, pToBeInjected);
                if (num1 == INTPTR_ZERO)
                    return false;
                IntPtr procAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
                if (procAddress == INTPTR_ZERO)
                    return false;
                IntPtr num2 = VirtualAllocEx(num1, (IntPtr)0, (IntPtr)sDllPath.Length, 12288U, 64U);
                if (num2 == INTPTR_ZERO)
                    return false;
                byte[] bytes = Encoding.ASCII.GetBytes(sDllPath);
                if (WriteProcessMemory(num1, num2, bytes, (uint)bytes.Length, 0) == 0 || CreateRemoteThread(num1, (IntPtr)0, INTPTR_ZERO, procAddress, num2, 0U, (IntPtr)0) == INTPTR_ZERO)
                    return false;
                CloseHandle(num1);
                return true;
            }
            public RxInjectionResult Inject(string sProcName, string sDllPath)
            {
                if (!File.Exists(sDllPath))
                    return RxInjectionResult.R_DllNotFound;
                uint pToBeInjected = 0;
                Process[] processes = Process.GetProcesses();
                for (int index = 0; index < processes.Length; ++index)
                {
                    if (!(processes[index].ProcessName != sProcName))
                    {
                        pToBeInjected = (uint)processes[index].Id;
                        break;
                    }
                }
                if (pToBeInjected == 0U)
                    return RxInjectionResult.R_GameNotFound;
                return !bInject(pToBeInjected, sDllPath) ? RxInjectionResult.R_Failed : RxInjectionResult.R_Success;
            }
            #endregion

            #region Import Kernel32
            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern int CloseHandle(IntPtr hObject);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, 
                IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, 
                uint flAllocationType, uint flProtect);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, 
                uint size, int lpNumberOfBytesWritten);
            #endregion
        }
    }
}
