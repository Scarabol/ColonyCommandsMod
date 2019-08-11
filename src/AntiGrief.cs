using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Chatting;
using Chatting.Commands;
using Pipliz.JSON;
using TerrainGeneration;
using BlockEntities.Implementations;
using UnityEngine;
using Shared.Networking;

namespace ColonyCommands {

	[ModLoader.ModManager]
	public static class AntiGrief
	{
		public const string MOD_PREFIX = "mods.scarabol.commands.";
		public const string NAMESPACE = "AntiGrief";
		public static string MOD_DIRECTORY;
		public const string PERMISSION_SUPER = "mods.scarabol.antigrief";
		public const string PERMISSION_SPAWN_CHANGE = PERMISSION_SUPER + ".spawnchange";
		public const string PERMISSION_BANNER_PREFIX = PERMISSION_SUPER + ".banner.";
		private const string COLONY_ID_FORMAT = "colony.{0:0000000000}";
		static int SpawnProtectionRangeXPos;
		static int SpawnProtectionRangeXNeg;
		static int SpawnProtectionRangeZPos;
		static int SpawnProtectionRangeZNeg;
		static int BannerProtectionRangeX;
		static int BannerProtectionRangeZ;
		public static int ColonistLimit;
		public static int ColonistLimitCheckSeconds;
		public static List<CustomProtectionArea> CustomAreas = new List<CustomProtectionArea> ();
		static int NpcKillsJailThreshold;
		static int NpcKillsKickThreshold;
		static int NpcKillsBanThreshold;
		static Dictionary<Players.Player, int> KillCounter = new Dictionary<Players.Player, int> ();

		static string ConfigFilepath {
			get {
				return Path.Combine (Path.Combine ("gamedata", "savegames"), Path.Combine (ServerManager.WorldName, "antigrief-config.json"));
			}
		}

		// used only by the /top command to hide players from the scoring
		public static List<Players.Player> UnscoredPlayers = new List<Players.Player>();

		[ModLoader.ModCallback(ModLoader.EModCallbackType.OnAssemblyLoaded, NAMESPACE + ".OnAssemblyLoaded")]
		public static void OnAssemblyLoaded(string path)
		{
			MOD_DIRECTORY = Path.GetDirectoryName(path);
			Log.Write("Loaded ColonyCommands (Anti-Grief)");
		}

		[ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, NAMESPACE + ".RegisterTypes")]
		public static void AfterItemTypesDefined()
		{
			Log.Write("Registering commands (Anti-Grief)");
			CommandManager.RegisterCommand(new AnnouncementsChatCommand());
			CommandManager.RegisterCommand(new AntiGriefChatCommand());
			CommandManager.RegisterCommand(new BanChatCommand());
			CommandManager.RegisterCommand(new BannerNameChatCommand());
			CommandManager.RegisterCommand(new BetterChatCommand());
			// CommandManager.RegisterCommand(new CleanBannersChatCommand());
			CommandManager.RegisterCommand(new ColonyCap());
			CommandManager.RegisterCommand(new DrainChatCommand());
			// CommandManager.RegisterCommand(new GiveAllChatCommand());
			CommandManager.RegisterCommand(new GodChatCommand());
			CommandManager.RegisterCommand(new InactiveChatCommand());
			CommandManager.RegisterCommand(new ItemIdChatCommand());
			CommandManager.RegisterCommand(new KickChatCommand());
			CommandManager.RegisterCommand(new KillNPCsChatCommand());
			CommandManager.RegisterCommand(new KillPlayerChatCommand());
			CommandManager.RegisterCommand(new LastSeenChatCommand());
			CommandManager.RegisterCommand(new NoFlightChatCommand());
			CommandManager.RegisterCommand(new OnlineChatCommand());
			// CommandManager.RegisterCommand(new PurgeAllChatCommand());
			CommandManager.RegisterCommand(new ServerPopCommand());
			CommandManager.RegisterCommand(new StuckChatCommand());
			CommandManager.RegisterCommand(new TopChatCommand());
			CommandManager.RegisterCommand(new TradeChatCommand());
			CommandManager.RegisterCommand(new TrashChatCommand());
			// CommandManager.RegisterCommand(new TrashPlayerChatCommand());
			CommandManager.RegisterCommand(new TravelChatCommand());
			CommandManager.RegisterCommand(new TravelHereChatCommand());
			CommandManager.RegisterCommand(new TravelThereChatCommand());
			CommandManager.RegisterCommand(new TravelRemoveChatCommand());
			CommandManager.RegisterCommand(new WarpBannerChatCommand());
			CommandManager.RegisterCommand(new WarpChatCommand());
			CommandManager.RegisterCommand(new WarpPlaceChatCommand());
			CommandManager.RegisterCommand(new WarpSpawnChatCommand());
			CommandManager.RegisterCommand(new WhisperChatCommand());
			CommandManager.RegisterCommand(new SetJailCommand());
			CommandManager.RegisterCommand(new JailCommand());
			CommandManager.RegisterCommand(new JailReleaseCommand());
			CommandManager.RegisterCommand(new JailVisitCommand());
			CommandManager.RegisterCommand(new JailLeaveCommand());
			CommandManager.RegisterCommand(new JailRecCommand());
			CommandManager.RegisterCommand(new JailTimeCommand());
			CommandManager.RegisterCommand(new AreaShowCommand());
			CommandManager.RegisterCommand(new HelpCommand());
			// CommandManager.RegisterCommand(new DeleteJobsCommand());
			// CommandManager.RegisterCommand(new DeleteJobSpeedCommand());
			// CommandManager.RegisterCommand(new ProductionCommand());
			CommandManager.RegisterCommand(new ColorTestCommand());
			CommandManager.RegisterCommand(new SpawnNpcCommand());
			CommandManager.RegisterCommand(new BedsCommand());
			CommandManager.RegisterCommand(new PurgeBannerCommand());
			CommandManager.RegisterCommand(new MuteChatCommand());
			CommandManager.RegisterCommand(new UnmuteChatCommand());
			CommandManager.RegisterCommand(new ListPlayerChatCommand());
			return;
		}

