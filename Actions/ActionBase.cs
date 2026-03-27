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

        public async Task<int> ApplyAsync()
        {
            if ((!Platforms.Mobile && PlatformHelper.IsMobile()) || (!Platforms.Desktop && !PlatformHelper.IsMobile()))
            {
                return 0;
            }

            return await ApplyAsyncCore();
        }

        protected abstract Task<int> ApplyAsyncCore();
    }
}
