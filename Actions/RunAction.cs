using TweakLib.Helpers;
using TweakLib.Models;

namespace TweakLib.Actions
{
    public class RunAction : ActionBase
    {
        public required string Exe { get; set; }
        public string? Args { get; set; }

        public async override Task<int> RunTask() => await RunHelper.RunApplicationAsync(Exe, Args);
    }
}
