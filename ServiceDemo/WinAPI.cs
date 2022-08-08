using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using Microsoft.Win32;

namespace ServiceDemo
{
    public class Interop
    {
        public static IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;
        /// <summary>
        /// 服务程序执行消息提示,前台MessageBox.Show
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="title">标题</param>
        public static void ShowServiceMessage(string message, string title)
        {
            int resp = 0;
            WTSSendMessage(WTS_CURRENT_SERVER_HANDLE, WTSGetActiveConsoleSessionId(), title, title.Length, message, message.Length, 0, 0, out resp, false);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int WTSGetActiveConsoleSessionId();

        [DllImport("wtsapi32.dll", SetLastError = true)]
        public static extern bool WTSSendMessage(IntPtr hServer, int SessionId, String pTitle, int TitleLength, String pMessage, int MessageLength, int Style, int Timeout, out int pResponse, bool bWait);
        #region P/Invoke WTS APIs
        private enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,
            WTSConnected,
            WTSConnectQuery,
            WTSShadow,
            WTSDisconnected,
            WTSIdle,
            WTSListen,
            WTSReset,
            WTSDown,
            WTSInit
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct WTS_SESSION_INFO
        {
            public UInt32 SessionID;
            public string pWinStationName;
            public WTS_CONNECTSTATE_CLASS State;
        }

        [DllImport("WTSAPI32.DLL", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool WTSEnumerateSessions(
            IntPtr hServer,
            [MarshalAs(UnmanagedType.U4)] UInt32 Reserved,
            [MarshalAs(UnmanagedType.U4)] UInt32 Version,
            ref IntPtr ppSessionInfo,
            [MarshalAs(UnmanagedType.U4)] ref UInt32 pSessionInfoCount
            );

        [DllImport("WTSAPI32.DLL", SetLastError = true, CharSet = CharSet.Auto)]
        static extern void WTSFreeMemory(IntPtr pMemory);

        [DllImport("WTSAPI32.DLL", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool WTSQueryUserToken(UInt32 sessionId, out IntPtr Token);
        #endregion

        #region P/Invoke CreateProcessAsUser
        /// <summary> 
        /// Struct, Enum and P/Invoke Declarations for CreateProcessAsUser. 
        /// </summary> 
        ///  

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [Flags]
        enum CreateProcessFlags : uint
        {
            DEBUG_PROCESS = 0x00000001,
            DEBUG_ONLY_THIS_PROCESS = 0x00000002,
            CREATE_SUSPENDED = 0x00000004,
            DETACHED_PROCESS = 0x00000008,
            CREATE_NEW_CONSOLE = 0x00000010,
            NORMAL_PRIORITY_CLASS = 0x00000020,
            IDLE_PRIORITY_CLASS = 0x00000040,
            HIGH_PRIORITY_CLASS = 0x00000080,
            REALTIME_PRIORITY_CLASS = 0x00000100,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            CREATE_SEPARATE_WOW_VDM = 0x00000800,
            CREATE_SHARED_WOW_VDM = 0x00001000,
            CREATE_FORCEDOS = 0x00002000,
            BELOW_NORMAL_PRIORITY_CLASS = 0x00004000,
            ABOVE_NORMAL_PRIORITY_CLASS = 0x00008000,
            INHERIT_PARENT_AFFINITY = 0x00010000,
            INHERIT_CALLER_PRIORITY = 0x00020000,
            CREATE_PROTECTED_PROCESS = 0x00040000,
            EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
            PROCESS_MODE_BACKGROUND_BEGIN = 0x00100000,
            PROCESS_MODE_BACKGROUND_END = 0x00200000,
            CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
            CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
            CREATE_NO_WINDOW = 0x08000000,
            PROFILE_USER = 0x10000000,
            PROFILE_KERNEL = 0x20000000,
            PROFILE_SERVER = 0x40000000,
            CREATE_IGNORE_SYSTEM_DEFAULT = 0x80000000,
        }


        /// <summary>
        /// 以当前登录的windows用户(角色权限)运行指定程序进程
        /// </summary>
        /// <param name="hToken"></param>
        /// <param name="lpApplicationName">指定程序(全路径)</param>
        /// <param name="lpCommandLine">参数</param>
        /// <param name="lpProcessAttributes">进程属性</param>
        /// <param name="lpThreadAttributes">线程属性</param>
        /// <param name="bInheritHandles"></param>
        /// <param name="dwCreationFlags"></param>
        /// <param name="lpEnvironment"></param>
        /// <param name="lpCurrentDirectory"></param>
        /// <param name="lpStartupInfo">程序启动属性</param>
        /// <param name="lpProcessInformation">最后返回的进程信息</param>
        /// <returns>是否调用成功</returns>
        [DllImport("ADVAPI32.DLL", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool CreateProcessAsUser(IntPtr hToken, string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes,
                                                      bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory,
                                                      ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("KERNEL32.DLL", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool CloseHandle(IntPtr hHandle);

        [DllImport("userenv.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool CreateEnvironmentBlock(out IntPtr lpEnvironment, IntPtr hToken, bool bInherit);
        #endregion

        /// <summary>
        /// 以当前登录系统的用户角色权限启动指定的进程
        /// </summary>
        /// <param name="ChildProcName">指定的进程(全路径)</param>
        public static void CreateProcess(string ChildProcName, string parmeter)
        {
            IntPtr ppSessionInfo = IntPtr.Zero;
            UInt32 SessionCount = 0;
            if (WTSEnumerateSessions(
                                    (IntPtr)WTS_CURRENT_SERVER_HANDLE,  // Current RD Session Host Server handle would be zero. 
                                    0,  // This reserved parameter must be zero. 
                                    1,  // The version of the enumeration request must be 1. 
                                    ref ppSessionInfo, // This would point to an array of session info. 
                                    ref SessionCount  // This would indicate the length of the above array.
                                    ))
            {
                for (int nCount = 0; nCount < SessionCount; nCount++)
                {
                    WTS_SESSION_INFO tSessionInfo = (WTS_SESSION_INFO)Marshal.PtrToStructure(ppSessionInfo + nCount * Marshal.SizeOf(typeof(WTS_SESSION_INFO)), typeof(WTS_SESSION_INFO));
                    if (WTS_CONNECTSTATE_CLASS.WTSActive == tSessionInfo.State)
                    {
                        IntPtr hToken = IntPtr.Zero;
                        IntPtr lpEnvironment = IntPtr.Zero;
                        if (WTSQueryUserToken(tSessionInfo.SessionID, out hToken))
                        {
                            PROCESS_INFORMATION tProcessInfo;
                            STARTUPINFO tStartUpInfo = new STARTUPINFO();
                            tStartUpInfo.cb = Marshal.SizeOf(typeof(STARTUPINFO));
                            CreateProcessFlags t = CreateProcessFlags.NORMAL_PRIORITY_CLASS | 
                                                   //CreateProcessFlags.CREATE_NO_WINDOW |
                                                   CreateProcessFlags.CREATE_UNICODE_ENVIRONMENT;
                            CreateEnvironmentBlock(out lpEnvironment, hToken, false);

                            //int exeindex = ChildProcName.IndexOf("exe", StringComparison.Ordinal) + 3;
                            //string program = ChildProcName.Substring(1, exeindex);
                            //string param = ChildProcName.Substring(exeindex + 1, ChildProcName.LastIndexOf('"') - exeindex - 1).Trim();

                            string programPath = "";
                            using (RegistryKey myKey =
                                   Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\App Paths"))
                            {
                                if (myKey != null)
                                {
                                    var res = myKey.GetSubKeyNames();

                                    var IESubKey = res.FirstOrDefault(i => i.Contains("IEXPLORE.EXE"));

                                    if (!string.IsNullOrWhiteSpace(IESubKey))
                                    {
                                        programPath = myKey.OpenSubKey(IESubKey)?.GetValue("").ToString();
                                    }

                                }
                            }

                            using (RegistryKey myKey =
                                   Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths"))
                            {
                                if (myKey != null)
                                {
                                    var res = myKey.GetSubKeyNames();

                                    var IESubKey = res.FirstOrDefault(i => i.Contains("IEXPLORE.EXE"));

                                    if (!string.IsNullOrWhiteSpace(IESubKey))
                                    {
                                        programPath = myKey.OpenSubKey(IESubKey)?.GetValue("").ToString();
                                    }

                                }
                            }

                            /*不处理直接传递参数*/
                            bool childProcStarted = CreateProcessAsUser(
                                hToken,             // Token of the logged-on user. 
                                programPath,      // Name of the process to be started. 
                                $" \"{parmeter}\"",                   // Any command line arguments to be passed. 
                                IntPtr.Zero,        // Default Process' attributes. 
                                IntPtr.Zero,        // Default Thread's attributes. 
                                false,              // Does NOT inherit parent's handles. 
                                (uint)t,                  // No any specific creation flag. 
                                lpEnvironment,               // Default environment path. 
                                null,               // Default current directory. 
                                ref tStartUpInfo,   // Process Startup Info.  
                                out tProcessInfo    // Process information to be returned. 
                            );

                            /*从 CMD 启动程序需要打开 CreateProcessFlags.CREATE_NO_WINDOW 这个枚举，防止出现 CMD 窗口*/
                            //string program = ChildProcName.Split(' ')[0];
                            //string param = ChildProcName.Substring(ChildProcName.IndexOf(' ') + 1);
                            //bool childProcStarted = CreateProcessAsUser(
                            //    hToken,             // Token of the logged-on user. 
                            //    program,      // Name of the process to be started. 
                            //    " /b /c " + $"\"{param}\"",                   // Any command line arguments to be passed. 
                            //    IntPtr.Zero,        // Default Process' attributes. 
                            //    IntPtr.Zero,        // Default Thread's attributes. 
                            //    false,              // Does NOT inherit parent's handles. 
                            //    (uint)t,                  // No any specific creation flag. 
                            //    lpEnvironment,               // Default environment path. 
                            //    null,               // Default current directory. 
                            //    ref tStartUpInfo,   // Process Startup Info.  
                            //    out tProcessInfo    // Process information to be returned. 
                            //);

                            /*从 CMD 启动程序需要打开 CreateProcessFlags.CREATE_NO_WINDOW 这个枚举，防止出现 CMD 窗口 并传递 Authing CODE*/
                            //bool childProcStarted = CreateProcessAsUser(
                            //                                            hToken,             // Token of the logged-on user. 
                            //                                            program,      // Name of the process to be started. 
                            //                                            " /b /c " + $"\"{param} {GetParam(parmeter)["code"]}\"",                   // Any command line arguments to be passed. 
                            //                                            IntPtr.Zero,        // Default Process' attributes. 
                            //                                            IntPtr.Zero,        // Default Thread's attributes. 
                            //                                            false,              // Does NOT inherit parent's handles. 
                            //                                            (uint)t,                  // No any specific creation flag. 
                            //                                            lpEnvironment,               // Default environment path. 
                            //                                            null,               // Default current directory. 
                            //                                            ref tStartUpInfo,   // Process Startup Info.  
                            //                                            out tProcessInfo    // Process information to be returned. 
                            //                         );

                            /*直接启动程序并传递参数*/
                            //bool childProcStarted = CreateProcessAsUser(
                            //                                            hToken,             // Token of the logged-on user. 
                            //                                            ChildProcName,      // Name of the process to be started. 
                            //                                            $" {GetParam(parmeter)["code"]}",                   // Any command line arguments to be passed. 
                            //                                            IntPtr.Zero,        // Default Process' attributes. 
                            //                                            IntPtr.Zero,        // Default Thread's attributes. 
                            //                                            false,              // Does NOT inherit parent's handles. 
                            //                                            (uint)t,                  // No any specific creation flag. 
                            //                                            lpEnvironment,               // Default environment path. 
                            //                                            null,               // Default current directory. 
                            //                                            ref tStartUpInfo,   // Process Startup Info.  
                            //                                            out tProcessInfo    // Process information to be returned. 
                            //                         );

                            if (childProcStarted)
                            {
                                CloseHandle(tProcessInfo.hThread);
                                CloseHandle(tProcessInfo.hProcess);
                            }
                            else
                            {
                                //ShowServiceMessage("CreateProcessAsUser失败", "CreateProcess");     
                                ShowServiceMessage("IE 调用失败", "启动错误");
                            }
                            CloseHandle(hToken);
                            break;
                        }
                    }
                }
                WTSFreeMemory(ppSessionInfo);
            }
        }

        public static Dictionary<string, string> GetParam(string url)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            int start = 0, end = 0;
            var resstring = HttpUtility.UrlDecode(url);
            start = resstring.IndexOf("?");
            resstring = resstring.Substring(start + 1);
            start = 0;
            while (end != -1)
            {
                end = resstring.IndexOf("&", start, StringComparison.Ordinal);
                if (end != -1)
                {
                    var temp = resstring.Substring(start, end - start).Split('=');
                    res.Add(temp?[0], temp?[1]);
                }
                else
                {
                    var temp = resstring.Substring(start).Split('=');
                    if (temp.Length == 1) break;
                    res.Add(temp?[0], temp?[1]);
                }
                start = end + 1;
            }

            return res;
        }

    }
}
