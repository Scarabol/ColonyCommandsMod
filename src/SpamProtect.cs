using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Chatting;
using Chatting.Commands;

namespace ColonyCommands
{
	public static class MuteList
	{
		public static Dictionary<Players.Player, long> MutedMinutes = new Dictionary<Players.Player, long>();

		public static void Update()
		{
			var tmp = new Dictionary<Players.Player, long> (MutedMinutes);
			foreach (var entry in tmp) {
				if (entry.Value < DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) {
					MutedMinutes.Remove(entry.Key);
					Log.Write($"Unmuted {entry.Key.Name}");
				}
			}
		}
	}

	public class MuteChatCommand : IChatCommand
	{

		public bool TryDoCommand(Players.Player causedBy, string chattext, List<string> splits)
		{
			if (!splits[0].Equals("/mute")) {
				return false;
			}
			if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + "mute")) {
				return true;
			}
			var m = Regex.Match(chattext, @"/mute (?<targetplayername>['].+[']|[^ ]+) (?<minutes>\d+)");
			if (!m.Success) {
				Chat.Send(causedBy, "Syntax: /mute [playername] [minutes]");
				return true;
			}
			string targetPlayerName = m.Groups["targetplayername"].Value;
			Players.Player targetPlayer;
			string error;
			if (!PlayerHelper.TryGetPlayer(targetPlayerName, out targetPlayer, out error)) {
				Chat.Send(causedBy, $"Could not find target player '{targetPlayerName}': {error}");
				return true;
			}
			long minutes;
			if (!long.TryParse(m.Groups["minutes"].Value, out minutes)) {
				Chat.Send(causedBy, "Could not read minutes value");
				return true;
			}
			if (MuteList.MutedMinutes.ContainsKey(targetPlayer)) {
				MuteList.MutedMinutes.Remove(targetPlayer);
			}
			MuteList.MutedMinutes.Add(targetPlayer, DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + minutes * 60 * 1000);
			Chat.Send(targetPlayer, $"You're muted for {minutes} Minute(s)");
			Log.Write($"{targetPlayer.Name} muted for {minutes} Minute(s)");
			Chat.Send(causedBy, $"{targetPlayer.Name} muted for {minutes} Minute(s)");
			return true;
		}
	}

	public class UnmuteChatCommand : IChatCommand
	{
		public bool TryDoCommand(Players.Player causedBy, string chattext, List<string> splits)
		{
			if (!splits[0].Equals("/unmute")) {
				return false;
			}
			if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + "mute")) {
				return true;
			}
			var m = Regex.Match(chattext, @"/unmute (?<targetplayername>['].+[']|[^ ]+)");
			if (!m.Success) {
				Chat.Send(causedBy, "Syntax: /unmute [targetplayername]");
				return true;
			}
			var targetPlayerName = m.Groups["targetplayername"].Value;
			Players.Player targetPlayer;
			string error;
			if (!PlayerHelper.TryGetPlayer(targetPlayerName, out targetPlayer, out error)) {
				Chat.Send(causedBy, $"Could not find target player '{targetPlayerName}': {error}");
				return true;
			}
			if (MuteList.MutedMinutes.ContainsKey(targetPlayer)) {
				MuteList.MutedMinutes.Remove(targetPlayer);
				Log.Write ($"Unmuted {targetPlayer.Name}");
			} else {
				Chat.Send(causedBy, $"{targetPlayer.Name} was not muted");
			}
			return true;
		}
	}
}
