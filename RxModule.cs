using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using static R3xLib.RxInjection;

namespace R3xLib
{
    public class RxModule
    {
        private WebClient wc = new WebClient();

        #region Dll 
        private bool DllCheckVer()
        {
            if (!File.Exists(RxSettings.ModuleVerPath)) return false;
            if (!File.Exists(RxSettings.ModulePath)) return false;
            string currentVer = File.ReadAllText(RxSettings.ModuleVerPath).Trim();
            string modVer = wc.DownloadString(RxSettings.ModuleVer).Trim();
            return currentVer == modVer;
        }

        private void DllDownload()
        {
            if (File.Exists(RxSettings.ModulePath)) File.Delete(RxSettings.ModulePath);
            if (File.Exists(RxSettings.ModuleVerPath)) File.Delete(RxSettings.ModuleVerPath);
            wc.DownloadFile(RxSettings.Module, RxSettings.ModuleName);
            bool done = File.Exists(RxSettings.ModulePath);
            if (!done)
            {
                throw new Exception("Failed to download");
            }

          /*  string shaOnline = wc.DownloadString(RxSettings.ModuleSha).Trim();
            bool valid = RxFunction.SHA256Verify(RxSettings.ModulePath, shaOnline);

            if (!valid)
            {
                throw new Exception("Invalid Sha SUM, Please download from official site");
            }*/

            wc.DownloadFile(RxSettings.ModuleVer, RxSettings.ModuleVerPath);
            return;
        }

        private void DllInject()
        {
            switch (DllInjector.GetInstance.Inject("RobloxPlayerBeta", Assembly.GetExecutingAssembly().Location + "\\" + RxSettings.ModuleName))
            {
                case RxInjectionResult.R_DllNotFound:
                    throw new Exception("Couldn't find the dll");
                case RxInjectionResult.R_GameNotFound:
                    throw new Exception("No Roblox found");
                case RxInjectionResult.R_Failed:
                    throw new Exception("Injection failed");
            }
        }

        #endregion

        #region Injection
        public void ExecuteScript(string Script)
        {
            if (namedPipeExist(RxSettings.ModuleCode))
            {
                using (NamedPipeClientStream pipeClientStream = new NamedPipeClientStream(".", RxSettings.ModuleCode, PipeDirection.Out))
                {
                    pipeClientStream.Connect();
                    using (StreamWriter streamWriter = new StreamWriter((Stream)pipeClientStream, Encoding.Default, 999999))
                    {
                        streamWriter.Write(Script);
                        streamWriter.Dispose();
                    }
                    pipeClientStream.Dispose();
                }
            }
            else if (File.Exists(RxSettings.ModulePath))
            {
                throw new Exception("Please attach!");
            }
            else
            {
                throw new Exception("DLL doesnt exist, turn off your antivirus");
            }
        }

        public bool isInjected() => namedPipeExist(RxSettings.ModuleCode);

        public void killRoblox()
        {
            foreach (Process process in Process.GetProcessesByName("RobloxPlayerBeta"))
                process.Kill();
        }

        public void LaunchExploit()
        {
            try
            {
                bool lastVer = DllCheckVer();
                if (!lastVer) DllDownload();
            }
            catch
            {
            }
            if (namedPipeExist(RxSettings.ModuleName))
            {
                throw new Exception("Already attached");
            }

            DllInject();
        }

        #endregion

        #region Pipe
        private static bool namedPipeExist(string pipeName)
        {
            bool flag;
            try
            {
                if (!WaitNamedPipe(Path.GetFullPath(string.Format("\\\\.\\pipe\\{0}", (object)pipeName)), 0))
                {
                    switch (Marshal.GetLastWin32Error())
                    {
                        case 0:
                            return false;
                        case 2:
                            return false;
                    }
                }
                flag = true;
            }
            catch (Exception)
            {
                flag = false;
            }
            return flag;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool WaitNamedPipe(string name, int timeout);

        #endregion

    }
}
