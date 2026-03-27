using Microsoft.Win32;
using TweakLib.Helpers;

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
        public RegistryValueType? Type { get; set; }
        public RegistryValueOperation Operation { get; set; } = RegistryValueOperation.Modify;

        private void ApplyAsCurrentUserElevated()
        {
            var parts = Path.Split('\\').ToList();
            var hive = parts[0];
            parts.RemoveAt(0);

            string subKey = string.Join("\\", parts);

            if (Operation == RegistryValueOperation.Delete)
            {
                RegistryKey baseKey = hive switch
                {
                    "HKCR" or "HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
                    "HKCU" or "HKEY_CURRENT_USER" => Registry.CurrentUser,
                    "HKLM" or "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
                    _ => throw new NotImplementedException()
                };

                using var key = baseKey.OpenSubKey(subKey, writable: true);
                key?.DeleteValue(Value, false);

                return;
            }

            string baseName = hive switch
            {
                "HKCR" => "HKEY_CLASSES_ROOT",
                "HKCU" => "HKEY_CURRENT_USER",
                "HKLM" => "HKEY_LOCAL_MACHINE",
                "HKU" => "HKEY_USERS",
                "HKCC" => "HKEY_CURRENT_CONFIG",
                _ => hive
            };

            string keyName = baseName + "\\" + subKey;

            switch (Type)
            {
                case RegistryValueType.REG_SZ:
                    Registry.SetValue(keyName, Value, Data ?? string.Empty, (RegistryValueKind)Type);
                    break;

                case RegistryValueType.REG_MULTI_SZ:
                    string[] data = string.IsNullOrEmpty(Data)
                        ? Array.Empty<string>()
                        : Data.Split('\0');

                    Registry.SetValue(keyName, Value, data, (RegistryValueKind)Type);
                    break;

                case RegistryValueType.REG_DWORD:
                    Registry.SetValue(keyName, Value, unchecked((int)Convert.ToUInt32(Data)), (RegistryValueKind)Type);
                    break;

                case RegistryValueType.REG_QWORD:
                    Registry.SetValue(keyName, Value, Convert.ToUInt64(Data), (RegistryValueKind)Type);
                    break;

                case RegistryValueType.REG_BINARY:
                    var binary = RegistryHelper.StringToByteArray(Data ?? string.Empty);
                    Registry.SetValue(keyName, Value, binary, (RegistryValueKind)Type);
                    break;

                case RegistryValueType.REG_NONE:
                    Registry.SetValue(keyName, Value, Array.Empty<byte>(), (RegistryValueKind)Type);
                    break;
            }
        }

        private void ApplyAsTrustedInstaller()
        {
            string type = Type switch
            {
                RegistryValueType.REG_SZ => "REG_SZ",
                RegistryValueType.REG_MULTI_SZ => "REG_MULTI_SZ",
                RegistryValueType.REG_DWORD => "REG_DWORD",
                RegistryValueType.REG_QWORD => throw new NotSupportedException(),
                RegistryValueType.REG_BINARY => "REG_BINARY",
                RegistryValueType.REG_NONE => throw new NotSupportedException(),
                _ => Operation != RegistryValueOperation.Delete ? string.Empty : throw new NotSupportedException()
            };

            string arguments = Operation switch
            {
                RegistryValueOperation.Modify => $"add {Path} /v {Value} /t {type} /d {Data} /f",
                RegistryValueOperation.Delete => $"delete {Path} /f",
                _ => throw new NotImplementedException()
            };

            TrustedInstallerHelper.RunAsTrustedInstaller("reg.exe", arguments);
        }

        public override async Task<int> ApplyAsync()
        {
            if (Operation == RegistryValueOperation.Delete && Type != null)
            {
                throw new NotImplementedException();
            }

            switch (RunAs)
            {
                case Models.Privilege.CurrentUserElevated:
                    await Task.Run(ApplyAsCurrentUserElevated);
                    return 0;

                case Models.Privilege.TrustedInstaller:
                    await Task.Run(ApplyAsTrustedInstaller);
                    return 0;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
