using System.Diagnostics;
using TweakLib.Models;

namespace TweakLib.Helpers
{
    internal static class RunHelper
    {
        internal static async Task<int> RunApplicationAsync(string fileName, string? arguments, Privilege privilege)
        {
            if (privilege != Privilege.TrustedInstaller)
            {
                var startInfo = new ProcessStartInfo()
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = true
                };

                var process = Process.Start(startInfo);

                if (process == null) { return -1; }

                await process.WaitForExitAsync();

                return process.ExitCode;
            }
            else
            {
                return TrustedInstallerHelper.RunAsTrustedInstaller(fileName, arguments);
            }
        }
    }
}
