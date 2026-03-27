using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TweakLib.Helpers
{
    public static class TrustedInstallerHelper
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(
            IntPtr processHandle,
            uint desiredAccess,
            out IntPtr tokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool DuplicateTokenEx(
            IntPtr existingTokenHandle,
            uint desiredAccess,
            IntPtr tokenAttributes,
            int impersonationLevel,
            int tokenType,
            out IntPtr duplicateTokenHandle);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool CreateProcessWithTokenW(
            IntPtr hToken,
            uint dwLogonFlags,
            string lpApplicationName,
            string lpCommandLine,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool LookupPrivilegeValue(
            string lpSystemName,
            string lpName,
            ref LUID lpLuid);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AdjustTokenPrivileges(
            IntPtr tokenHandle,
            [MarshalAs(UnmanagedType.Bool)] bool disableAllPrivileges,
            ref TOKEN_PRIVILEGES newState,
            uint bufferLength,
            IntPtr previousState,
            IntPtr returnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr OpenSCManager(
            string machineName,
            string databaseName,
            uint dwAccess);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr OpenService(
            IntPtr hSCManager,
            string lpServiceName,
            uint dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool StartService(
            IntPtr hService,
            uint dwNumServiceArgs,
            string[] lpServiceArgVectors);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool QueryServiceStatus(
            IntPtr hService,
            ref SERVICE_STATUS lpServiceStatus);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(
            uint processAccess,
            bool bInheritHandle,
            uint processId);


        [StructLayout(LayoutKind.Sequential)]
        private struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX, dwY, dwXSize, dwYSize;
            public uint dwXCountChars, dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public ushort wShowWindow;
            public ushort cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput, hStdOutput, hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public uint Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_PRIVILEGES
        {
            public uint PrivilegeCount;
            public LUID_AND_ATTRIBUTES Privileges;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SERVICE_STATUS
        {
            public uint dwServiceType;
            public uint dwCurrentState;
            public uint dwControlsAccepted;
            public uint dwWin32ExitCode;
            public uint dwServiceSpecificExitCode;
            public uint dwCheckPoint;
            public uint dwWaitHint;
        }

        private const uint TOKEN_ALL_ACCESS = 0xF01FF;
        private const uint TOKEN_DUPLICATE = 0x0002;
        private const uint TOKEN_QUERY = 0x0008;
        private const uint PROCESS_QUERY_INFO = 0x0400;
        private const uint SC_MANAGER_ALL_ACCESS = 0xF003F;
        private const uint SERVICE_ALL_ACCESS = 0xF01FF;
        private const uint SERVICE_RUNNING = 0x00000004;
        private const uint SE_PRIVILEGE_ENABLED = 0x00000002;
        private const string SE_DEBUG_NAME = "SeDebugPrivilege";

        private const int SecurityImpersonation = 2;
        private const int TokenPrimary = 1;


        public static uint RunAsTrustedInstaller(string executablePath, string? arguments = "")
        {
            EnableDebugPrivilege();

            uint tiPid = StartTrustedInstallerService();

            IntPtr tiProcess = OpenProcess(PROCESS_QUERY_INFO, false, tiPid);
            if (tiProcess == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            try
            {
                if (!OpenProcessToken(tiProcess, TOKEN_DUPLICATE | TOKEN_QUERY, out IntPtr tiToken))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                try
                {
                    if (!DuplicateTokenEx(tiToken, TOKEN_ALL_ACCESS, IntPtr.Zero, SecurityImpersonation, TokenPrimary, out IntPtr dupToken))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }

                    try
                    {
                        var si = new STARTUPINFO { cb = (uint)Marshal.SizeOf<STARTUPINFO>() };
                        string cmdLine = string.IsNullOrEmpty(arguments) ? null : $"\"{executablePath}\" {arguments}";

                        if (!CreateProcessWithTokenW(dupToken, 0, executablePath, cmdLine, 0, IntPtr.Zero, null, ref si, out PROCESS_INFORMATION pi))
                        {
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                        }

                        CloseHandle(pi.hThread);
                        CloseHandle(pi.hProcess);

                        return pi.dwProcessId;
                    }
                    finally { CloseHandle(dupToken); }
                }
                finally { CloseHandle(tiToken); }
            }
            finally { CloseHandle(tiProcess); }
        }


        private static void EnableDebugPrivilege()
        {
            if (!OpenProcessToken(Process.GetCurrentProcess().Handle, 0x0020 | TOKEN_QUERY, out IntPtr token))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            try
            {
                var tp = new TOKEN_PRIVILEGES { PrivilegeCount = 1 };
                if (!LookupPrivilegeValue(null, SE_DEBUG_NAME, ref tp.Privileges.Luid))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                tp.Privileges.Attributes = SE_PRIVILEGE_ENABLED;

                if (!AdjustTokenPrivileges(token, false, ref tp, (uint)Marshal.SizeOf(tp), IntPtr.Zero, IntPtr.Zero))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            finally { CloseHandle(token); }
        }

        private static uint StartTrustedInstallerService()
        {
            IntPtr scm = OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);
            if (scm == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            try
            {
                IntPtr svc = OpenService(scm, "TrustedInstaller", SERVICE_ALL_ACCESS);
                if (svc == IntPtr.Zero)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                try
                {
                    var status = new SERVICE_STATUS();

                    if (!StartService(svc, 0, null))
                    {
                        int err = Marshal.GetLastWin32Error();

                        if (err != 1056)
                        {
                            throw new Win32Exception(err);
                        }
                    }

                    for (int i = 0; i < 20; i++)
                    {
                        QueryServiceStatus(svc, ref status);

                        if (status.dwCurrentState == SERVICE_RUNNING)
                        {
                            break;
                        }

                        Thread.Sleep(200);
                    }

                    if (status.dwCurrentState != SERVICE_RUNNING)
                    {
                        throw new InvalidOperationException();
                    }

                    foreach (var p in Process.GetProcessesByName("TrustedInstaller"))
                    {
                        return (uint)p.Id;
                    }

                    throw new InvalidOperationException();
                }
                finally { CloseHandle(svc); }
            }
            finally { CloseHandle(scm); }
        }
    }
}
