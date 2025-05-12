# hudson_solo_module

Contains `hudson_solo_module`, providing an interface and adapter that works alongside Hudson's SOLOSOFT application to automate a Hudson Solo Liquidhandler.

## Installation Notes

- After cloning the repo and installing SOLOSOFT, open up the Visual Studio Solution in Visual Studio 2022 or later.
- Go to `View -> Other Windows -> Package Manager Console`
- Paste or type in `Update-Package -reinstall` in the console to install the appropriate NuGet dependencies on the local machine

## REST Server Accessibility

To make the server accessible, run the following in a terminal as Administrator

```
netsh http add urlacl url=http://+:2005/ user=<USER> listen=yes delegate=yes
```

Replace `2005` with the port you intend to use, if it differs from the default, and `<USER>` with the username that will be running the server (you may need to use the form `DOMAIN/USER`)

## Firewall

To interface with the module from another device, you'll need to [open up the port](https://www.windowscentral.com/how-open-port-windows-firewall) you intend to run the module's server on (2005 by default).
On most Windows devices, all incoming ports are blocked by the firewall by default.
