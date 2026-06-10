# RatholeGUI

**RatholeGUI** is a lightweight Windows Forms application designed to automate the configuration and deployment of [Rathole](https://github.com/rapiz1/rathole) reverse proxy services across a pair of Linux servers.

## Overview

RatholeGUI simplifies the process of:

- Generating `.toml` configuration files for Rathole;
- Uploading these configuration files to remote Linux servers via SSH/SFTP;
- Running and stopping Rathole on both server and client machines;
- Checking whether Rathole is currently running.

This tool is especially useful for setting up reverse proxy tunnels quickly without manually editing config files or running commands.

## Features

- Automatically generates Rathole configuration files based on configured services.
- Supports secure upload of configs via SSH/SFTP to both server and client machines.
- Provides basic remote control over Rathole execution.
- Saves connection/configuration data in local `data.json`.

> Do not commit your real `data.json`: it can contain SSH addresses, usernames and passwords. Use `PortsAppGui/data.example.json` as a template.

## Build

```bash
dotnet build PortsAppGui/PortsAppGui.csproj
```

## Run

```bash
dotnet run --project PortsAppGui/PortsAppGui.csproj
```

You can also open `PortsAppGui/PortsAppGui.sln` in Visual Studio and run the WinForms app from there.

## Configuration

Create a local config from the example:

```text
PortsAppGui/data.example.json -> PortsAppGui/data.json
```

Then fill:

- `ServerAdress` / `ClientAdress` in `host:port` format;
- SSH usernames and passwords;
- Rathole directories on both machines;
- Local paths for generated client/server `.toml` files;
- Services that should be proxied.

## Example Generated Config

```toml
[server]
bind_addr = "0.0.0.0:2333"
heartbeat_interval = 20

[server.transport]
type = "tcp"

[server.transport.tcp]
nodelay = true
keepalive_secs = 20
keepalive_interval = 8

[server.services.example]
type = "tcp"
token = "change_me"
bind_addr = "0.0.0.0:8080"
nodelay = true
```

## Notes

- Keep `data.json`, `.toml`, `.vs`, `bin` and `obj` out of git.
- If credentials were accidentally committed, rotate/change them.
- The app currently uses the existing JSON field names like `Adress` for backward compatibility.
