using TweakLib.Helpers;

namespace TweakLib.Actions
{
    public class CmdAction : ActionBase
    {
        public required string Command { get; set; }

        public async override Task<int> ApplyAsync() => await RunHelper.RunApplicationAsync("cmd.exe", "/c \"" + Command + "\"", RunAs);
    }
}
