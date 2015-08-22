# SteamWorkshopUploader
A generic, bare-bones app made in Unity3D for letting players upload mods to Steam Workshop for your game.

Right now everything included will work as-is for the game "Sky Rogue" (app id 381020), but due to the way the Steam API works, it should 

### Setup

Edit "steam_appid.txt" to match your game's app id.

Edit "config.json" to match your workshop settings. For example, if you use predefined tags, be sure to define them and set "validateTags" to true.

### Distributing to modders

Be sure to include these files in builds you distribute:

config.json
steam_appid.txt
steam_api.dll (Steamworks.NET should do this automatically)
WorkshopContent/
Optionally: an example mod or two in WorkshopContent/ so modders can understand the format of .workshop.json files.
