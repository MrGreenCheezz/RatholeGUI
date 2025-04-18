# RatholeGUI
App that allows you to generate, send and run configs of Rathole reverse proxy https://github.com/rapiz1/rathole


Small app to automate some actions with rathole reverse proxy. App designed to generate and upload .toml config files to pair of Linux servers with installed rathole app.
First launch will generate data.json file, fill it with data ( server and client ip adress used for ssh connection, ports, paths).
For now app generates config with
[server]
bind_addr = "0.0.0.0:2333"
