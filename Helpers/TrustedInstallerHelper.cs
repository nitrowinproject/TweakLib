using Microsoft.Win32.TaskScheduler;

namespace TweakLib.Helpers
{
    public static class TrustedInstallerHelper
    {
        public static int RunAsTrustedInstaller(string filePath, string? arguments)
        {
            string taskName = "RunAsTiTweak" + new Random().Next(999);

            using TaskService ts = new();

            TaskDefinition td = ts.NewTask();
            td.Actions.Add(new ExecAction(filePath, arguments));

            ts.RootFolder.RegisterTaskDefinition(taskName, td);

            var task = ts.GetTask(taskName);

            RunningTask runningTask = task.RunEx(TaskRunFlags.NoFlags, 0, @"NT SERVICE\TrustedInstaller");

            while (runningTask != null && runningTask.State == TaskState.Running)
            {
                Thread.Sleep(200);
                runningTask.Refresh();
            }

            var result = task.LastTaskResult;

            ts.RootFolder.DeleteTask(taskName);

            return result;
        }
    }
}
