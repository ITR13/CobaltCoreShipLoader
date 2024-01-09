# Installing modloader
Download [Nickel.zip] and extract it somewhere, *preferably* your game's directory.  
Inside the downloaded zip, you'll find the Nickel folder. Extract this folder, preferably within your game's directory.  
![The game's folder open with a folder named "Nickel" highlighted](https://raw.githubusercontent.com/ITR13/CobaltCoreShipLoader/main/.readme/nickel_zip.png)  
Make sure it's *extracted* and a **folder** like in the previous image. That it doesn't end with ".zip"  
Open the Nickel folder and run "Nickel.exe".  
![Folder is sorted with type ascending, run Nickel.exe](https://raw.githubusercontent.com/ITR13/CobaltCoreShipLoader/main/.readme/nickel_exe.png)  
If the game loads without any errors the modloader is working.  

## Transferring your save file from the vanilla game
If you're on windows press windows+r and type in "%AppData%/CobaltCore" and hit run. This will open your vanilla save folder. For other platforms, please tell me so I can update the guide.  
![Folder with Slot0, Slot1, Settings.json, and steam_autocloud.vdf in it](https://raw.githubusercontent.com/ITR13/CobaltCoreShipLoader/main/.readme/save_folder.png)  
Open your Nickel folder from before, and find the "ModSaves" folder. If you haven't run Nickel.exe yet then you need to create this folder yourself.  
![Folder with ModSaves marked](https://raw.githubusercontent.com/ITR13/CobaltCoreShipLoader/main/.readme/nickel_saves.png)  
Copy all the files in here except steam_autocloud.vdf into this folder. Make sure to replace any files already in this folder.  

## Downloading and Installing Mods
Find the mod you want to install, and find their zip file. As an example we'll be using [ITRsShiploader.zip](https://github.com/ITR13/CobaltCoreShipLoader/releases/download/2.0.0/ITRsShipLoader.zip) and [SampleShips.zip](https://github.com/ITR13/CobaltCoreShipLoader/releases/download/2.0.0/SampleShips.zip)  
Open your Nickel folder and find the "ModLibrary" folder.
![Folder with ModSaves marked](https://raw.githubusercontent.com/ITR13/CobaltCoreShipLoader/main/.readme/nickel_saves.png)  
Place the entire downloaded zip files inside the "ModLibrary" folder within the Nickel directory. No need to extract the contents.  
![Folder with Essentials, Nickel.Legacy, ITRsShipLoader.zip, and SampleShips.zip in it](https://raw.githubusercontent.com/ITR13/CobaltCoreShipLoader/main/.readme/nickel_mods.png)  
If you did it correctly it will look something like the previous image. Now you just have to run Nickel.exe, and everything should be working!  

### About mods on Github
A lot of mods are currently hosted on GitHub. Make sure to not press the big DOWNLOAD button on the github repository. Instead navigate over to Releases.  
![Download ZIP crossed out and Releases highlighted](https://raw.githubusercontent.com/ITR13/CobaltCoreShipLoader/main/.readme/nickel_mods.png)  
If there are no Releases, try reading the mods readme to see where to download the zipfile. Or ask on the forum. If it's a work in progress mod you might have to compile it yourself, which this tutorial doesn't cover  .
Once you're on the releases page, make sure "Assets" is expanded on the topmost release, and download the topmost zipfolder, not one of the ones named "Source code".
![A foldable titled "Assets" expanded, with "CobaltCoreSeeded.zip" highlighted beneath it](https://raw.githubusercontent.com/ITR13/CobaltCoreShipLoader/main/.readme/nickel_mods.png)  
Some people host multiple mods on the same github file, so make sure the release title is for the mod you want!  
**For shipmods specifically** it might be fine to use the big "Download ZIP" button or to download the Source Code, but for other mods this won't work!  

### About errors
If the mod doesn't show up in your game, find Nickel/Logs/Nickel.log and read through it. Any errors will be reported there.  
If you're unable to figure out the issue and plan to ask for help on discord, make sure to upload Nickel.log with your message!  
