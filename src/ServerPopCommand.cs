using System.Collections.Generic;
using Chatting;
using Chatting.Commands;

namespace ColonyCommands
{

	public class ServerPopCommand : IChatCommand
	{

		public bool TryDoCommand(Players.Player causedBy, string chattext, List<string> splits)
		{
			if (!splits[0].Equals("/serverpop")) {
				return false;
			}
			var allPlayers = Players.PlayerDatabase.Count;
			var onlinePlayers = Players.CountConnected;
			var allFollower = 0;
			foreach (Colony colony in ServerManager.ColonyTracker.ColoniesByID.Values) {
				allFollower += colony.FollowerCount;
			}
			var allMonsters = Monsters.MonsterTracker.MonstersTotal;
			var allUnits = allPlayers + allFollower + allMonsters;
			Chat.Send(causedBy, $"Server Population: {allUnits}, Players: {allPlayers}, Online: {onlinePlayers}, Colonists: {allFollower}, Monsters: {allMonsters}");
			return true;
		}
	}
}

