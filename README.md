# SteamWorkshopUploader
A generic, bare-bones app made in Unity3D for letting players upload mods to Steam Workshop for your game.

Right now everything included will work as-is for the game "Sky Rogue" (app id 381020), but due to the way the Steam API works, it should work just fine on any other game simply by changing the app id. I've even tested it myself with Skyrim.

Thanks goes to [rlabrecque](https://github.com/rlabrecque), none of this would work without [Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET).

### Setup

Edit "steam_appid.txt" to match your game's app id.

Edit "config.json" to match your workshop settings. For example, if you use predefined tags, be sure to define them and set "validateTags" to true.

### Distributing to modders

Be sure to include these files in builds you distribute:

- config.json
- steam_appid.txt
- steam_api.dll (Steamworks.NET should do this automatically)
- WorkshopContent/
- Optionally: an example mod or two in WorkshopContent/ so modders can understand the format of .workshop.json files.

### KNOWN ISSUES

Sometimes it seems that users are unable to upload things and get an "invalid item" error. Sometimes they're able to resolve it by doing seemingly random things like choosing a different preview image or making other seemingly random changes to the content itself. Usually, if another user uploads the same content as-is there are no problems, so I'm unable to verify if the issue is with SteamWorkshopUploader or Steam itself. I've been talking to Valve about this but have been unable to determine a cause or potential fixes.

If you use this uploader for your own game and find your players run into this same issue, but are able to figure out some solution or workaround, PLEASE get in contact or create a pull request, it would be greatly appreciated.
