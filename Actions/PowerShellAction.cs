using TweakLib.Helpers;
using TweakLib.Models;

namespace TweakLib.Actions
{
    public class PowerShellAction : ActionBase
    {
        public required string Command { get; set; }

        public async override Task<int> ApplyAsync() => await RunHelper.RunApplicationAsync("powershell.exe", $"-ExecutionPolicy Bypass -NoProfile -Command \"{Command}\"");
    }
}
