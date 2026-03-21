namespace TweakLib.Models.Actions
{
    public class RunAction : ActionBase
    {
        public required string Exe { get; set; }
        public string? Args { get; set; }
    }
}
