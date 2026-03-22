using System.Diagnostics;

namespace TweakLib.Helpers
{
    internal static class RunHelper
    {
        internal static async Task<int> RunApplicationAsync(string fileName, string? arguments)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = fileName,
                Arguments = arguments
            };

            var process = Process.Start(startInfo);

            if (process == null) { return -1; }

            await process.WaitForExitAsync();

            return process.ExitCode;
        }
    }
}
