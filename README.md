## More Saves

### Automatic Saves
This mod saves along your play automatically and creates savefiles you can load from the modmenu.
The automated saves are limited to one per map and reflect the state of the last time you have played in that map.
Should you choose to "Give up" deleting the save that Jump King creates so will the automatic save be deleted.

### Manual Saves
Manual saves can be made from the pausemenu and will have the name of the map as well as the timestamp.
It is recommended to rename the folder of the manual save to a more descriptive name.

### Alternatively
Copy out these files to save your playtrough w/o the mod:
- Saves/combined.sav
- SavesPerma/general_settings.set
- SavesPerma/event_flags.set
- SavesPerma/attempt_stats.stat
- SavesPerma/perma_player_stats.stat
- SavesPerma/inventory.inv

[Mod on Steam](https://steamcommunity.com/sharedfiles/filedetails/?id=3239040787)

---
Notes I made that might be useful to have

(set) refers to saveName being set
```
- When what saves -
general_settings.set / not encrypted
option selections.
equipped items.
-> Saves a few times on game start, before [OnLevelStart]
-> Saves when loading into a map, before [OnLevelStart]
-> Saves when you exit a map, after [OnLevelEnd]
-> Saves when you change equipped item (set)

event_flags.set / encrypted
loading into the correct map BUT not the information of what map itself.
-> Saves when you select a new game (point of no return, like sacrificing crown etc.), before [OnLevelStart]

attempt_stats.stat / encrypted
stats you started the run with.
BUT also the actual id of the map loaded, or null if vanilla at which point event flags make the final distinction.
requires all time stats as is just subtracts the stats you started with from all time.
-> Saves when you select a new game (point of no return, like sacrificing crown etc.), before [OnLevelStart]

perma_player_stats.stat / encrypted
all time stats.
-> Saves when you select a new game (point of no return, like sacrificing crown etc.), before [OnLevelStart]
-> Saves when you exit a level, before [OnLevelEnd] (set)

combined.sav / encrypted
position, velocity, in world item/npc states.
-> Saves constantly while in level, after [OnLevelStart] (set)

inventory.inv /  encrypted
the inventory :)
-> Saves when inventory changes (set)
```
