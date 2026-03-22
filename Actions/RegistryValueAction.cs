using Microsoft.Win32;
using TweakLib.Helpers;
using TweakLib.Models;

namespace TweakLib.Actions
{
    public enum RegistryValueOperation
    {
        Modify,
        Delete
    }

    public enum RegistryValueType
    {
        REG_SZ = RegistryValueKind.String,
        REG_MULTI_SZ = RegistryValueKind.MultiString,
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

        public override Task<int> ApplyAsync()
        {
            if (Operation == RegistryValueOperation.Delete)
            {
                var parts = Path.Split('\\').ToList();
                var hive = parts[0];
                parts.RemoveAt(0);

                string subKeyPath = string.Join("\\", parts);

                RegistryKey baseKey = hive switch
                {
                    "HKCU" => Registry.CurrentUser,
                    "HKLM" => Registry.LocalMachine,
                    "HKCR" => Registry.ClassesRoot,
                    _ => throw new NotImplementedException()
                };

                using var key = baseKey.OpenSubKey(subKeyPath, writable: true);
                key?.DeleteValue(Value, false);

                return Task.FromResult(0);
            }

            switch (Type)
            {
                case RegistryValueType.REG_SZ:
                    Registry.SetValue(Path, Value, Data, (RegistryValueKind)Type);
                    break;

                case RegistryValueType.REG_MULTI_SZ:
                    string[] data = string.IsNullOrEmpty(Data)
                        ? Array.Empty<string>()
                        : Data.Split('\0');

                    Registry.SetValue(Path, Value, data, (RegistryValueKind)Type);
                    break;

                case RegistryValueType.REG_DWORD:
                    Registry.SetValue(Path, Value, unchecked((int)Convert.ToUInt32(Data)), (RegistryValueKind)Type);
                    break;

                case RegistryValueType.REG_QWORD:
                    Registry.SetValue(Path, Value, Convert.ToUInt64(Data), (RegistryValueKind)Type);
                    break;

                case RegistryValueType.REG_BINARY:
                    var binary = RegistryHelper.StringToByteArray(Data);
                    Registry.SetValue(Path, Value, binary, (RegistryValueKind)Type);
                    break;

                case RegistryValueType.REG_NONE:
                    Registry.SetValue(Path, Value, Array.Empty<byte>(), (RegistryValueKind)Type);
                    break;
            }

            return Task.FromResult(0);
        }
    }
}
