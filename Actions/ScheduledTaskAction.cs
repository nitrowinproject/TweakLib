using Microsoft.Win32.TaskScheduler;

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

        protected override Task<int> ApplyAsyncCore()
        {
            using var ts = new TaskService();

            var task = ts.GetTask(Path) ?? throw new NullReferenceException();

            switch (Operation)
            {
                case ScheduledTaskActionOperation.Disable:
                    task.Enabled = false;
                    break;

                case ScheduledTaskActionOperation.Delete:
                    ts.RootFolder.DeleteTask(Path, false);
                    break;
            }

            return System.Threading.Tasks.Task.FromResult(0);
        }

        public ScheduledTaskAction()
        {
            if (RunAs == Models.Privilege.TrustedInstaller) throw new NotImplementedException();
        }
    }
}
