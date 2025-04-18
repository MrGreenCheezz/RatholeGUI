# RatholeGUI

**RatholeGUI** is a lightweight application designed to automate the configuration and deployment of [Rathole](https://github.com/rapiz1/rathole) reverse proxy services across a pair of Linux servers.

## Overview

RatholeGUI simplifies the process of:
- Generating `.toml` configuration files for Rathole,
- Uploading these configuration files to remote Linux servers via SSH,
- Running Rathole on both server and client machines.

This tool is especially useful for setting up reverse proxy tunnels quickly without manually editing config files or running commands.

## Features

- Automatically generates Rathole configuration files based on predefined templates.
- Supports secure upload of configs via SSH to both server and client machines.
- Provides control over the execution of Rathole remotely.
- Saves all data in `data.json` , which stores:
  - Server and client IP addresses for SSH access,
  - SSH ports,
  - Paths to the Rathole binary on each machine,
  - Any other relevant connection/configuration details.

## Example Generated Config

```toml
[server]
bind_addr = "0.0.0.0:2333"

[server.transport]
type = "tcp"

[server.transport.tcp]
nodelay = true
keepalive_secs = 20
keepalive_interval = 8

