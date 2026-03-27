using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TweakLib.Helpers
{
    public static class TrustedInstallerHelper
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint access, bool inherit, int pid);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr process, uint access, out IntPtr token);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool DuplicateTokenEx(
            IntPtr existingToken,
            uint access,
            IntPtr tokenAttributes,
            int impersonationLevel,
            int tokenType,
            out IntPtr newToken);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool CreateProcessAsUser(
            IntPtr token,
            string app,
            string cmdLine,
            IntPtr procAttr,
            IntPtr threadAttr,
            bool inherit,
            uint flags,
            IntPtr env,
            string cwd,
            ref STARTUPINFO si,
            out PROCESS_INFORMATION pi);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr h);

        private const uint PROCESS_QUERY_INFORMATION = 0x0400;
        private const uint TOKEN_DUPLICATE = 0x0002;
        private const uint TOKEN_QUERY = 0x0008;
        private const uint TOKEN_ALL_ACCESS = 0xF01FF;

        private const int SecurityImpersonation = 2;
        private const int TokenPrimary = 1;

        public static void RunAsTrustedInstaller(string exe, string? arguments)
        {
            var winlogon = Process.GetProcessesByName("winlogon")[0];
            IntPtr hProc = OpenProcess(PROCESS_QUERY_INFORMATION, false, winlogon.Id);

            if (!OpenProcessToken(hProc, TOKEN_DUPLICATE | TOKEN_QUERY, out IntPtr sysToken))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            if (!DuplicateTokenEx(sysToken, TOKEN_ALL_ACCESS, IntPtr.Zero, SecurityImpersonation, TokenPrimary, out IntPtr sysDup))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            CloseHandle(sysToken);
            CloseHandle(hProc);

            Process.Start(new ProcessStartInfo
            {
                FileName = "sc",
                Arguments = "start TrustedInstaller",
                CreateNoWindow = true,
                UseShellExecute = false
            })?.WaitForExit();

            Process tiProc = null;
            foreach (var p in Process.GetProcessesByName("TrustedInstaller"))
            {
                tiProc = p;
                break;
            }

            if (tiProc == null)
            {
                throw new NullReferenceException();
            }

            IntPtr hTi = OpenProcess(PROCESS_QUERY_INFORMATION, false, tiProc.Id);

            if (!OpenProcessToken(hTi, TOKEN_DUPLICATE | TOKEN_QUERY, out IntPtr tiToken))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            if (!DuplicateTokenEx(tiToken, TOKEN_ALL_ACCESS, IntPtr.Zero,
                SecurityImpersonation, TokenPrimary, out IntPtr tiDup))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            CloseHandle(tiToken);
            CloseHandle(hTi);

            STARTUPINFO si = new STARTUPINFO();
            si.cb = Marshal.SizeOf(si);

            string cmdLine = string.IsNullOrEmpty(arguments) ? $"\"{exe}\"" : $"\"{exe}\" {arguments}";

            if (!CreateProcessAsUser(tiDup, null, cmdLine, IntPtr.Zero, IntPtr.Zero, false, 0, IntPtr.Zero, null, ref si, out PROCESS_INFORMATION pi))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            CloseHandle(pi.hProcess);
            CloseHandle(pi.hThread);
            CloseHandle(tiDup);
        }

        private struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX, dwY, dwXSize, dwYSize;
            public int dwFlags;
        }

        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }
    }
}
