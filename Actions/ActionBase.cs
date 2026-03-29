using TweakLib.Helpers;
using TweakLib.Models;
using YamlDotNet.Serialization;

namespace TweakLib.Actions
{
    public abstract class ActionBase
    {
        [YamlMember(typeof(Privilege), Alias = "runas")]
        public Privilege RunAs { get; set; } = Privilege.CurrentUserElevated;
        public bool IgnoreErrors { get; set; } = false;
        public Platforms Platforms { get; set; } = new();
        public int Timeout { get; set; } = 30;

        public async Task<int> ApplyAsync()
        {
            if ((!Platforms.Mobile && PlatformHelper.IsMobile()) || (!Platforms.Desktop && !PlatformHelper.IsMobile()))
            {
                return 0;
            }

            return await ApplyAsyncCore().WaitAsync(TimeSpan.FromSeconds(Timeout));
        }

        protected abstract Task<int> ApplyAsyncCore();
    }
}
