using TweakLib.Models;
using YamlDotNet.Serialization;

namespace TweakLib.Actions
{
    public abstract class ActionBase
    {
        [YamlMember(typeof(Privilege), Alias = "runas")]
        public Privilege RunAs { get; set; } = Privilege.CurrentUserElevated;
        public bool IgnoreErrors { get; set; } = false;
        public abstract Task<int> ApplyAsync();
    }
}
