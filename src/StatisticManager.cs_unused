using System;
using System.IO;
using System.Collections.Generic;
using Pipliz;
using Pipliz.JSON;
using Pipliz.Threading;
using Chatting;

namespace ColonyCommands {

	public static class StatisticManager
	{
		private static uint iteration = 0;
		private static bool didReport = true;
		private static Dictionary<Players.Player, List<TrackedItem>> trackedItems =
			new Dictionary<Players.Player, List<TrackedItem>>();
		private const double WAIT_DELAY = 10.0;
		private const string CONFIG_FILE = "production-tracking.json";

		private static string ConfigFilePath
		{
			get {
				return Path.Combine(Path.Combine("gamedata", "savegames"), Path.Combine(ServerManager.WorldName, CONFIG_FILE));
			}
		}

		// Add item to track
		public static bool AddTrackingItem(Players.Player owner, ushort type, ushort interval = 1)
		{
			List<TrackedItem> itemList;
			if (trackedItems.ContainsKey(owner)) {
				itemList = trackedItems[owner];
			} else {
				itemList = new List<TrackedItem>();
				trackedItems[owner] = itemList;
			}

			// avoid duplicate tracking
			foreach (TrackedItem item in itemList) {
				if (type == item.item.ItemIndex) {
					return false;
				}
			}
			// avoid non existing items
			ItemTypes.ItemType itemType;
			if (!ItemTypes.TryGetType(type, out itemType)) {
				return false;
			}

			Stockpile playerStockpile = Stockpile.GetStockPile(owner);
			int currentAmount = playerStockpile.AmountContained(itemType.ItemIndex);
			TrackedItem theItem = new TrackedItem(itemType, interval, currentAmount);
			itemList.Add(theItem);
			return true;
		}

		// Remove item from tracking
		public static bool RemoveTrackingItem(Players.Player owner, ushort type)
		{
			if (!trackedItems.ContainsKey(owner)) {
				return false;
			}
			List<TrackedItem> itemList = trackedItems[owner];
			TrackedItem foundItem = null;
			foreach (TrackedItem item in itemList) {
				if (type == item.item.ItemIndex) {
					foundItem = item;
				}
			}

			if (foundItem != null) {
				itemList.Remove(foundItem);
				return true;
			}

			// clean up if empty
			if (itemList.Count == 0) {
				trackedItems.Remove(owner);
			}

			return false;
		}

		// Get comma separated list of tracked items
		public static bool GetItemNameList(Players.Player owner, out string list)
		{
			list = "";
			if (!trackedItems.ContainsKey(owner)) {
				return false;
			}

			List<TrackedItem> itemList = trackedItems[owner];
			foreach (TrackedItem item in itemList) {
				if (!list.Equals("")) {
					list += ", ";
				}
				list += string.Format("{0} ({1}/d)", char.ToUpper(item.Name[0]) + item.Name.Substring(1), item.interval);
			}
			return true;
		}

		// Tracking background thread
		public static void TrackItems()
		{
			if (TimeCycle.IsDay && !didReport) {
				// update all tracked items with the new day value
				foreach (KeyValuePair<Players.Player, List<TrackedItem>> kvp in trackedItems) {
					Players.Player owner = kvp.Key;
					List<TrackedItem> itemList = kvp.Value;
					Stockpile playerStockpile = Stockpile.GetStockPile(owner);

					foreach (TrackedItem item in itemList) {

						if (iteration % item.interval != 0) {
							continue;
						}

						item.Update(playerStockpile.AmountContained(item.item.ItemIndex));
						string delta = item.GetDeltaFormatted();
						string name = char.ToUpper(item.Name[0]) + item.Name.Substring(1);
						if (owner.IsConnected) {
							if (item.DidIncrease) {
								Chat.Send(owner, $"<color=#15cf64>{name} per {item.interval}/day: +{delta}</color>");
							}
							if (item.DidDecrease) {
								Chat.Send(owner, $"<color=red>{name} per {item.interval}/day: {delta}</color>");
							}
						}
					}
				}

				++iteration;
				didReport = true;
			}
			if (!TimeCycle.IsDay) {
				didReport = false;
			}

			ThreadManager.InvokeOnMainThread(delegate() {
					StatisticManager.TrackItems();
					}, WAIT_DELAY);
		}

		// Load config from file
		public static void Load()
		{
			JSONNode jsonConfig;
			if (!JSON.Deserialize(ConfigFilePath, out jsonConfig, false)) {
				Log.Write($"{CONFIG_FILE} not found, no items to track");
				return;
			}

			Log.Write($"Loading items to track from {CONFIG_FILE}");
			try {
				foreach (JSONNode node in jsonConfig.LoopArray()) {
					string PlayerId = node.GetAs<string>("player");
					Players.Player owner;
					string error;
					if (!PlayerHelper.TryGetPlayer(PlayerId, out owner, out error, true)) {
						Log.Write($"Could not identify player id {PlayerId} from {CONFIG_FILE}");
						continue;
					}

					JSONNode itemRecords;
					node.TryGetAs("items", out itemRecords);
					List<TrackedItem> itemList = new List<TrackedItem>();
					foreach (JSONNode record in itemRecords.LoopArray()) {
						itemList.Add(new TrackedItem(record));
					}
					trackedItems[owner] = itemList;
				}
			} catch (Exception e) {
				Log.Write($"Error parsing {CONFIG_FILE}: {e.Message}");
			}
			return;
		}

		// Save config to file
		public static void Save()
		{
			Log.Write($"Saving {CONFIG_FILE}");
			JSONNode config = new JSONNode(NodeType.Array);
			foreach (KeyValuePair<Players.Player, List<TrackedItem>> kvp in trackedItems) {
				Players.Player owner = kvp.Key;
				List<TrackedItem> itemList = kvp.Value;
				JSONNode playerRecord = new JSONNode();
				playerRecord.SetAs("player", owner.ID.steamID);

				JSONNode jsonItems = new JSONNode(NodeType.Array);
				foreach (TrackedItem item in itemList) {
					jsonItems.AddToArray((JSONNode)(item));
				}
				playerRecord.SetAs("items", jsonItems);
				config.AddToArray(playerRecord);
			}

			try {
				JSON.Serialize(ConfigFilePath, config);
			} catch (Exception e) {
				Log.Write($"Error saving {CONFIG_FILE}: {e.Message}");
			}
			return;
		}

	}

}

