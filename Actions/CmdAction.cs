using TweakLib.Helpers;

namespace TweakLib.Actions
{
    public class CmdAction : ActionBase
    {
        public required string Command { get; set; }

        protected async override Task<int> ApplyAsyncCore() => await RunHelper.RunApplicationAsync("cmd.exe", "/c \"" + Command + "\"", RunAs);
    }
}
