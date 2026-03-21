using TweakLib.Actions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TweakLib.Parser
{
    public static class TweakParser
    {
        public static IDeserializer Deserializer { get; } = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTagMapping("!cmd:", typeof(CmdAction))
            .WithTagMapping("!powerShell:", typeof(PowerShellAction))
            .WithTagMapping("!registryValue:", typeof(RegistryValueAction))
            .WithTagMapping("!run:", typeof(RunAction))
            .WithTagMapping("!scheduledTask:", typeof(ScheduledTaskAction))
            .WithTagMapping("!service:", typeof(ServiceAction))
            .Build();

        public static ISerializer Serializer { get; } = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTagMapping("!cmd:", typeof(CmdAction))
            .WithTagMapping("!powerShell:", typeof(PowerShellAction))
            .WithTagMapping("!registryValue:", typeof(RegistryValueAction))
            .WithTagMapping("!run:", typeof(RunAction))
            .WithTagMapping("!scheduledTask:", typeof(ScheduledTaskAction))
            .WithTagMapping("!service:", typeof(ServiceAction))
            .EnsureRoundtrip()
            .Build();
    }
}
