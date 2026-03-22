using Microsoft.Win32.TaskScheduler;
using TweakLib.Models;

namespace TweakLib.Actions
{
    public enum ScheduledTaskActionOperation
    {
        Delete,
        Disable
    }

    public class ScheduledTaskAction : ActionBase
    {
        public required string Path { get; set; }
        public required ScheduledTaskActionOperation Operation { get; set; }

        public override Task<int> RunTask()
        {
            switch (Operation)
            {
                case ScheduledTaskActionOperation.Delete:
                    using (var ts = new TaskService())
                    {
                        var task = ts.GetTask(Path);
                        task.Definition.Settings.Enabled = false;
                        task.RegisterChanges();
                    }
                    break;
                case ScheduledTaskActionOperation.Disable:
                    using (var ts = new TaskService())
                    {
                        var task = ts.GetTask(Path);
                        ts.RootFolder.DeleteTask(Path);
                        task.RegisterChanges();
                    }
                    break;
            }
            return System.Threading.Tasks.Task.FromResult(0);
        }
    }
}
