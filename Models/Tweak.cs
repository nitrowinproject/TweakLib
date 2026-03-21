namespace TweakLib.Models
{
    public class Tweak
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required List<ActionBase> Actions { get; set; }
    }
}
