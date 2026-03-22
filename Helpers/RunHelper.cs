using System.Diagnostics;

namespace TweakLib.Helpers
{
    public static class RunHelper
    {
        public static async Task<int> RunApplication(string fileName, string? arguments)
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
