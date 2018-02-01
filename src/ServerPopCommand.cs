using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using System.Threading;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class ServerPopCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.serverpopcommand.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      ChatCommands.CommandManager.RegisterCommand (new ServerPopCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/serverpop");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        long allPlayers = Players.PlayerDatabase.Count;
        long onlinePlayers = Players.CountConnected;
        long allFollower = 0;
        Players.PlayerDatabase.ForeachValue (player => allFollower += Colony.Get (player).FollowerCount);
        long allMonsters = Server.Monsters.MonsterTracker.MonstersTotal;
        long allUnits = allPlayers + allFollower + allMonsters;
        Chat.Send (causedBy, $"Server Population: {allUnits}, Players: {allPlayers}, Online: {onlinePlayers}, Colonists: {allFollower}, Monsters: {allMonsters}");
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }
}
