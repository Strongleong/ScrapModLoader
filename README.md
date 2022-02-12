WIP ScrapModLoader
==============

This applications is for managing mods for Scrapland.

**WARNING! THIS PROJECT IS IN DEVELOPMENT. ALL WRITTEN IN THIS DOCUMENT IS SUBJECT TO CHANGE**


## How to use

ScrapModLoader after first start will create `Scrapland Mods` folder in User's `Document` folder
and scans yours installed programs for any Scrapland instalation. If it could not find any you will be promt with error message.

You can specify Scrapland installation location in settings.

By default this app will search for mods in `Scrapland Mods` in User's `Document` folder, 
and you can put all your mods in there, or add any folder you want in 
settings and app will search for mods there.

ScrapModLoader supports both original and remastered versions of Scrapland.


## How To Make Mod

For now mod for Scrapland is a *.sm file that basically is a zip arhive with following content:

| Filename				 | Description										|
|------------------------|--------------------------------------------------|
| icon.png				 | Icon for mod that will show up in mod loader		|
| config.toml			 | Information about mod							|
| <game_version\>\		 | Folder that named as game version  mod made for	|
| <filename\>.packed	 | Container with all mod game assets				|

You can have as many .packed files as you want. Mod loader will load everything.

.packed files in the root of mod will be copied to the `Mods` folder of Scrapland. 

.pakced files under game version folder will load only to the appopriate game version.  

### .sm structure sample
```
│  icon.png
│  config.toml
│  mod_assets.packed
├──1.0/
│    only_for_original.packed
└──1.1/
     only_for_remastered.packed
```

### config.toml sample
```toml
title = "Mod title"
description = "Mod description"
category = "Mod category"
version = "1.0"
requiredLauncher = "1.0"
supportedGameVersions = ["1.0", "1.1"]

authors = [ 
	{ name = "Author 1" },
	{ name = "Author 2" }
]

[[credits]]
group = "Group 1"
credits = [
	{ name = "Author 1" },
	{ name = "Author 2" },
	{ name = "Author 3" }
]

[[credits]]
group = "Group 2"
credits = [
	{ name = "Author 3" },
	{ name = "Author 4" }
]
```

## TODO:

 - [X] Support for custom *.packed
 - [X] Supoprt for Scrapland Remastered
 - [ ] Support for both Scrapland versions in single .sm file
 - [ ] Support for custom game files (i.e. `\Traslation\` files or custom `QuickConsole.py`)
 - [ ] Recompiling *.py to *.pyc
 - [ ] Mod settings.
 - [ ] More meta info in `config.toml`
 - [ ] Multilanguage support
 - [ ] More mods :wink: