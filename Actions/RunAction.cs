using TweakLib.Helpers;

namespace TweakLib.Actions
{
    public class RunAction : ActionBase
    {
        public required string Exe { get; set; }
        public string? Args { get; set; }

        protected async override Task<int> ApplyAsyncCore() => await RunHelper.RunApplicationAsync(Exe, Args, RunAs);
    }
}
