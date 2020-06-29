# S4GFX [![GPLv3 License](https://img.shields.io/badge/License-GPL%20v3-yellow.svg)](https://opensource.org/licenses/GPL-3.0)
Import and export of "The Settlers IV" game files - Please credit us, when using this program
## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

### Prerequisites

```
- .Net Framework 4.6
- The Settlers 4
```

### Usage

This is a command line interface program (for now)

Place the executable with all dependencies in the main folder of the game (next to the game's exe)
```
...
Gfx/
Snd/
S4_Main.exe
**S4GFX.exe**
**S4GFXLibrary.dll**
...
```

#### Export
Just follow the on screen instructions to export the game files.

All files are saved to the `export/` folder next to the exe.

#### Import
To import files, place them according to the file structure, that this program provides when exporting a group.
Each game file has a unique ID inside a group file (.gfx) and belongs to a specific sub group (when paired with a .dil file). 

This program only looks for those exact file names at the needed path, so make sure to place your custom files accordingly.

Here is an example path for the torjan's lumberjack building: `export\GFX\14\1\2.png`
- `export` is the main folder of the program
- `GFX` is the type of file
- `14` is the file group (e.g. trojan buildings)
- `1` is the sub group (the lumberjack)
- `2.png` is the first file in the sub group of the lumberjack

## Authors
* **Wizzard Maker - Jonas Schoenwald** - *Main author/contributer* - [WizzardMaker](https://github.com/WizzardMaker/)
* **XanatosX** - *Interface and library work* - [XanatosX](https://github.com/XanatosX/)
* **nyfrk** - *Sound export* - [nyfrk](https://github.com/nyfrk)
## License

This project is licensed under the GPL-3.0 License - see the [LICENSE.md](LICENSE.MD) file for details

## Acknowledgments

* **Settlers.ts** - *Initial reverse engineering of some of the game files* - [Settlers.ts](https://github.com/tomsoftware/Settlers.ts)
* **The Settlers 4 Community Path** - [Settlers 4 CP](https://github.com/Settlers4Modding/Settlers4Patch/)
