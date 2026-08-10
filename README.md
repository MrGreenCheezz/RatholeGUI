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
- **Pick a port from a running application** instead of typing it by hand (see below).
- Dark themed UI: custom-drawn controls, a resizable window and per-service validation shown inline.

## Adding a service from a running application

Typing `127.0.0.1:25565` by hand means knowing the port in the first place. Two buttons do it for you:

- **Add from running app** (services header) — creates a whole service from a port you pick;
- **From app** (inside a service card) — fills only the client address and port of that service.

Both open a picker that lists every application currently listening on this PC, together with its
process name, PID, protocol, bind address and a hint about what the port usually is
(`MySQL`, `Minecraft Java`, `RDP`, …). The list is read straight from Windows via
`GetExtendedTcpTable` / `GetExtendedUdpTable`, so it does not probe the network and needs no
elevation.

Picking an entry fills in:

- the client address — `127.0.0.1` when the application listens on `0.0.0.0`, otherwise the exact
  address it is bound to;
- the client port, and the same port on the server side when it is still free;
- a service name derived from the process (`minecraft-server-25565`);
- a freshly generated random token.

UDP is hidden by default (browsers and Windows services open dozens of short-lived UDP sockets);
tick **UDP** in the picker when you need to forward one. **Hide system processes** filters out
`svchost` and friends.

> The scan looks at the machine running RatholeGUI. When the Rathole *client* lives on another host,
> that host's ports are not listed — fill those in manually.

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
- Unhandled exceptions are appended to `ratholegui-error.log` next to the working directory instead
  of closing the app silently.
- Icons come from the `Segoe MDL2 Assets` font shipped with Windows 10/11; the UI simply drops the
  glyphs if it is missing.