		[ModLoader.ModCallback (ModLoader.EModCallbackType.OnTryChangeBlock, NAMESPACE + ".OnTryChangeBlock")]
		public static void OnTryChangeBlock (ModLoader.OnTryChangeBlockData userData)
		{
			if (userData.RequestOrigin.Type != BlockChangeRequestOrigin.EType.Player) {
				return;
			}
			Players.Player causedBy = userData.RequestOrigin.AsPlayer;
			if (causedBy == null) {
				return;
			}
			Pipliz.Vector3Int playerPos = userData.Position;

			// allow staff members
			if (PermissionsManager.HasPermission(causedBy, PERMISSION_SUPER)) {
				return;
			}

			// check spawn area
			int ox = playerPos.x - ServerManager.TerrainGenerator.GetDefaultSpawnLocation().x;
			int oz = playerPos.z - ServerManager.TerrainGenerator.GetDefaultSpawnLocation().z;
			if (((ox >= 0 && ox <= SpawnProtectionRangeXPos) || (ox < 0 && ox >= -SpawnProtectionRangeXNeg)) && ((oz >= 0 && oz <= SpawnProtectionRangeZPos) || (oz < 0 && oz >= -SpawnProtectionRangeZNeg))) {
				if (!PermissionsManager.HasPermission(causedBy, PERMISSION_SPAWN_CHANGE)) {
					if (causedBy.ConnectionState == Players.EConnectionState.Connected) {
						Chat.Send(causedBy, "<color=red>You don't have permission to change the spawn area!</color>");
					}
					BlockCallback(userData);
					return;
				}
			}

			// Check all banners and then decide by Colony.Owners if allowed or not
			int checkRangeX = BannerProtectionRangeX;
			int checkRangeZ = BannerProtectionRangeZ;
			if (userData.TypeNew.ItemIndex == BlockTypes.BuiltinBlocks.Indices.banner) {
				checkRangeX *= 2;
				checkRangeZ *= 2;
			}
			foreach (Colony checkColony in ServerManager.ColonyTracker.ColoniesByID.Values) {

				foreach (BannerTracker.Banner checkBanner in checkColony.Banners) {
					int distanceX = (int)System.Math.Abs(playerPos.x - checkBanner.Position.x);
					int distanceZ = (int)System.Math.Abs(playerPos.z - checkBanner.Position.z);

					if (distanceX < checkRangeX && distanceZ < checkRangeZ) {
						foreach (Players.Player owner in checkColony.Owners) {
							if (owner == causedBy) {
								return;
							}
						}
						// check if /antigrief permission - only done for banner placement
						// after the banner is placed the player will be an owner of the colony
						if (userData.TypeNew.ItemIndex == BlockTypes.BuiltinBlocks.Indices.banner) {

							// permission for this colony id
							if (PermissionsManager.HasPermission(causedBy, PERMISSION_BANNER_PREFIX + string.Format(COLONY_ID_FORMAT, checkColony.ColonyID))) {
								return;
							}

							// permission for all colonies of the owner
							foreach (Players.Player owner in checkColony.Owners) {
								if (PermissionsManager.HasPermission(causedBy, PERMISSION_BANNER_PREFIX + owner.ID.steamID)) {
									return;
								}
							}
						}

						if (userData.TypeNew.ItemIndex == BlockTypes.BuiltinBlocks.Indices.banner) {
							int tooCloseX = checkRangeX - distanceX;
							int tooCloseZ = checkRangeZ - distanceZ;
							int moveBlocks = (tooCloseX > tooCloseZ) ? tooCloseX : tooCloseZ;
							Chat.Send(causedBy, $"<color=red>Too close to another banner! Please move {moveBlocks} blocks further</color>");
						} else {
							Chat.Send(causedBy, "<color=red>No permission to change blocks near this banner!</color>");
						}
						BlockCallback(userData);
						return;
					}
				}
			}

			// check custom protection areas
			foreach (CustomProtectionArea area in CustomAreas) {
				if (area.Contains(playerPos) && !PermissionsManager.HasPermission(causedBy, PERMISSION_SPAWN_CHANGE)) {
					Chat.Send(causedBy, "<color=red>You don't have permission to change this protected area!</color>");
					BlockCallback(userData);
					return;
				}
			}

			return;
		}

