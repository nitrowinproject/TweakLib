using TweakLib.Helpers;

namespace TweakLib.Actions
{
    public class RunAction : ActionBase
    {
        public required string Exe { get; set; }
        public string? Args { get; set; }

        public async override Task<int> ApplyAsync() => await RunHelper.RunApplicationAsync(Exe, Args, RunAs);
    }
}
