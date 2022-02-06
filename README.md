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

| Filename			 | Description									|
|--------------------|----------------------------------------------|
| icon.png			 | Icon for mod that will show up in mod loader	|
| config.xml		 | Information about mod						|
| <filename\>.packed | Container with all mod game assets			|

### meta.ini sample
```xml
<?xml version="1.0" encoding="UTF-8"?>
<ScrapMod>
	<Title>Mod Title</Title>
	<Description>Mod Desciption</Description>

	<Category>Category</Category>
	<Version>1.0</Version>
	<RequiredLauncher>1.0</RequiredLauncher>
	<RequiredGame>1.1</RequiredGame>
	
	<Author name="Author1" website="https://example.com" />
	<Author name="Author2" />
	
	<Credits group="Mod author">
		<Credit name="Author1" />
	</Credits>
	<Credits group="Some credit" >
		<Credit name="Credit1" />
		<Credit name="Credit2" />
		<Credit name="Credit3" />
	</Credits>
</ScrapMod>
```

## TODO:

 - [X] Support for custom *.packed
 - [X] Supoprt for Scrapland Remastered
 - [ ] Support for custom game files (i.e. `\Traslation\` files or custom `QuickConsole.py`)
 - [ ] Recompiling *.py to *.pyc
 - [ ] Mod settings.
 - [ ] More meta info in `config.xml`
 - [ ] Multilanguage support
 - [ ] More mods :wink: