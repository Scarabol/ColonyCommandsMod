using System.Collections.Generic;

namespace ColonyCommands
{

	public static class AreaShowManager
	{
		// list of players that want to see all area highlights
		private static List<Players.Player> playerList = new List<Players.Player>();

		// check if player is on list
		public static bool ShowAll(Players.Player player)
		{
			if (playerList.Contains(player)) {
				return true;
			}
			return false;
		}

		// add player to list
		public static bool Add(Players.Player player)
		{
			if (playerList.Contains(player)) {
				return false;
			}
			playerList.Add(player);
			AreaJobTracker.SendData(player);
			return true;
		}

		// remove player from list
		public static bool Remove(Players.Player player)
		{
			if (playerList.Contains(player)) {
				playerList.Remove(player);
				AreaJobTracker.SendData(player);
				return true;
			}
			return false;
		}


	} // class

} // namespace

