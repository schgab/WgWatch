# What is WgWatch
WgWatch is a service which monitors and limits interfaces' traffic usage on a mikrotik router.

## Prerequisites
Run the following commands on your Mikrotik Router to enable the rest api. WgWatch is designed to allow self signed certificates.
```
/certificate
add name=root-cert common-name=MikrotikRouter days-valid=3650 key-usage=key-cert-sign,crl-sign
sign root-cert
add name=https-cert common-name=MikrotikRouter days-valid=3650
sign ca=root-cert https-cert

/ip service
set www-ssl certificate=https-cert disabled=no
set www disabled=yes
```
## Building
To build this project
1. `git clone https://github.com/schgab/WgWatch.git && cd WgWatch`
2. `dotnet publish -c Release -o publish --self-contained WgWatch`

This outputs the application to the folder `publish/`

Note: Everything in the publish folder is needed

## Before running
You need to open `publish/appsettings.json` and change the value of the key `ConfigFile` to the path of your config file. The default location is `/etc/wgwatch/config.json`.  
This repository contains an example unit file for systemd and an example config file.

## Configuration
The main configuration is the following:
- `intervalinminutes` - every x minutes a request is send to the mikrotik to update data usage
- `endpoint` - the url of the Mikrotik in the form of 'https://{yourdomain or your ip}/rest/'
- `user` - your mikrotik username (needs privileges to turn off and on interfaces)
- `password` - the password
- `interfaces` - list of interfaces to be monitored

An interface has the following options:
- `name` - has to match the name of the interface on the Mikrotik device
- `quota` - the amount of traffic (in GiB) the interface is allowed to use in the specified time period
- `period` - the time period (in whole days). After the period the traffic usage is reset
- `action` - takes one of three possible values 
  - `none` - nothing happens when the quota is exceeded
  - `shut` - the interface gets disabled when quota is exceeded
  - `auto` - same as shut, but the interface gets enabled after new period starts

