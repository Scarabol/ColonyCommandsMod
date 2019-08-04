# Colony Commands by Adrenalynn (continued from Scarabol's mod)

**Special thanks to Tjohei for most of the ideas and feature requests.**

**Special thanks to Crone for the chat coloring system and score system.**

## Anti-Grief

This mods adds an anti-griefing mechanism to the server.
Regular players can't change 50 blocks around the spawn point and 50 blocks around other player banners.
Both values can be adjusted.
Furthermore players are auto-**jailed** if they kill more than 2 foreign colonists, auto-**kicked** if killing more than 5 and auto-**banned** if they kill more than 6 foreign colonists.
Those values can be configured in the config file, set them to 0 to disable the feature.

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

## Jail System

The jail is an extension for the Anti-Grief system. Admin or staff members can throw players into jail as punishment. A jailed player can only move within the defined jail area. Should a player try to escape repeatedly his/her jail time will be automatically extended (configurable).
Getting jailed is also available as punishment option for killing another player's colonists.

Use **/setjail [range]** to define the server jail at your current position. Parameter range determines how many blocks out the border of the jail is, the default is 5.
**Notice:** The jail will automatically create a protection area with its range. You might want to create a larger protection area instead, depending on your actual jail build.

Use **/setjail visitor** to define a visitor position for people wanting to visit the jail and its inmates. This position can be outside or inside the jail, visitors are always free to move.
Requires permission **mods.scarabol.commands.setjailposition**

Use **/jail {player} [time] {reason}** to send a player into jail. The default jail time is 3 minutes or you can specify the exact amount. Reason can be any text (including white spaces) but don't start it with a number (would be taken as time value instead).
Requires permission **mods.scarabol.commands.jail**

Use **/jailrelease {player}** to release a player from jail for good conduct. Players will be released automatically after their time is done.
Requires permission **mods.scarabol.commands.jail**

Visitors can use **/jailvisit** to visit the jail (if the visitor position was set). Visitors are completely free to move and can leave anytime.
For convenience there is also **/jailleave** which will warp a visitor to the position where he/she used /jailvisit. Just in case they have no others means to leave.
No permissions required for both commands

A jailed player can use **/jailtime** to check his/her remaining time in jail. Once the time is done the player will be automatically released and teleported to spawn.
No permission required

Use **/jailrec [player]** to check the jail history records. With a player name given it will show all jail records for that player. Without player name it will show the last 10 jail actions.
Requires permission **mods.scarabol.commands.jail**

<dl>
<dt>Configurable Settings</dt>
<dd>After the first start a new file jail-config.json will be generated at the world save directory (gamedate/savegames/&lt;worldname&gt;).</dd>
<dd>defaultJailTime: 5 Minutes, can be adjusted to any value</dd>
<dd>graceEscapeAttempts: 3. Number of escape attempts after which additional jail time is added. Set to 0 to disable.</dd>
<dd>protection-ranges.json contains a new entry NpcKillsJailThreshold, similar to kick and ban thresholds. If unwanted, set the value higher than the kick threshold to disable jailing as punishment option.</dd>
</dl>

## Writing Player Names

A lot of the commands require the name of a player as target. Names of players can sometimes be hard to type due to international character sets. For all commands the name can be specified in different ways:

**/command 'player name with blanks'** enclose the player name with single quotes.

**/command #12345678** use a 8 digit hash instead of the name. The hashes are printed out by the **/online id** command.

**/command &lt;steamid&gt;** if all of the above fails one can use the full steamid also

## Further Commands

<dl>
<dt>/help</dt>
<dd>Requires no permission. Shows a list of available commands. One can add a text filter optionally to search for specific commands, like /help jail, for example.<br>
The other variant of this command is <b>/help admin</b> which list all admin commands. Permission <b>mods.scarabol.commands.adminhelp</b> is required for that.</dd>
<dt>/bannername</dt>
<dd>Requires no permission<br>Tells you the name of the owner of the closest banner.</dd>
<dt>/online</dt>
<dd>Requires no permission<br>Lists all online player names.<br><b>/online id</b> lists the players including a 8 digit hash which can be used instead of the player name for all commands</dd>
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
<dt>/warpspawn</dt>
<dd>Requires permission: <b>mods.scarabol.commands.warp.spawn.self</b><br>Warps the calling player to the spawn point.</dd>
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
<dt>/killplayer [playername]</dt>
<dd>Requires permission: <b>mods.scarabol.commands.killplayer</b><br>Kills the player with the given playername.<br>
To allow players killing themselfes permission <b>mods.scarabol.commands.killplayer.self</b> can be given.</dd>
<dt>/killnpcs [playername]</dt>
<dd>Requires permission: <b>mods.scarabol.commands.killnpcs</b><br>Kills all npcs of the given playername. Useful to reduce lag on crowded servers.<br>
To allow players killing their own colonists permission <b>mods.scarabol.commands.killnpcs.self</b> can be given.</dd>
<dt>/inactive [days]</dt>
<dd>Requires permission: <b>mods.scarabol.commands.inactive</b><br>Lists all players, who have not logged in or out since the last number of days.</dd>
<dt>/purgeall [days]</dt>
<dd>Requires permission: <b>mods.scarabol.commands.purgeall</b><br>Kills all NPCs and removes banner for each player, who has not logged in or out since the last number of days.</dd>
<dt>/trashplayer [playername] [itemname] [amount]</dt>
<dd>Requires permission: <b>mods.scarabol.commands.trashplayer</b><br>Removes the given amount and type of items from the given players stockpile and inventory. The playername and itemname can be set to 'all', which is also the default value for amount.<br><b>Note: For safety reasons you can't use /trashplayer all all all</b></dd>
<dt>/areashow</dt>
<dd>Checks if the player is inside a custom protection area and will print its coordinates. If not inside an area it will print the closest area nearby instead</dd>
<dt>/spawnnpc {number}</dt>
<dd>Requires permission: <b>mods.scarabol.commands.npcandbeds</b><br>
Spawn given number of colonists. Intended for admin/test use, spawning will not cost food and no beds required.</dd>
<dt>/beds {number}</dt>
<dd>Requires permission: <b>mods.scarabol.commands.npcandbeds</b><br>
Create given number of beds around your banner. Intended for admin/test use, beds will be placed in a rectangular spiral form around your banner</dd>
<dt>/purgebanner</dt>
<dd>Requires permission: <b>mods.scarabol.purgebanner</b><br>
For admin staff only. Remove the banner closest to you, if it is the last banner of a colony the colony gets deleted.</dd>
<dd><b>/purgebanner colony</b>purge a whole colony at once</dd>
<dd><b>/purgebanner [playername]</b>remove all banners/colonyaccess for a player.</dd>
<dt>/purgebanner all [range]</dt>
<dd>Requires permission: <b>mods.scarabol.purgeallbanner</b><br>
Purge <b>all</b> banners within the given range. This command can be dangerous</dd>
</dl>

## Installation

**This mod must be installed on the server!**

* download a (compatible) [release](https://github.com/adrenalynn/ColonyCommandsMod/releases) or build from source code
* place the unzipped folder inside your *ColonySurvival/gamedata/mods/* directory, like *ColonySurvival/gamedata/mods/adrenalynn/ColonyCommandsMod*

**Pull requests welcome!**

