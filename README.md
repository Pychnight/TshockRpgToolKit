# TShockRpgToolKit

TShockRpgToolKit is a collection of plugins and tools for the TShock Terraria server. They customize the stock Terraria experience with role playing game elements, like classes and levels( among others ).

## Building

### Prerequisites:

Its recommended you have the following available( though other configurations may work ).

Windows:

* .NET Framework 4.5.2 or better.
* Visual Studio 2017 Community Edition
* TShock 4.3.24

Linux:

* Mono 5.x
* MonoDevelop or Visual Studio Code
* TShock 4.3.24

Building and testing on Linux is still experimental, mainly due to issues with MonoDevelop and limitations with Visual Studio Code.

### Grab the code

```bash
git clone --recurse-submodules https://github.com/Pychnight/TshockRpgToolKit
```

### Build

```bash
cd TshockRpgToolKit
msbuild 
```

This will build debug binaries, with each plugin being output to the projects bin/tshock_xxx/ServerPlugins folder. Note that currently, you'll need to manually unpack tshock for each location( so that it parents the "ServerPlugins" folder. ). You may  also need to restore nuget packages first. 

This works fine for testing individual plugins in isolation, but many times you'll want all plugins to be built in the same place. So for this scenario you can also override the path to the tshock folder by passing in an msbuild property, or via an environment variable.

MSBuild Property

```bash
msbuild /p:RpgToolsServerPath="some/other/location"
```

It is recommended to set a persistent env variable however.

### Run

Once the plugins are available in the tshock ServerPlugins folder, you can start the server as normal.

On first run, the plugins will create their directorys, each containing plugin specific configuration and data. Note that some plugins will require additional configuration in order to operate correctly. 
