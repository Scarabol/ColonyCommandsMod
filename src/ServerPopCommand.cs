using Chatting;
using Chatting.Commands;

namespace ColonyCommands
{

  public class ServerPopCommand : IChatCommand
  {

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/serverpop");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      var allPlayers = Players.PlayerDatabase.Count;
      var onlinePlayers = Players.CountConnected;
      var allFollower = 0;
      ServerManager.ColonyTracker.ColoniesByID.ForeachValue(colony => allFollower += colony.FollowerCount);
      var allMonsters = NPC.MonsterTracker.MonstersTotal;
      var allUnits = allPlayers + allFollower + allMonsters;
      Chat.Send (causedBy, $"Server Population: {allUnits}, Players: {allPlayers}, Online: {onlinePlayers}, Colonists: {allFollower}, Monsters: {allMonsters}");
      return true;
    }
  }
}
