using System.Collections.Generic;
using System.Text.RegularExpressions;
using Chatting;
using Chatting.Commands;

/*
 * Copy of Crone's top command
 */
namespace ColonyCommands
{

  public class TopChatCommand : IChatCommand
  {

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/top") || chat.StartsWith ("/top ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      var m = Regex.Match (chattext, @"/top (?<typename>.+)");
      if (!m.Success) {
        Chat.Send (causedBy, "Command didn't match, use /top [score|food|colonists|time|itemtypename]");
        return true;
      }
      string typename = m.Groups ["typename"].Value;

      List<Players.Player> players = new List<Players.Player> ();
      Players.PlayerDatabase.ForeachValue (x => players.Add (x));
      players.RemoveAll (x => string.IsNullOrEmpty (x.Name));

      // remove players that should be hidden from scoring
      players.RemoveAll(x => AntiGrief.UnscoredPlayers.Contains(x));

      if (typename.Equals ("score")) {
        players.Sort (delegate (Players.Player c1, Players.Player c2) {
          double c1score = Utility.CalculatePlayerScore (c1);
          double c2score = Utility.CalculatePlayerScore (c2);
          return c2score.CompareTo (c1score);
        });
        Chat.Send (causedBy, "##### Top Scores #####");
        int count = 1;
        if (players.Count > 10) {
          for (var i = 0; i < 10; i++) {
            Players.Player currentPlayer = players [i];
            var score = Utility.CalculatePlayerScore (currentPlayer);
            Chat.Send (causedBy, $"{i + 1}: {currentPlayer.Name} Score: {System.Math.Round (score, 1)}");
          }
        } else {
          foreach (var currentPlayer in players) {
            var score = Utility.CalculatePlayerScore (currentPlayer);
            Chat.Send (causedBy, $"{count++}: {currentPlayer.Name} Score: {System.Math.Round (score, 1)}");
          }
        }
      } else if (typename.Equals ("food")) {
        players.Sort (delegate (Players.Player c1, Players.Player c2) {
          double c1Food = Stockpile.GetStockPile (c1).TotalFood;
          double c2Food = Stockpile.GetStockPile (c2).TotalFood;
          return c2Food.CompareTo (c1Food);
        });
        Chat.Send (causedBy, "##### Top Food #####");
        int count = 1;
        if (players.Count > 10) {
          for (var i = 0; i < 10; i++) {
            Players.Player currentPlayer = players [i];
            var food = Stockpile.GetStockPile (currentPlayer).TotalFood;
            Chat.Send (causedBy, $"{i + 1}: {currentPlayer.Name} Food: {System.Math.Round (food, 1)}");
          }
        } else {
          foreach (var currentPlayer in players) {
            var food = Stockpile.GetStockPile (currentPlayer).TotalFood;
            Chat.Send (causedBy, $"{count++}: {currentPlayer.Name} Food: {System.Math.Round (food, 1)}");
          }
        }
      } else if (typename.Equals ("colonists")) {
        players.Sort (delegate (Players.Player c1, Players.Player c2) {
          double c1Colonists = Colony.Get (c1).FollowerCount;
          double c2Colonists = Colony.Get (c2).FollowerCount;
          return c2Colonists.CompareTo (c1Colonists);
        });
        Chat.Send (causedBy, "##### Top Colonists #####");
        int count = 1;
        if (players.Count > 10) {
          for (var i = 0; i < 10; i++) {
            Players.Player currentPlayer = players [i];
            var colonists = Colony.Get (currentPlayer).FollowerCount;
            Chat.Send (causedBy, $"{i + 1}: {currentPlayer.Name} Colonists: {colonists}");
          }
        } else {
          foreach (var currentPlayer in players) {
            var colonists = Colony.Get (currentPlayer).FollowerCount;
            Chat.Send (causedBy, $"{count++}: {currentPlayer.Name} Colonists: {colonists}");
          }
        }
      } else if (typename.Equals ("time")) {
        players.Sort (delegate (Players.Player c1, Players.Player c2) {
          long c1Time = ActivityTracker.GetOrCreateStats (c1.ID.ToStringReadable()).secondsPlayed;
          long c2Time = ActivityTracker.GetOrCreateStats (c2.ID.ToStringReadable()).secondsPlayed;
          return c2Time.CompareTo (c1Time);
        });
        Chat.Send (causedBy, "##### Top Time Played #####");
        int count = 1;
        if (players.Count > 10) {
          for (var i = 0; i < 10; i++) {
            Players.Player currentPlayer = players [i];
            var seconds = ActivityTracker.GetOrCreateStats (currentPlayer.ID.ToStringReadable()).secondsPlayed;
            var time = $"{seconds / 3600f:00}:{System.Math.Truncate ((seconds / 60f)) % 60f:00}:{seconds % 60f:00}";
            Chat.Send (causedBy, $"{i + 1}: {currentPlayer.Name} Time: {time}");
          }
        } else {
          foreach (var currentPlayer in players) {
            var seconds = ActivityTracker.GetOrCreateStats (currentPlayer.ID.ToStringReadable()).secondsPlayed;
            var time = $"{seconds / 3600f:00}:{System.Math.Truncate ((seconds / 60f)) % 60f:00}:{seconds % 60f:00}";
            Chat.Send (causedBy, $"{count++}: {currentPlayer.Name} Time: {time}");
          }
        }
      } else if (!typename.Contains (" ")) {
        ushort itemId;
        if (!ItemTypes.IndexLookup.TryGetIndex (typename, out itemId)) {
          Chat.Send (causedBy, $"There is no item called {typename}");
          return true;
        }
        players.Sort (delegate (Players.Player c1, Players.Player c2) {
          double c1ItemAmount = Stockpile.GetStockPile (c1).AmountContained (itemId);
          double c2ItemAmount = Stockpile.GetStockPile (c2).AmountContained (itemId);
          return c2ItemAmount.CompareTo (c1ItemAmount);
        });
        Chat.Send (causedBy, $"##### Top {typename} #####");
        int count = 1;
        if (players.Count > 10) {
          for (var i = 0; i < 10; i++) {
            Players.Player currentPlayer = players [i];
            var items = Stockpile.GetStockPile (currentPlayer).AmountContained (itemId);
            Chat.Send (causedBy, $"{i + 1}: {currentPlayer.Name} {typename}: {items}");
          }
        } else {
          foreach (var currentPlayer in players) {
            var items = Stockpile.GetStockPile (currentPlayer).AmountContained (itemId);
            Chat.Send (causedBy, $"{count++}: {currentPlayer.Name} {typename}: {items}");
          }
        }
      }
      return true;
    }
  }

  public static class Utility
  {
    public static double CalculatePlayerScore (Players.Player c1)
    {
      var colony = Colony.Get (c1);
      var colonists = colony.FollowerCount;
      double foodForDays;
      if (colonists != 0) {
        foodForDays = Stockpile.GetStockPile (c1).TotalFood / System.Math.Round ((colony.FoodUsePerHour * colonists) * 24, 1);
      } else {
        foodForDays = 0;
      }
      return (DiminishingReturns (foodForDays, 1)) * (colonists);
    }

    public static double DiminishingReturns (double val, double scale)
    {
      if (val < 0) {
        return -DiminishingReturns (-val, scale);
      }
      var mult = val / scale;
      var trinum = (System.Math.Sqrt (8.0 * mult + 1.0) - 1.0) / 2.0;
      return trinum * scale;
    }
  }
}
