using TweakLib.Helpers;
using TweakLib.Models;

namespace TweakLib.Actions
{
    public class CmdAction : ActionBase
    {
        public required string Command { get; set; }

        public async override Task<int> RunTask() => await RunHelper.RunApplication("cmd.exe", "/c \"" + Command + "\"");
    }
}
