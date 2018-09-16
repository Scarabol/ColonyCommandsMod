using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Pipliz;
using Chatting;
using Chatting.Commands;
using BlockTypes;
using BlockEntities.Implementations;

namespace ColonyCommands
{

	public class BedsCommand : IChatCommand
	{

		public static List<ushort> bedBlocks = new List<ushort>{
			BuiltinBlocks.BedZN, BuiltinBlocks.BedXP,
			BuiltinBlocks.BedZP, BuiltinBlocks.BedXN,
			BuiltinBlocks.BedEndZN, BuiltinBlocks.BedEndXP,
			BuiltinBlocks.BedEndZP, BuiltinBlocks.BedEndXN
		};

		public bool IsCommand(string chat)
		{
			return (chat.Equals("/beds") || chat.StartsWith("/beds "));
		}

		public bool TryDoCommand(Players.Player causedBy, string chattext)
		{
			if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + "beds")) {
				return true;
			}

			var m = Regex.Match(chattext, @"/beds (?<amount>\d+)");
			if (!m.Success) {
				Chat.Send(causedBy, "Syntax: /beds {number}");
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

			Chat.Send(causedBy, $"Placing {amount} beds around you");
			int radius = 3;
			Vector3Int pos = banner.Position.Add(-radius + 1, 0, -radius);
			int counterX = -radius + 1, counterZ = -radius;
			int bedUsed = 0;
			int stepX = 1, stepZ = 0;
			while (amount > 0) {
				ushort blockType;
				if (World.TryGetTypeAt(pos, out blockType) && blockType == BuiltinBlocks.Air
					&& ServerManager.TryChangeBlock(pos, bedBlocks[bedUsed], causedBy)) {
					--amount;
				}
				if (counterX == -radius && counterZ == -radius) {
					stepX = 1;
					stepZ = 0;
					radius += 2;
					counterZ = -radius;
					counterX = -radius;
					bedUsed = 0;
					pos = pos.Add(-2, 0, -2);
				} else if (counterX == radius && counterZ == -radius) {
					stepX = 0;
					stepZ = 1;
					bedUsed = 1;
				} else if (counterX == radius && counterZ == radius) {
					stepX = -1;
					stepZ = 0;
					bedUsed = 2;
				} else if (counterX == -radius && counterZ == radius) {
					stepX = 0;
					stepZ = -1;
					bedUsed = 3;
				}
				counterX += stepX;
				counterZ += stepZ;
				pos = pos.Add(stepX, 0, stepZ);
			}

			return true;
		}
	}
}

