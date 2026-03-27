using System.ServiceProcess;
using TweakLib.Helpers;

namespace TweakLib.Actions
{
    public enum ServiceActionOperation
    {
        Stop,
        Continue,
        Start,
        Pause,
        Disable,
        Delete
    }

    public class ServiceAction : ActionBase
    {
        public required string Name { get; set; }
        public required ServiceActionOperation Operation { get; set; }

        public async override Task<int> ApplyAsync()
        {
            switch (Operation)
            {
                case ServiceActionOperation.Delete:
                    return await RunHelper.RunApplicationAsync("sc.exe", $"delete {Name}", RunAs);

                case ServiceActionOperation.Disable:
                    return await RunHelper.RunApplicationAsync("sc.exe", $"config {Name} start= disabled", RunAs);
            }

            using ServiceController sc = new(Name);
            switch (Operation)
            {
                case ServiceActionOperation.Start:
                    if (sc.Status == ServiceControllerStatus.Stopped)
                        sc.Start();
                    break;

                case ServiceActionOperation.Stop:
                    if (sc.Status == ServiceControllerStatus.Running)
                        sc.Stop();
                    break;

                case ServiceActionOperation.Pause:
                    if (sc.CanPauseAndContinue && sc.Status == ServiceControllerStatus.Running)
                        sc.Pause();
                    break;

                case ServiceActionOperation.Continue:
                    if (sc.CanPauseAndContinue && sc.Status == ServiceControllerStatus.Paused)
                        sc.Continue();
                    break;
            }

            await ServiceHelper.WaitForStatusAsync(sc, ServiceHelper.GetTargetStatus(Operation), TimeSpan.FromSeconds(10));
            return 0;
        }
    }
}
