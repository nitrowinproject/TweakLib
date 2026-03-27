using TweakLib.Helpers;

namespace TweakLib.Actions
{
    public class PowerShellAction : ActionBase
    {
        public required string Command { get; set; }

        protected async override Task<int> ApplyAsyncCore() => await RunHelper.RunApplicationAsync("powershell.exe", $"-ExecutionPolicy Bypass -NoProfile -Command \"{Command}\"", RunAs);
    }
}
