using Pipliz.Chatting;
using ChatCommands;
using Permissions;
using System;
using System.Text.RegularExpressions;
using NPC;
using Server.NPCs;

namespace ColonyCommands
{

	public class SpawnNpcCommand : IChatCommand
	{

		public bool IsCommand(string chat)
		{
			return (chat.Equals("/spawnnpc") || chat.StartsWith("/spawnnpc "));
		}

		public bool TryDoCommand(Players.Player causedBy, string chattext)
		{
			if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + "npcandbeds")) {
				return true;
			}

			var m = Regex.Match(chattext, @"/spawnnpc (?<amount>\d+)");
			if (!m.Success) {
				Chat.Send(causedBy, "Syntax: /spawnnpc {number}");
				return true;
			}
			int amount = 0;
			if (!int.TryParse(m.Groups["amount"].Value, out amount) || amount <= 0) {
				Chat.Send(causedBy, "Number should be > 0");
				return true;
			}

			Colony colony = Colony.Get(causedBy);
			Banner banner = BannerTracker.Get(causedBy);
			NPCType laborer = NPCType.GetByKeyNameOrDefault("pipliz.laborer");
			if (banner == null || colony == null) {
				Chat.Send(causedBy, "You need to place a banner to spawn in colonists");
				return true;
			}

			for (int i = 0; i < amount; i++) {
				NPCBase npc = new NPCBase(laborer, banner.KeyLocation.Vector, colony);
				ModLoader.TriggerCallbacks(ModLoader.EModCallbackType.OnNPCRecruited, npc);
			}
			colony.SendUpdate();

			Chat.Send(causedBy, $"Spawned {amount} colonists");
			return true;
		}
	}
}

