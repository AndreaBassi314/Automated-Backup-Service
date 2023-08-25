# Automated Backup Service

Welcome to the Automated Backup Service repository! This project is designed to provide an automated backup solution for your important files and folders.

## Table of Contents
- [Introduction](#introduction)
- [Features](#features)
- [Files](#files)
  - [Program.cs](#programcs)
  - [Backup.cs](#backupcs)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
- [Usage](#usage)
- [Configuration](#configuration)
- [Contributing](#contributing)
- [License](#license)

## Introduction

The Automated Backup Service is a C# application that allows you to create automated backups of your files and folders at specific intervals. It provides a user-friendly command-line interface for configuration and control.

## Features

- Create backups with different frequencies: hourly, daily, or weekly.
- Easy setup process for initial configuration.

## Files

### Backup.cs

The `Backup.cs` file contains the implementation of the `Backup` class, which handles the backup process. It compares files in the main folder with those in the backup folder and performs necessary actions.

### Program.cs

The `Program.cs` file contains the implementation of the `Program` class, which serves as the entry point of the application. It handles user interaction, setup, and menu options.

## Getting Started

### Prerequisites

- .NET Core SDK (version 5.0 or later)
- Windows operating system (for Windows Task Scheduler usage)

### Installation

1. Clone this repository to your local machine.
2. Open a terminal or command prompt and navigate to the repository's directory.
3. Run the application using the following command:

```
dotnet run
```

## Usage

1. Follow the prompts to set up your backup configuration.
2. The application will create an automated task in Windows Task Scheduler for periodic backups.
3. The backups will be stored in the specified backup folder.
4. Use the menu options to force a backup or delete the backup process.

## Configuration

You can configure the backup settings by editing the `config.txt` file. The file contains information about backup frequency, main folder, backup folder, and log file path.

## Contributing

Contributions are welcome! If you find a bug or have an improvement idea, please open an issue or submit a pull request.

## License

This project is licensed under the MIT License.
