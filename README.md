# TweakLib

A simple .NET library that powers the tweaks in NitroWin. Compatible with AME tweaks.

## Features

- Compatible with AME tweaks
- Usable outside of AME playbooks

## Usage

This is how a tweak file could look:

```yaml
---
title: Example Tweak
description: This an example
actions:
  - !cmd:
    command: "echo Hello World!"
  - !registryValue:
    path: 'HKCU\Control Panel\Accessibility\StickyKeys'
    value: "Flags"
    data: "3a"
    type: REG_DWORD
```

This is how you would apply it:

```csharp
using TweakLib.Models;
using TweakLib.Parser;

var yaml = await File.ReadAllTextAsync("disable-sticky-keys.yml");

var tweak = TweakParser.Deserializer.Deserialize<Tweak>(yaml);

foreach (var action in tweak.Actions)
{
    await action.ApplyAsync();
}
```

## License

This project is licensed under the [Zero-Clause BSD License](LICENSE).

### Credits

This project uses modified code from [trusted-uninstaller-cli](https://github.com/Ameliorated-LLC/trusted-uninstaller-cli/) by [Ameliorated-LLC](https://github.com/Ameliorated-LLC), which is licensed under the [MIT License](https://raw.githubusercontent.com/Ameliorated-LLC/trusted-uninstaller-cli/refs/heads/public/LICENSE.md).
