# ITR'S COBALT CORE SHIP LOADER
This shiploader allows you to use and create ships without any coding expertise. These mods, dubbed in the text below as "ShipMods", differs from regular Cobalt Core mods by only needing a .startership file or a zip file to function. If you're unsure if a mod is a shipmod or not- if the mod files contains a .dll file it's not a shipmod, if it contains a .startership file it **is** a shipmod!

## How to install
1. Install Ewanderer's [Cobalt Core Mod Loader](https://github.com/Ewanderer/CobaltCoreModLoader)  
2. Go to [Releases](https://github.com/ITR13/CobaltCoreShipLoader/releases) and expand "Assets" on the latest release.  
3. Download ITRsShipLoader.zip and extract it.
4. Run the modloader and add ITRsShipLoader.dll assembly. Alternatively use the scan for mods function.    
NB: You can see 

### Installing additional ShipMods
5. Make sure you have ran the game at least once.
6. Go to the game's install location, you can find this by going right clicking Cobalt Core in your steam library, selecting "Manage" then "Browse local files"  
![Image of the steps explained above](https://raw.githubusercontent.com/ITR13/CobaltCoreShipLoader/.readme/gamepath.png)
7. There should be a ShipMods folder there, put any ShipMods into this folder and restart the game!

## How to use
The mod comes with a few sampleships to try out, to see them, simply start a new run and cycle to the additional ships.  
![Begin TimeLoop screen with the ship "Crystal Escort" selected](https://raw.githubusercontent.com/ITR13/CobaltCoreShipLoader/.readme/shipselect.png)  
If you install any other ShipMods, they will appear here too!

## Creating a ShipMod

## Tutorial
### Simplest ship mod

- Create a folder named "Test" in your ShipMods folder
- Create a file named "test.startership" in it
  - NB: make sure it has the "startership" filetype, not .txt
- Open it in a text editor and paste the following:
```json
{
  "__meta": {
    "Name": "TestShip",
    "Author": "",
    "Description": "This is a test ship!",
    "RequiredMods": [],
    "ExtraLocalization": {}
  },
  "ship": {
    "parts": [{
        "type": "wing",
        "skin": "wing_player",
        "flip": false,
      }, {
        "type": "missiles",
        "skin": "missiles_artemis",
        "flip": false,
      }, {
        "type": "cannon",
        "skin": "cannon_artemis",
        "flip": false,
      }, {
        "type": "cockpit",
        "skin": "cockpit_artemis",
        "flip": false,
      }, {
        "type": "wing",
        "skin": "wing_player",
        "flip": true,
      }
    ],
  },
  "artifacts": [{
      "$type": "ShieldPrep, CobaltCore",
    }, {
      "$type": "CargoHold, CobaltCore",
    }
  ],
  "cards": []
}
```  
- Restart the game and you should see the following ship:  
![Begin TimeLoop screen with the ship "Test Ship" selected](https://raw.githubusercontent.com/ITR13/CobaltCoreShipLoader/.readme/testship.png)  

### Custom Sprites
- Go out of the ShipMods folder, and find the "Data" folder in the game's root directory
- Enter Data/sprites/parts and find a cool looking part
- Copy the part into your "ShipMods/Test" folder, and edit the image a little. Rename the file to "custom.png"
- Inside parts, change one of hte "skin" fields to "@@Test/custom"
  - NB: "Test" should correspond to your foldername, if you rename the test folder, rename it here too
- Start the game, and you should now see your sprite in game  

### Exporting
To export your mod, simply zip the folder you made and send it to your friend!
Your friend can put the entire zip file into their ShipMods folder and it will still work!

## Other Info
### Fields
#### Meta
The meta object has the following fields:
- Name: The name of the ship
- Author: The creator of the ship, put your name here!
- Description: The english description of the ship
- Required Mods: Put info about required mods here? Currently unused
- ExtraLocalization: Name and description in other languages, check one of the sample ships to see how it's used

#### Ship
The ship object has the following fields:
- baseEnergy: How much energy the ship has
- baseDraw: How much you draw per turn
- evadeMax: The maximum amount of evade the ship can have
- hpGainFromEliteKills: How much hp you gain from killing an Elite
- hpGainFromBossKills: How much hp you gain from killing a Boss
- chassisUnder: Sprite to draw under the ship
- chassisOver: Sprite to draw on top of the ship
- hull: Starting health
- hullMax: Starting Maximum Health
- shieldMaxBase: Starting Maximum Shield
- parts: A list of parts with the following parameters
  - type: The type of part, can be "cockpit", "cannon", "missiles", "wing", "empty", "comms", or "special"
  - skin: Sprite to draw
  - flip: Mirrors the sprite if true
  - invincible: If the part can take damage
  - damageModifier: can be "none", "weak", "armor", "brittle"  

#### Artifacts
The artifact object is a list of objects with a "$type" field, corresponding to their internal class name + ", CobaltCore". You can either look at your profile to find these, or decompile CobaltCore.exe.
These are given to the ship when you start a run.

#### Cards
The card object is a list of objects with a "$type" field, corresponding to their internal class name + ", CobaltCore". You can either look at your profile to find these, or decompile CobaltCore.exe.
These are added to your deck when you start a run.

### Random sprite stufff
If you make subfolders, these will be part of the sprite name. For example, "ShipMods/Test/Test2/cat.png" will have the name "@@Test/Test2/cat".  
You can use the sprites from other shipmods if you know their paths.  
You can use sprites from regular mods if you know what they register them as, and append it with "@mod_part:" or "@mod_extra_part:"

