# Black-Ops-Zombies-GSC-Injector
This is a program that can be used to inject custom GSC files to Call of Duty: Black Ops Zombies. To use this program, you must be on patch 1.13 and have CFW installed on your PlayStation 3.
## Getting Started
### Preventing Game Freezes
Due to the nature of the game and how this program works, the script info in game memory must be reset everytime you exit a zombies match. To do this, look at the program's parameter usage. It may be possible to skip this step by using a different game script to run the injected main.gsc, but I have yet to find it.
### Creating a Project Directory
Injected custom GSCs using this program requires a project directory with a main.gsc containing an `init()` function file in its root. An example of an acceptable project directory would be:
```
C:\Zombies Project Folder
  - \huds\
    - hudutils.gsc
    - hud tests.gsc
    - hud_examples.gsc
  - \gamemodes\
    - oic.gsc
    - ffa.gsc
    - tdm.gsc
  - main.gsc
```
Folder and file names do not matter as long as there is a main.gsc file containing an `init()` function in the root of the project directory.
### Writing Project Code
GSC projects are written in GSC code, the scripting language of the Call of Duty engine. An example of a main.gsc that prints some text at player spawn would be:
```c
#include common_scripts\utility;
#include maps\_utility;
#include maps\_hud_util;

init()
{
	level thread onPlayerConnect();
}

onPlayerConnect()
{
	for(;;)
	{
		level waittill( "connected", player );

		player thread onPlayerSpawned();
	}
}

onPlayerSpawned()
{
	for(;;)
	{
		self waittill("spawned_player");

		//Player has finished spawning
		self iprintln("Hello world!"); //iprintln prints text to the top left of the screen
	}
}
```
Any additional project files should not have `#include` lines due to the way that this program operates.
### Injecting a Project
Injecting a project using this program can either be done by executing the exe with a single parameter.
#### Parameters
`r OR reset - Resets the game's rawfile table in memory`
`api OR change-api - Changes the current working API between Target Manager API and Control Console API`
## How It Works
This program works by doing the following:
1. Make a list of files to be injected by scanning input project path
2. Put the main.gsc at the top of that list
3. Iterate through each file in the list and combine it into a single string which will be a modified `maps\_dev.gsc`
4. Injected a modified `maps\_cheat.gsc` that calls the `init()` function in a modified `maps\_dev.gsc`
5. Update the script file information in game memory to accomodate for the changes of the modified `maps\_cheat.gsc` and `maps\_dev.gsc`