		// Block (deny) a TryChangeBlock event
		static void BlockCallback(ModLoader.OnTryChangeBlockData userData)
		{
			userData.CallbackState = ModLoader.OnTryChangeBlockData.ECallbackState.Cancelled;
			userData.InventoryItemResults.Clear();
		}

		// load everything after the world starts
		[ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, NAMESPACE + ".AfterWorldLoaded")]
		public static void AfterWorldLoad()
		{
			Load();
			JailManager.Load();
			WaypointManager.Load();
			// StatisticManager.Load();
			// StatisticManager.TrackItems();
		}

		// send welcome message
		[ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerConnectedLate, NAMESPACE + ".OnPlayerConnected")]
		public static void OnPlayerConnectedLate(Players.Player player)
		{
			Chat.Send(player, "<color=yellow>Anti-Grief protection enabled</color>");
		}

		// load config
		public static void Load()
		{
			SpawnProtectionRangeXPos = 50;
			SpawnProtectionRangeXNeg = 50;
			SpawnProtectionRangeZPos = 50;
			SpawnProtectionRangeZNeg = 50;
			BannerProtectionRangeX = 50;
			BannerProtectionRangeZ = 50;
			CustomAreas.Clear();
			JSONNode jsonConfig;
			if (JSON.Deserialize (ConfigFilepath, out jsonConfig, false)) {
				int rx;
				if (jsonConfig.TryGetAs ("SpawnProtectionRangeX+", out rx)) {
					SpawnProtectionRangeXPos = rx;
				} else if (jsonConfig.TryGetAs ("SpawnProtectionRangeX", out rx)) {
					SpawnProtectionRangeXPos = rx;
				} else {
					Log.Write ($"Could not get SpawnProtectionRangeX+ or SpawnProtectionRangeX from json config, using default value {SpawnProtectionRangeXPos}");
				}
				if (jsonConfig.TryGetAs ("SpawnProtectionRangeX-", out rx)) {
					SpawnProtectionRangeXNeg = rx;
				} else if (jsonConfig.TryGetAs ("SpawnProtectionRangeX", out rx)) {
					SpawnProtectionRangeXNeg = rx;
				} else {
					Log.Write ($"Could not get SpawnProtectionRangeX- or SpawnProtectionRangeX from json config, using default value {SpawnProtectionRangeXNeg}");
				}
				int rz;
				if (jsonConfig.TryGetAs ("SpawnProtectionRangeZ+", out rz)) {
					SpawnProtectionRangeZPos = rz;
				} else if (jsonConfig.TryGetAs ("SpawnProtectionRangeZ", out rz)) {
					SpawnProtectionRangeZPos = rz;
				} else {
					Log.Write ($"Could not get SpawnProtectionRangeZ+ or SpawnProtectionRangeZ from json config, using default value {SpawnProtectionRangeZPos}");
				}
				if (jsonConfig.TryGetAs ("SpawnProtectionRangeZ-", out rz)) {
					SpawnProtectionRangeZNeg = rz;
				} else if (jsonConfig.TryGetAs ("SpawnProtectionRangeZ", out rz)) {
					SpawnProtectionRangeZNeg = rz;
				} else {
					Log.Write ($"Could not get SpawnProtectionRangeZ- or SpawnProtectionRangeZ from json config, using default value {SpawnProtectionRangeZNeg}");
				}
				if (!jsonConfig.TryGetAs ("BannerProtectionRangeX", out BannerProtectionRangeX)) {
					Log.Write ($"Could not get banner protection x-range from json config, using default value {BannerProtectionRangeX}");
				}
				if (!jsonConfig.TryGetAs ("BannerProtectionRangeZ", out BannerProtectionRangeZ)) {
					Log.Write ($"Could not get banner protection z-range from json config, using default value {BannerProtectionRangeZ}");
				}
				JSONNode jsonCustomAreas;
				if (jsonConfig.TryGetAs ("CustomAreas", out jsonCustomAreas) && jsonCustomAreas.NodeType == NodeType.Array) {
					foreach (var jsonCustomArea in jsonCustomAreas.LoopArray ()) {
						try {
							CustomAreas.Add (new CustomProtectionArea (jsonCustomArea));
						} catch (Exception exception) {
							Log.WriteError ($"Exception loading custom area; {exception.Message}");
						}
					}
					Log.Write ($"Loaded {CustomAreas.Count} from file");
				}
				jsonConfig.TryGetAsOrDefault ("NpcKillsJailThreshold", out NpcKillsJailThreshold, 2);
				jsonConfig.TryGetAsOrDefault ("NpcKillsKickThreshold", out NpcKillsKickThreshold, 5);
				jsonConfig.TryGetAsOrDefault ("NpcKillsBanThreshold", out NpcKillsBanThreshold, 6);

				JSONNode jsonNameList;
				if (jsonConfig.TryGetAs("UnscoredPlayers", out jsonNameList) && jsonNameList.NodeType == NodeType.Array) {
					foreach (JSONNode jsonName in jsonNameList.LoopArray()) {
						Players.Player player;
						string error;
						string playerName = jsonName.GetAs<string>();
						if (PlayerHelper.TryGetPlayer(playerName, out player, out error, true)) {
							UnscoredPlayers.Add(player);
						} else {
							Log.Write($"Error loading unscored players {playerName}: {error}");
						}
					}
				}
				jsonConfig.TryGetAsOrDefault("ColonistLimit", out ColonistLimit, 0);
				jsonConfig.TryGetAsOrDefault("ColonistCheckInterval", out ColonistLimitCheckSeconds, 30);

				int speed = 0;
				jsonConfig.TryGetAsOrDefault("DeleteJobSpeed", out speed, 4);
				// DeleteJobsManager.SetDeleteJobSpeed(speed, false);

			} else {
				Save ();
				Log.Write ($"Could not find {ConfigFilepath} file, created default one");
			}
			Log.Write ($"Using spawn protection with X+ range {SpawnProtectionRangeXPos}");
			Log.Write ($"Using spawn protection with X- range {SpawnProtectionRangeXNeg}");
			Log.Write ($"Using spawn protection with Z+ range {SpawnProtectionRangeZPos}");
			Log.Write ($"Using spawn protection with Z- range {SpawnProtectionRangeZNeg}");
			Log.Write ($"Using banner protection with X range {BannerProtectionRangeX}");
			Log.Write ($"Using banner protection with Z range {BannerProtectionRangeZ}");
		}

		public static void AddCustomArea (CustomProtectionArea area)
		{
			CustomAreas.Add(area);
			Save ();
		}

		public static void RemoveCustomArea(CustomProtectionArea area)
		{
			CustomAreas.Remove(area);
			Save();
		}

		// save config
		public static void Save()
		{
			JSONNode jsonConfig;
			if (!JSON.Deserialize (ConfigFilepath, out jsonConfig, false)) {
				jsonConfig = new JSONNode ();
			}
			jsonConfig.SetAs ("SpawnProtectionRangeX+", SpawnProtectionRangeXPos);
			jsonConfig.SetAs ("SpawnProtectionRangeX-", SpawnProtectionRangeXNeg);
			jsonConfig.SetAs ("SpawnProtectionRangeZ+", SpawnProtectionRangeZPos);
			jsonConfig.SetAs ("SpawnProtectionRangeZ-", SpawnProtectionRangeZNeg);
			jsonConfig.SetAs ("BannerProtectionRangeX", BannerProtectionRangeX);
			jsonConfig.SetAs ("BannerProtectionRangeZ", BannerProtectionRangeZ);
			jsonConfig.SetAs ("NpcKillsKickThreshold", NpcKillsKickThreshold);
			jsonConfig.SetAs ("NpcKillsBanThreshold", NpcKillsBanThreshold);
			jsonConfig.SetAs ("NpcKillsJailThreshold", NpcKillsJailThreshold);
			var jsonCustomAreas = new JSONNode (NodeType.Array);
			foreach (var customArea in CustomAreas) {
				jsonCustomAreas.AddToArray (customArea.ToJSON ());
			}
			jsonConfig.SetAs ("CustomAreas", jsonCustomAreas);

			JSONNode jsonUnscoredPlayers = new JSONNode(NodeType.Array);
			foreach (Players.Player player in UnscoredPlayers) {
				JSONNode jsonName = new JSONNode();
				jsonName.SetAs(player.Name);
				jsonUnscoredPlayers.AddToArray(jsonName);
			}
			jsonConfig.SetAs("UnscoredPlayers", jsonUnscoredPlayers);
			// jsonConfig.SetAs("DeleteJobSpeed", DeleteJobsManager.GetDeleteJobSpeed());

			JSON.Serialize (ConfigFilepath, jsonConfig, 2);
		}

		// track NPC killing
		[ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCHit, NAMESPACE + ".OnNPCHit")]
		public static void OnNPCHit(NPC.NPCBase npc, ModLoader.OnHitData data)
		{
			if (!IsKilled(npc, data) || !IsHitByPlayer(data.HitSourceType) || !(data.HitSourceObject is Players.Player)) {
				return;
			}
			Players.Player killer = (Players.Player)data.HitSourceObject;
			foreach (Players.Player owner in npc.Colony.Owners) {
				if (owner == killer) {
					return;
				}
			}

			int kills;
			if (!KillCounter.TryGetValue(killer, out kills)) {
				kills = 0;
			}
			KillCounter[killer] = ++kills;
			if (NpcKillsBanThreshold > 0 && kills >= NpcKillsBanThreshold) {
				Chat.SendToConnected($"{killer.Name} banned for killing too many colonists");
				BlackAndWhitelisting.AddBlackList(killer.ID.steamID.m_SteamID);
				Players.Disconnect(killer);
			} else if (NpcKillsKickThreshold > 0 && kills >= NpcKillsKickThreshold) {
				Chat.SendToConnected($"{killer.Name} kicked for killing too many colonists");
				Players.Disconnect(killer);
			} else if (NpcKillsJailThreshold > 0 && kills >= NpcKillsJailThreshold) {
				Chat.SendToConnected($"{killer.Name} put in Jail for killing too many colonists");
				JailManager.jailPlayer(killer, null, "Killing Colonists", JailManager.DEFAULT_JAIL_TIME);
			}
			Log.Write($"{killer.Name} killed a colonist of {npc.Colony.ColonyID} at {npc.Position}");
			int remainingJail = NpcKillsJailThreshold - kills;
			int remainingKick = NpcKillsKickThreshold - kills;
			Chat.Send(killer, $"You killed a colonist, remaining until jail: {remainingJail}, remaining until kick: {remainingKick}");
		}

		static bool IsKilled(NPC.NPCBase npc, ModLoader.OnHitData data)
		{
			return npc.health - data.ResultDamage <= 0;
		}

		static bool IsHitByPlayer(ModLoader.OnHitData.EHitSourceType hitSourceType)
		{
			return hitSourceType == ModLoader.OnHitData.EHitSourceType.PlayerClick ||
				hitSourceType == ModLoader.OnHitData.EHitSourceType.PlayerProjectile ||
				hitSourceType == ModLoader.OnHitData.EHitSourceType.Misc;
		}

		[ModLoader.ModCallback (ModLoader.EModCallbackType.OnAutoSaveWorld, NAMESPACE + ".OnAutoSaveWorld")]
		public static void OnAutoSaveWorld()
		{
			Save();
			// TODO StatisticManager.Save();
		}

		[ModLoader.ModCallback (ModLoader.EModCallbackType.OnQuit, NAMESPACE + ".OnQuit")]
		public static void OnQuit()
		{
			// TODO StatisticManager.Save();
		}

	} // class

	// Helper function to save some lines of code
	public static class Helper
	{
		public static void TeleportPlayer(Players.Player target, Vector3 position)
		{
			using (ByteBuilder byteBuilder = ByteBuilder.Get()) {
				byteBuilder.Write(ClientMessageType.ReceivePosition);
				byteBuilder.Write(position);
				NetworkWrapper.Send(byteBuilder, target);
			}
		}
	}

} // namespace

