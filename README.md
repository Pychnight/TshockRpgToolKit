# TShockRpgToolkit

TShockRpgToolkit is a collection of plugins and tools for the TShock Terraria server. They customize the stock Terraria experience with role playing game elements, like classes and levels( among others ).

## Building

### Prerequisites:

Its recommended you have the following available( though other configurations may work ).

Windows:

* .NET Framework 4.5.2 or better.
* Visual Studio 2017 Community Edition ( or Visual Studio Build Tools 2017, for commandline builds  )
* TShock 4.3.24

Linux:

* Mono 5.x
* MonoDevelop or Visual Studio Code
* TShock 4.3.24

Building and testing on Linux is still experimental, mainly due to issues with MonoDevelop and limitations with Visual Studio Code.

Note: .NET Core is not supported at this time, due to missing API's required by Boo.

### Grab the source

```bash
git clone https://github.com/Pychnight/TshockRpgToolKit
```

### Development build

```bash
cd TshockRpgToolKit
msbuild 
```

Newer versions of msbuild should download necessary packages, but if not, make sure you have nuget installed and on your path. Then run `nuget restore`. 

This will build debug binaries, with each plugin being output to `Servers/tshock_4.3.24/ServerPlugins`. Note that you'll need to manually unzip tshock over this location, the build does not do it for you.
Any sub project with the `UseToolsFolder` property set, will be output to the RpgTools directory, and not ServerPlugins.

You may override the path to the tshock folder by passing in an msbuild property, or via an environment variable.

Using an MSBuild Property:

```bash
msbuild /p:RpgToolsServerPath="some/other/location"
```

It is recommended to set a persistent env variable however.

### To generate a release zip

Note: Packaged builds can only be generated on Windows currently. The before and after steps are not executing, and at first glance this appears to be an msbuild and/or mono thing. ( Needs investigation ) 

You *MUST* run msbuild from the commandline, and set configuration property to `Deploy` in order to automatically package the binaries as a zip.:

```bash
msbuild /p:Configuration="Deploy"
```

You can build the `Deploy` configuration within Visual Studio, but the packaging process will not run. Release binaries will still be output to `bin/TShockRPGToolkit`.

### Build issues

If building on the commandline under windows, and you get an error like "GetReferenceNearestTargetFrameworkTask" task was not found, run the Visual Studio Build Tools 2017 installer, and ensure that "Nuget targets and build tasks" are ticked. See https://developercommunity.visualstudio.com/content/problem/160494/msbuild-broken-getreferencenearesttargetframeworkt.html

### Run

Once the plugins are available in the tshock ServerPlugins folder, you can start the server as normal.

On first run, the plugins will create their directories, each containing plugin specific configuration and data. Note that some plugins will require additional configuration in order to operate correctly.

## Credits

Rpg Toolkit is a premium plugin created by MarioE for the original base Plugins and Further work done by hired freelancers.

Design documents and Testing done by Pychnight.

## Legal

IF you use RPG Toolkit for profit on your own server for selling items, armor, monsters or other features this is at your own risk.
you agree that usage of the plugins are at your own risk, I'm not responsible for any damages it may cause.

All continued work on this plugin must follow rules of conduct and be submited back to this repo for merger. upon review each improvement and bug fix will result in a issue closed Please tag correct issues when submiting a change or bugfix.

## Rules of Conduct

- Code commited must be in working condition.
- All code must be well documented.
- User End documention must be upaded to match plugin usage and features.
- Must obey all Tshock policies regarding plugin standards.
