using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Jobs;

namespace ColonyCommands
{

	[ModLoader.ModManager]
	public static class AreaShowManager
	{
		// list of players that want to see all area highlights
		private static List<Players.Player> playerList = new List<Players.Player>();

		private static Dictionary<Colony, List<IAreaJob>> allAreaJobs = typeof(AreaJobTracker).GetField("colonyTrackedJobs", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as Dictionary<Colony, List<IAreaJob>>;

		// check if player is on list
		public static bool IsShowAllActive(Players.Player player)
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

		[ModLoader.ModCallback(ModLoader.EModCallbackType.OnSendAreaHighlights, AntiGrief.NAMESPACE + ".OnSendAreaHighlights")]
		public static void OnSendAreaHighlights(Players.Player causedBy, List<AreaJobTracker.AreaHighlight> jobs, List<ushort> activeTypes)
		{
			if (!IsShowAllActive(causedBy)) {
				return;
			}

			Vector3 playerPos = causedBy.Position;
			foreach (KeyValuePair<Colony, List<IAreaJob>> kvp in allAreaJobs) {
				if (kvp.Key == causedBy.ActiveColony) {
					continue;
				}
				foreach (IAreaJob job in kvp.Value) {
					Vector3 center = new Vector3((job.Maximum.x + job.Minimum.x) / 2,
						(job.Maximum.y + job.Minimum.y) / 2,
						(job.Maximum.z + job.Minimum.z) / 2);
					if (Vector3.Distance(playerPos, center) < ServerManager.HostingSettings.MaxDrawDistance) {
						jobs.Add(new AreaJobTracker.AreaHighlight(job.Minimum, job.Maximum, job.AreaTypeMesh, job.AreaType));
					}
				}
			}
			return;
		}

	} // class

} // namespace

