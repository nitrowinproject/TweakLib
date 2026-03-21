using Microsoft.Win32;
using TweakLib.Models;

namespace TweakLib.Actions
{
    public enum RegistryValueOperation
    {
        Delete,
        Modify
    }

    public enum RegistryValueType
    {
        REG_SZ = RegistryValueKind.String,
        REG_MULTI_SZ = RegistryValueKind.MultiString,
        REG_EXPAND_SZ = RegistryValueKind.ExpandString,
        REG_DWORD = RegistryValueKind.DWord,
        REG_QWORD = RegistryValueKind.QWord,
        REG_BINARY = RegistryValueKind.Binary,
        REG_NONE = RegistryValueKind.None
    }

    public class RegistryValueAction : ActionBase
    {
        public required string Path { get; set; }
        public required string Value { get; set; }
        public string? Data { get; set; }
        public RegistryValueType Type { get; set; }
        public RegistryValueOperation Operation { get; set; } = RegistryValueOperation.Modify;
    }
}
