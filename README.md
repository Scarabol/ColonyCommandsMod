# Colony Commands Mod by Scarabol

**Special thanks to Tjohei for most of the ideas and feature requests.**

**Special thanks to Crone for the chat coloring system and score system.**

## Anti-Grief

This mods adds an anti-griefing mechanism to the server.
Regular players can't change 50 blocks around the spawn point and 50 blocks around other player banners.
Both values can be adjusted.
Furthermore players are auto-**kicked**, if they kill more than 10 foreign colonists and even auto-**banned** if they kill more than 50 foreign colonists.
Both values can be configured in the savegame folder.

Players are also able to whitelist friends by writing **/antigrief banner [friendly-player-name]** in chat. 

Last but not least players with master permission **mods.scarabol.antigrief** are able to define custom protected areas.
Use **/antigrief area 10 10** to restrict regular players from changing blocks in an area 10 blocks around your current position.

To **disable anti-grief** in case you play with friends only, just set the protection ranges in the *protection-ranges.json* file in your savegame folder to *-1* each. The message on joining the server will still show up, but it has no effect then.

## Activity-Tracker

This mod adds an player activity tracker. It updates a last seen timestamp for each player, on each player login, logout or world auto-save. Furthermore the total time played is tracked.

Use **/lastseen [playername]** to view the timestamp for a player.

Use **/top time** to view a top ranking with the most time played.

## Announcements

This mods adds a welcome message and automatic announcements to the game.
See *announcements.example.json* and place it in your savegame folder, like */gamedata/savegames/mysavegamename/announcements.json* to activate them.

## Events

This mod adds a simple event system. Players with **mods.scarabol.commands.events** permission can start an event with **/eventstart** at any position. Then all players are allowed to join using **/eventjoin** or leave the event with **/eventleave**. On join the players are warped to the event position. On leave players are warped back to their original position before the event. Same applies if an event is stopped using **/eventstop**. Stopping an event also requires the **mods.scarabol.commands.events** permission. There can only be one event at a time.

## Colonists Cap

This mod can set a maximum for each players colonists number. Each colonist beyond the limit will be killed. By default the limitation is disabled.

Players with **mods.scarabol.commands.colonycap** permission can use **/colonycap [max-number-of-colonists] [check-interval-delay]** to configure a limit or **/colonycap -1** to disable the limitation.

## Fast Travel System

This allows all players (no permission required) to use **/travel** to warp themself from a start-point to the travel path end-point.

Creating travel paths requires permission **mods.scarabol.commands.travelpaths**

Use **/travelhere** to start a travel path.

Use **/travethere** to set the end-point of a travel path.

## Whispering

Use **/whisper [playername] hello friend** or **/w [playername] hello friend** to send a chat message to a specific player instead of the complete server.

## Spam-Protect

Use **/mute [playername] [minutes]** to block a player from EVERY chatting for certain minutes.
After timeout the player is automatically unblocked or one can use **/unmute [playername]** to remove the blocking manually.

Muting and Unmuting players requires the **mods.scarabol.commands.mute** permission.

## Further Commands

<dl>
<dt>/bannername</dt>
<dd>Requires no permission<br>Tells you the name of the owner of the closest banner.</dd>
<dt>/online</dt>
<dd>Requires no permission<br>Lists all online player names.</dd>
<dt>/top [score|food|colonists|time|itemtypename]</dt>
<dd>Requires no permission<br>Shows a top 10 ranking for the given category or item type</dd>
<dt>/serverpop</dt>
<dd>Requires no permission<br>Shows current server population.</dd>
<dt>/stuck</dt>
<dd>Requires no permission<br>If you don't move for 1 minute, you're warped back to spawn.</dd>
<dt>/itemid</dt>
<dd>Requires no permission<br>Prints all items and their key currently stored in the players inventory. Useful for trade and trash.</dd>
<dt>/trade [playername] [itemname] [amount]</dt>
<dd>Requires no permission<br>Transfers the given amount of items from your stockpile to the other players stockpile.<br>Example: /trade MyFriend planks 100</dd>
<dt>/trash [itemname] [amount]</dt>
<dd>Requires no permission<br>Deletes number of items from your stockpile and inventory.</dd>

