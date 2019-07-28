using Chatting;
using Chatting.Commands;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NPC;
using BlockEntities.Implementations;

namespace ColonyCommands
{

	public class SpawnNpcCommand : IChatCommand
	{

		public bool IsCommand(string chat)
		{
			return (chat.Equals("/spawnnpc") || chat.StartsWith("/spawnnpc "));
		}

		public bool TryDoCommand(Players.Player causedBy, string chattext, List<string> splits)
		{
			if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + "npcandbeds")) {
				return true;
			}

			var m = Regex.Match(chattext, @"/spawnnpc (?<amount>\d+) ?(?<player>['].+[']|[^ ]+)?");
			if (!m.Success) {
				Chat.Send(causedBy, "Syntax: /spawnnpc {number} [targetplayer]");
				return true;
			}
			int amount = 0;
			if (!int.TryParse(m.Groups["amount"].Value, out amount) || amount <= 0) {
				Chat.Send(causedBy, "Number should be > 0");
				return true;
			}

			Colony colony = causedBy.ActiveColony;
			if (colony == null) {
				Chat.Send(causedBy, "You need to be at an active colony to spawn beds");
				return true;
			}
			BannerTracker.Banner banner = colony.GetClosestBanner(causedBy.VoxelPosition);
			if (banner == null) {
				Chat.Send(causedBy, "No banners found for the active colony");
				return true;
			}

			// NPCType laborer = NPCType.GetByKeyNameOrDefault("pipliz.laborer");
			for (int i = 0; i < amount; i++) {
				// NPCBase npc = new NPCBase(colony, banner.Position);
				// ModLoader.TriggerCallbacks(ModLoader.EModCallbackType.OnNPCRecruited, npc);
			}
			// colony.SendUpdate();

			Chat.Send(causedBy, $"Spawned {amount} colonists");
			return true;
		}
	}
}

