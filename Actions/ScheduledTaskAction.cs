using TweakLib.Models;

namespace TweakLib.Actions
{
    public enum ScheduledTaskActionOperation
    {
        Delete,
        Enable,
        Disable
    }

    public class ScheduledTaskAction : ActionBase
    {
        public required string Path { get; set; }
        public required ScheduledTaskActionOperation Operation { get; set; }
    }
}