<dt>/warp [targetplayername]</dt>
<dd>Requires permission: <b>mods.scarabol.commands.warp.self</b><br>Warps to the given playernames position.</dd>
<dt>/warp [targetplayername] [teleportplayername]</dt>
<dd>Requires permission: <b>mods.scarabol.commands.warp.player</b><br>Warps the second given player to the first given playernames position.</dd>
<dt>/warpbanner [targetplayername] [teleportplayername]</dt>
<dd>Requires permission: <b>mods.scarabol.commands.warp.banner</b><br>Warps to the first given playernames banner position. If two names are provided, the second player is warped to the firsts banner position.</dd>
<dt>/warpspawn [teleportplayername]</dt>
<dd>Requires permission: <b>mods.scarabol.commands.warp.spawn</b><br>Warps the given player to the spawn point. If no playername is provided, warps the calling player.</dd>
<dt>/warpplace [x] [y] [z]</dt>
<dd>Requires permission: <b>mods.scarabol.commands.warp.place</b><br>Warps the calling player to the given position.</dd>

<dt>/god</dt>
<dd>Requires permission: <b>mods.scarabol.commands.god</b><br>Grants or revokes the calling players super permission. This is handy for admins, who want to try a regular peasants gaming experience.</dd>
<dt>/ban [playername]</dt>
<dd>Requires permission: <b>mods.scarabol.commands.ban</b><br>Adds a player to the blacklist and kicks him.</dd>
<dt>/kick [playername]</dt>
<dd>Requires permission: <b>mods.scarabol.commands.kick</b><br>Kicks a player from the server.</dd>
<dt>/drain</dt>
<dd>Requires permission: <b>mods.scarabol.commands.drain</b><br>Drys out a small lake or puddle with up to 5k blocks.</dd>
<dt>/cleanbanners</dt>
<dd>Requires permission: <b>mods.scarabol.commands.cleanbanners</b><br>Removes all banners from banner-tracker, which have no banner block in the world. Useful after server crashes.</dd>
<dt>/killplayer [playername]</dt>
<dd>Requires permission: <b>mods.scarabol.commands.killplayer</b><br>Kills the player with the given playername.</dd>
<dt>/killnpcs [playername]</dt>
<dd>Requires permission: <b>mods.scarabol.commands.killnpcs</b><br>Kills all npcs of the given playername. Useful to reduce lag on crowded servers.</dd>
<dt>/inactive [days]</dt>
<dd>Requires permission: <b>mods.scarabol.commands.inactive</b><br>Lists all players, who have not logged in or out since the last number of days.</dd>
<dt>/purgeall [days]</dt>
<dd>Requires permission: <b>mods.scarabol.commands.purgeall</b><br>Kills all NPCs and removes banner for each player, who has not logged in or out since the last number of days.</dd>
<dt>/trashplayer [playername] [itemname] [amount]</dt>
<dd>Requires permission: <b>mods.scarabol.commands.trashplayer</b><br>Removes the given amount and type of items from the given players stockpile and inventory. The playername and itemname can be set to 'all', which is also the default value for amount.<br><b>Note: For safety reasons you can't use /trashplayer all all all</b></dd>
</dl>

## Installation

**This mod must be installed on server!**

* download a (compatible) [release](https://github.com/Scarabol/ColonyCommandsMod/releases) or build from source code (see below)
* place the unzipped *Scarabol* folder inside your *ColonySurvival/gamedata/mods/* directory, like *ColonySurvival/gamedata/mods/Scarabol/*

## Build

* install Linux
* download source code
```Shell
git clone https://github.com/Scarabol/ColonyCommandsMod
```
* use make
```Shell
cd ColonyCommandsMod
make
```

**Pull requests welcome!**

