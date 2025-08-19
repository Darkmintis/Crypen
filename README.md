# 🔐 Crypen

Crypen is a simple, modern Windows application that encrypts and decrypts files, folders, and USB drives with strong password protection.

## Features

- **Strong Encryption**: Uses AES-256-GCM for secure, authenticated encryption
- **Password Security**: Implements Argon2 key derivation to prevent brute-force attacks
- **Windows Integration**: Right-click menu options for quick encrypt/decrypt actions
- **User-Friendly**: Clean, modern UI built with WinUI 3
- **File, Folder and USB Support**: Encrypt all types of content with one tool
- **History Tracking**: Keep track of all your encrypted items
- **Secure Password Storage**: Optional password saving using Windows Data Protection API
- **Open-Source & Local**: All encryption happens locally, and the code is fully open-source

## Getting Started

### Prerequisites

- Windows 10 version 1809 (build 17763) or later
- .NET 8.0 or later

### Installation

1. Download the latest installer from the [Releases](https://github.com/darkmintis/crypen/releases) page
2. Run the installer and follow the on-screen instructions
3. Crypen will be available from the Start menu and will integrate with Windows Explorer

### Building from Source

1. Clone this repository
2. Open `Crypen.sln` in Visual Studio 2022
3. Build and run the solution

## Usage

### Encrypting Files

1. Right-click on a file and select "Encrypt with Crypen"
2. Enter a strong password
3. The original file will be securely deleted, and a `.crypen` file will be created

### Encrypting Folders

1. Right-click on a folder and select "Encrypt with Crypen"
2. Enter a strong password
3. The original folder will be securely deleted, and a `.crypen` file will be created

### Encrypting USB Drives

1. Insert a USB drive
2. Right-click on the drive in File Explorer and select "Encrypt with Crypen"
3. Enter a strong password
4. The files on the drive will be encrypted in place

### Decrypting

1. Double-click on a `.crypen` file or right-click and select "Decrypt with Crypen"
2. Enter the password
3. The original content will be restored

## Security Details

- **Encryption**: AES-256-GCM (Galois/Counter Mode)
- **Key Derivation**: Argon2id with secure parameters
- **Data Integrity**: GCM authenticated encryption protects against tampering
- **Secure Deletion**: Multi-pass overwrite of original files
- **Password Storage**: Windows Data Protection API (DPAPI) for user-specific encryption

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- [Sodium.Core](https://github.com/tabrath/libsodium-core) - For cryptographic operations
- [Konscious.Security.Cryptography](https://github.com/kmaragon/Konscious.Security.Cryptography) - For Argon2 implementation
- [WinUI 3](https://github.com/microsoft/microsoft-ui-xaml) - For the modern UI
