namespace TweakLib.Models.Actions
{
    public enum ServiceActionOperation
    {
        Stop,
        Continue,
        Start,
        Pause,
        Delete,
        Change
    }

    public class ServiceAction : ActionBase
    {
        public required string Name { get; set; }
        public required ServiceActionOperation Operation { get; set; }
    }
}
