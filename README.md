ScrapModLoader
==============

This applications is for managing mods for Scrapland.

## How To Make Mod

Mod for Scrapland is a *.sm file that basically is a zip arhive with following content:

| Filename    | Description										|
|-------------|-------------------------------------------------|
| icon.png    | Icon for mod that will show up in mod loader	|
| meta.ini    | Information about mod							|
| data.packed | Container with all mod game assets				|

### meta.ini structure
```ini
[Miscellaneous]
Title="Mod Title"
Description="A mod description"

Category=A category
Version=1.0
RequiredLauncher=1.0
RequiredGame=1.1

AuthorGroup=Group 1
AuthorGroup=Group 2

[Author]
Name=Mod author
Website=http://example.com

[Author]
Group=Group 1
Name=Mod author

[Author]
Group=Group 2
Name=Another author

[Setting]
Name=setting
Title=Setting titile
Tooltip=Some setting
Type=Int
Min=1
Max=10
Default=2
File=scripts/char/ditritus.py
Placeholder=<speed>
```