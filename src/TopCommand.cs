using System.Collections.Generic;
using System.Linq;
using Chatting;
using Chatting.Commands;

/*
 * Copy of Crone's top command
 */
namespace ColonyCommands
{

	public class TopChatCommand : IChatCommand
	{

		public enum ECalctype {
			Colony,
			Player
		}

		public enum EScoreType {
			HappinessScore,
			Food,
			Colonists,
			Item,
			TimePlayed
		}

		public bool TryDoCommand(Players.Player causedBy, string chattext, List<string> splits)
		{
			if (!splits[0].Equals("/top")) {
				return false;
			}
			if (splits.Count < 2) {
				Chat.Send(causedBy, "Syntax: /top [c|colony|p|player] {score|food|colonists|time|itemtypename}");
				return true;
			}
			string typename = splits[1];

			ECalctype calcType = ECalctype.Colony;
			if (splits.Count > 2) {
				if (splits[1].Equals("c") || splits[1].Equals("colony")) {
					calcType = ECalctype.Colony;
				} else if (splits[1].Equals("p") || splits[1].Equals("player")) {
					calcType = ECalctype.Player;
				} else {
					Chat.Send(causedBy, "Syntax: /top [c|colony|p|player] {score|food|colonists|time|itemtypename}");
					return true;
				}
				typename = splits[2];
			}

			List<Players.Player> players = new List<Players.Player>();
			foreach (KeyValuePair<NetworkID, Players.Player> item in Players.PlayerDatabase) {
				// remove players that should be hidden from scoring
				if (AntiGrief.UnscoredPlayers.Contains(item.Value)) {
					continue;
				}
				// and also empty ones
				if (string.IsNullOrEmpty(item.Value.Name)) {
					continue;
				}
				players.Add(item.Value);
			}

			Dictionary<string, int> results;
			EScoreType scoreType;
			if (typename.Equals("score")) {
				scoreType = EScoreType.HappinessScore;
				results = ScoreColonies(players, calcType, scoreType);

			} else if (typename.Equals("food")) {
				scoreType = EScoreType.Food;
				results = ScoreColonies(players, calcType, scoreType);

			} else if (typename.Equals("colonists")) {
				scoreType = EScoreType.Colonists;
				results = ScoreColonies(players, calcType, scoreType);

			} else if (typename.Equals("time")) {
				scoreType = EScoreType.TimePlayed;
				results = ScoreByTime(players);

			} else {
				scoreType = EScoreType.Item;
				ushort itemId;
				if (!ItemTypes.IndexLookup.TryGetIndex(typename, out itemId)) {
					Chat.Send(causedBy, $"There is no item called {typename}");
					return true;
				}
				results = ScoreColonies(players, calcType, EScoreType.Item, itemId);
			}

			// pretty string for output
			while (typename.Contains(".")) {
				typename = typename.Substring(typename.IndexOf(".") + 1);
			}
			typename = typename.Substring(0,1).ToUpper() + typename.Substring(1);

			List<KeyValuePair<string, int>> sortedResult = results.ToList();
			sortedResult.Sort(delegate(KeyValuePair<string, int> kvp1, KeyValuePair<string, int> kvp2) {
				return kvp1.Value.CompareTo(kvp2.Value);
			});

			Chat.Send(causedBy, $"##### Top {typename} #####");
			for (int i = 0; i < 10 && i < sortedResult.Count; i++) {
				string val = sortedResult[i].Value.ToString();
				if (scoreType == EScoreType.TimePlayed) {
					val = $"{System.Math.Truncate(sortedResult[i].Value / 60f) % 60f:00}h:{sortedResult[i].Value % 60f:00}m";
				}
				Chat.Send(causedBy, $"{i + 1}: {sortedResult[i].Key}   {val}");
			}

			return true;
		}

		public Dictionary<string, int> ScoreColonies(List<Players.Player> players, ECalctype calcType, EScoreType scoreType, ushort item = 0)
		{
			Dictionary<Colony, int> colonyresults = new Dictionary<Colony, int>();
			foreach (Colony colony in ServerManager.ColonyTracker.ColoniesByID.Values) {
				if (colony.Owners.Any(a => players.Contains(a))) {
					int score = 0;
					if (scoreType == EScoreType.HappinessScore) {
						score = (int)(colony.HappinessData.CachedHappiness * colony.FollowerCount);
					} else if (scoreType == EScoreType.Food) {
						score = (int)colony.Stockpile.TotalFood;
					} else if (scoreType == EScoreType.Colonists) {
						score = colony.FollowerCount;
					} else if (scoreType == EScoreType.Item) {
						score = colony.Stockpile.AmountContained(item);
					}
					colonyresults[colony] = score;
				}
			}

			Dictionary<string, int> results = new Dictionary<string, int>();
			if (calcType == ECalctype.Colony) {
				foreach (Colony col in colonyresults.Keys) {
					results[col.Name] = colonyresults[col];
				}
			} else {
				foreach (Colony col in colonyresults.Keys) {
					foreach (Players.Player owner in col.Owners) {
						if (!results.ContainsKey(owner.Name)) {
							results[owner.Name] = colonyresults[col];
						} else {
							results[owner.Name] += colonyresults[col];
						}
					}
				}
			}

			return results;
		}

		// time played is always player based, no colony version needed
		public Dictionary<string, int> ScoreByTime(List<Players.Player> players)
		{
			Dictionary<string, int> results = new Dictionary<string, int>();
			foreach (Players.Player player in players) {
				long seconds = ActivityTracker.GetOrCreateStats(player.ID.ToStringReadable()).secondsPlayed;
				results[player.Name] = (int)(seconds / 60);
			}
			return results;
		}

	}

}
