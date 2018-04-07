using Pipliz.Chatting;
using ChatCommands;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class ServerPopCommand : IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.serverpopcommand.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      CommandManager.RegisterCommand (new ServerPopCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/serverpop");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      var allPlayers = Players.PlayerDatabase.Count;
      var onlinePlayers = Players.CountConnected;
      var allFollower = 0;
      Colony.collection.ForeachValue (colony => allFollower += colony.FollowerCount);
      var allMonsters = Server.Monsters.MonsterTracker.MonstersTotal;
      var allUnits = allPlayers + allFollower + allMonsters;
      Chat.Send (causedBy, $"Server Population: {allUnits}, Players: {allPlayers}, Online: {onlinePlayers}, Colonists: {allFollower}, Monsters: {allMonsters}");
      return true;
    }
  }
}
