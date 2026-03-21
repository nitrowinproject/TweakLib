using YamlDotNet.Serialization;

namespace TweakLib.Models
{
    public abstract class ActionBase
    {
        [YamlMember(typeof(Privilege), Alias = "runas")]
        public Privilege RunAs { get; set; } = Privilege.CurrentUserElevated;
        public bool IgnoreErrors { get; set; } = false;
    }
}
