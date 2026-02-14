# 🔐 Crypen

Modern Windows file encryption app with AES-256-GCM encryption and seamless Explorer integration.

## Features

- **Strong Encryption**: AES-256-GCM + Argon2id key derivation
- **Windows Integration**: Right-click context menu for files, folders, and USB drives
- **Modern UI**: Clean WinUI 3 interface with keyboard shortcuts
- **Secure Storage**: Password management with Windows DPAPI
- **Bulk Operations**: Encrypt multiple files with progress tracking
- **History**: SQLite database tracks all encryption operations

## Installation

**Requirements:**
- Windows 10 (1809+) or Windows 11
- .NET 8.0 Runtime

**Build from Source:**
1. Open `Crypen.sln` in Visual Studio 2022
2. Build and run

## Usage

**Encrypt:**
- Right-click any file/folder → "Encrypt with Crypen" → Enter password
- Or press `Ctrl+O` in the app to select files

**Decrypt:**
- Double-click `.crypen` file → Enter password
- Or right-click → "Decrypt with Crypen"

**Keyboard Shortcuts:**
- `Ctrl+O` - Open file to encrypt
- `Ctrl+H` - View history
- `Ctrl+,` - Settings
- `F5` - Refresh

## Security

**Encryption:** AES-256-GCM (authenticated encryption)  
**Key Derivation:** Argon2id (64MB memory, 4 iterations)  
**Secure Deletion:** 3-pass overwrite of original files  
**Password Storage:** Windows DPAPI

## License

MIT License

## Tech Stack

- WinUI 3 / .NET 8.0
- [Konscious.Security.Cryptography](https://github.com/kmaragon/Konscious.Security.Cryptography) - Argon2
- [Sodium.Core](https://github.com/tabrath/libsodium-core) - Cryptographic operations
- Microsoft.Data.Sqlite - History tracking
