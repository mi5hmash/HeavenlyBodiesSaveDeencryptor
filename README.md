[![License: MIT](https://img.shields.io/badge/License-MIT-blueviolet.svg)](https://opensource.org/license/mit)
[![Release Version](https://img.shields.io/github/v/tag/mi5hmash/HeavenlyBodiesSaveDeencryptor?label=Version)](https://github.com/mi5hmash/HeavenlyBodiesSaveDeencryptor/releases/latest)
[![Visual Studio 2026](https://custom-icon-badges.demolab.com/badge/Visual%20Studio%202026-F0ECF8.svg?&logo=visual-studio-26)](https://visualstudio.microsoft.com/)
[![.NET10](https://img.shields.io/badge/.NET%2010-512BD4?logo=dotnet&logoColor=fff)](#)

> [!IMPORTANT]
> **This software is free and open source. If someone asks you to pay for it, it's likely a scam.**

# 👨‍🚀 HeavenlyBodiesSaveDeencryptor - What is it :interrobang:
![icon](https://github.com/mi5hmash/HeavenlyBodiesSaveDeencryptor/blob/main/.resources/images/HBSD-logo.png)

This console application can **decrypt and encrypt SaveData files** from the Heavenly Bodies game.

> [!NOTE]
The game uses the Easy Save 3 system. If another game also relies on this save‑file serialization system and you know the correct password, then my tool should be able to decrypt those files as well.

# :scream: Is it safe?
The short answer is: **No.** 
> [!CAUTION]
> If you unreasonably edit your SaveData files, you risk corrupting them lose your progress.

> [!IMPORTANT]
> Always back up the files you intend to edit before editing them.

> [!IMPORTANT]
> Disable the Steam Cloud before you replace any SaveData files.

You have been warned, and now that you are completely aware of what might happen, you may proceed to the next chapter.

# :scroll: How to use this tool
## [CLI] - 🪟 Windows | 🐧 Linux | 🍎 macOS

```plaintext
Usage: .\heavenly-bodies-save-deencryptor-cli.exe -m <mode> [options]

Modes:
  -m d  Decrypt SaveData files
  -m e  Encrypt SaveData files

Options:
  -p <input_folder_path>  Path to folder containing SaveData files
  -g <code>               Game code to use for decryption/encryption (optional)
  -nc                     Disables compression when encrypting (optional)
  -v                      Verbose output
  -h                      Show this help message
```

### Examples
#### Decrypt
```bash
.\heavenly-bodies-save-deencryptor-cli.exe -m d -p ".\InputDirectory"
```
#### Encrypt
```bash
.\heavenly-bodies-save-deencryptor-cli.exe -m e -p ".\InputDirectory"
```

#### Decrypt with custom game code
```bash
.\heavenly-bodies-save-deencryptor-cli.exe -m d -p ".\InputDirectory" -g "my-custom-password"
```

#### Encrypt without compression
> [!IMPORTANT]
> Some games skip the compression stage, so you need to use the `-nc` flag for their save states.

```bash
.\heavenly-bodies-save-deencryptor-cli.exe -m e -p ".\InputDirectory" -nc
```

> [!NOTE]
> Modified files are being placed in a newly created folder within the ***"HeavenlyBodiesSaveDeencryptor/_OUTPUT/"*** folder.

# :fire: Issues
All the problems I've encountered during my tests have been fixed on the go. If you find any other issues (which I hope you won't) feel free to report them [there](https://github.com/mi5hmash/HeavenlyBodiesSaveDeencryptor/issues).