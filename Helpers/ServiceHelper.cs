using System.ServiceProcess;
using TweakLib.Actions;

namespace TweakLib.Helpers
{
    public static class ServiceHelper
    {
        public static async Task WaitForStatusAsync(ServiceController sc, ServiceControllerStatus desiredStatus, TimeSpan timeout)
        {
            var startTime = DateTime.UtcNow;

            while (sc.Status != desiredStatus)
            {
                if (DateTime.UtcNow - startTime > timeout)
                    throw new System.TimeoutException($"Service did not reach status {desiredStatus}.");

                await Task.Delay(500);
                sc.Refresh();
            }
        }

        public static ServiceControllerStatus GetTargetStatus(ServiceActionOperation action)
        {
            return action switch
            {
                ServiceActionOperation.Start => ServiceControllerStatus.Running,
                ServiceActionOperation.Stop => ServiceControllerStatus.Stopped,
                ServiceActionOperation.Pause => ServiceControllerStatus.Paused,
                ServiceActionOperation.Continue => ServiceControllerStatus.Running,
                _ => throw new NotSupportedException()
            };
        }
    }
}
