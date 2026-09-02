# RimWorld Compatibility Fixes

A private source archive containing four small RimWorld 1.6 compatibility and behavior fixes:

- **MoeLotl: Rigor Mortis - Float Menu Fix** — reduces repeated expensive float-menu checks and preloads affected incantation graphics. [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3783214874)
- **Milira: Wings of Democracy - Float Menu Fix** — filters unrelated WorkGiver checks, removes duplicate menu work, and caches repeated storage searches. [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3783936979)
- **Milira Race - Float Menu Fix** — avoids unnecessary solar-fuel WorkGiver scans and repeated storage searches. [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3783931796)
- **MoeLotl Settlement Trade Faction Fix** — assigns traded MoeLotls to the correct faction and includes optional Real Faction Guest compatibility. [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3783743602)

Each subdirectory is a standalone RimWorld mod. See its `About/About.xml` for dependencies, load order, and full details.

## Workstation workflow

On the current workstation, the corresponding directories under `G:\SteamLibrary\steamapps\common\RimWorld\Mods` are the authoritative development, build-output, deployment, and game-test copies. This repository is the backup and Git/GitHub synchronization mirror. For a backup or release, compare and copy the intended files from each G-drive Mod directory into its matching subdirectory here, verify relative paths and hashes, inspect the complete repository diff, then commit and push. Do not develop in this mirror first, and never treat Steam Workshop copies as authoritative source.
