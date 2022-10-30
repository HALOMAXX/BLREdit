# BLREdit Features
## General:
* Profile Backup when the app starts it backups all Profiles to the backups folder which is Backup\dd-MM-yy\HH-mm\<profiles.json>.
* Checks for BLREdit updates and autopatches, if Assets folder is missing donwloads all Assets of current version.
* Donwloads ProxyModule list, contains all available modules which the user can install also contains info of how BLREdit should configure the loading of said modules if there is an error it can be updated from the github quite easily and no one has to update BLREdit.
* Saves Profiles OnAppExit if the app crashes you will lose Profile changes aswell as ClientList, ServerList and changed settings.
* BLREdit checks the registry for missing MSVC++ runtime for Proxy and BLR Client and prompts the user with a donwload link if missing.
* BLREdit Logs Many things if something goes wrong you can check the log.txt
* BLREdit shows Error messages which also get written to the log file("log.txt") if something went Very wrong which will change the behaviour of the App for example if it is unable to connect to github it will show a error message why it is unable to connect to github and the user will not be able to install Proxy Modules also BLREdit will be unable to Autoupdate.
* BLREdit has a Undo/Redo system which can be use with the shortcuts Ctrl+Z(Undo) and Ctrl+Y(Redo) currently only Profile Changes are affected by this.

## Profile Setup:
* PlayerName(TextBox) contains the Profile/PlayerName which get's used when launching the client you can create Multiple Profile with the same name.
* ProfileList(ComboBox) is a dropDown of all available Profiles, you can select a Profile from here to Edit/Use.
* Export Profile(Button) Exports the current Profile to the Default client Profile location to be used by Loadout-Manager Modules also copies the Profile into clipboard MagiCow's Style.
* Duplicate(Button) Duplicates the Current Profile and select's it.
* Randomize(Button) Randomizes the current Selected Loadout.
* General ItemSlot Behaviour LeftClick selects the ItemSlot and shows available items in the ItemList. RightClick Set's the ItemSlot to the Default item according to weapon setup or just default this behaviour is changed when using AdvancedModding here RightClick just removes the Item from the ItemSlot.
* Weapon(Tab) Here you can inspect stats and modify your Primary and Secondary Weapon with Muzzles, Barrels, Magazines, Scopes, Stocks, Recievers, Hangers, Camo, Ammo and Grips. you can also preview the Scope in the mini Preview and if you click it it shows a larger version in the Item List to better View the Preview.
* Gear(Tab) Here you can inspect and modify your Armor(Helmet, Chest and Legs), Tactical, Avatar, Gender, Camo, Throphy and your Grenades and other Gear.
* Extra(Tab) Here you can change your Depot Items and Taunts.

## ItemList
* Sorting. you can change the sorting Direction and by which stat the item list should sort. it also highlight's the stat you are sorting.
* Filter(TextBox) here you can search for item names very usefull for Helmets and Camos.
* Items Display a image, name and stats. the stats are getting highlighted according if they improve(green) a stat or decrease(red) a stat.

## Detailed Info
Shows too much info lol.

## Launcher:
* DNS Resolver Translates mooserver.ddns.net to IP address to be used when connecting to given server
* Server List Shows Server Info Like Map, Mode and Map Preview. below the server address it shows if the server is online. next to the connect button is a Playerlist DropDown which contains all currently connected PlayerNames. at the top right the button with the Refresh symbol Querries the server for new PlayerList and Map/Mode info. to the left the wrench button is to edit the current server it opens a new window, here you can modify the Server Address and Port. at the bottom of the screen is a button to add new servers.
* Client List contains all registered BLR Clients shows Client Version Number and Configuration Button at the top right the one with the wrench symbol, it opens a new window where the user can install, update and remove Proxy Modules. below is the Default Client Button which set the Client as the client to be used when connecting to Servers. further down are two buttons to Start a Server or Bot Match which pompt the user before launch for map, mode and Bot/Player Count in a new Window. at the bottom of the screen is a button to add a new BLR Client with just a file open prompt to locate the exe filters for *.exe. BLREdit on launch checks if steam is installed and tries to get the installed BLR client from steam to register in the client list if it is found and not already registerd it gets added and automagicaly patched the user still has to Install the modules manually.
