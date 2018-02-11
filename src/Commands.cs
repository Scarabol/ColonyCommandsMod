using System;
using System.IO;
using Pipliz;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public static class CommandsModEntries
  {
    public static string MOD_PREFIX = "mods.scarabol.commands.";
    public static string ModDirectory;

    [ModLoader.ModCallback (ModLoader.EModCallbackType.OnAssemblyLoaded, "scarabol.commands.assemblyload")]
    public static void OnAssemblyLoaded (string path)
    {
      ModDirectory = Path.GetDirectoryName (path);
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterStartup, "scarabol.commands.registercallbacks")]
    public static void AfterStartup ()
    {
      Pipliz.Log.Write ("Loaded Commands Mod 5.3.4 by Scarabol");
    }
  }

  public static class PlayerHelper
  {
    public static bool TryGetPlayer (string identifier, out Players.Player targetPlayer, out string error)
    {
      return TryGetPlayer (identifier, out targetPlayer, out error, false);
    }

    public static bool TryGetPlayer (string identifier, out Players.Player targetPlayer, out string error, bool includeOffline)
    {
      targetPlayer = null;
      if (identifier.StartsWith ("'")) {
        if (identifier.EndsWith ("'")) {
          identifier = identifier.Substring (1, identifier.Length - 2);
        } else {
          error = "missing ' after playername";
          return false;
        }
      }
      if (identifier.Length < 1) {
        error = "no playername given";
        return false;
      }
      ulong steamid;
      if (ulong.TryParse (identifier, out steamid)) {
        Steamworks.CSteamID csteamid = new Steamworks.CSteamID (steamid);
        if (csteamid.IsValid ()) {
          NetworkID networkId = new NetworkID (csteamid);
          error = "";
          if (Players.TryGetPlayer (networkId, out targetPlayer)) {
            return true;
          } else {
            targetPlayer = null;
          }
        }
      }
      int closestDist = int.MaxValue;
      Players.Player closestMatch = null;
      foreach (Players.Player player in Players.PlayerDatabase.ValuesAsList) {
        if (!player.IsConnected && !includeOffline) {
          continue;
        }
        if (player.Name != null) {
          if (string.Equals (player.Name, identifier, StringComparison.InvariantCultureIgnoreCase)) {
            if (targetPlayer == null) {
              targetPlayer = player;
            } else {
              targetPlayer = null;
              error = "duplicate player name, pls use SteamID";
              return false;
            }
          } else {
            int levDist = LevenshteinDistance.Compute (player.Name.ToLower (), identifier.ToLower ());
            if (levDist < closestDist) {
              closestDist = levDist;
              closestMatch = player;
            } else if (levDist == closestDist) {
              closestMatch = null;
            }
          }
        }
      }
      if (targetPlayer != null) {
        error = "";
        return true;
      } else if (closestMatch != null && (closestDist < closestMatch.Name.Length * 0.2)) {
        error = "";
        targetPlayer = closestMatch;
        Pipliz.Log.Write (string.Format ("Name '{0}' did not match, picked closest match '{1}' instead", identifier, targetPlayer.Name));
        return true;
      }
      error = "player not found";
      return false;
    }
  }

  // src: https://www.dotnetperls.com/levenshtein
  static class LevenshteinDistance
  {
    public static int Compute (string s, string t)
    {
      int n = s.Length;
      int m = t.Length;
      int[,] d = new int[n + 1, m + 1];
      if (n == 0) {
        return m;
      }
      if (m == 0) {
        return n;
      }
      for (int i = 0; i <= n; d [i, 0] = i++) {
      }
      for (int j = 0; j <= m; d [0, j] = j++) {
      }
      for (int i = 1; i <= n; i++) {
        for (int j = 1; j <= m; j++) {
          int cost = (t [j - 1] == s [i - 1]) ? 0 : 1;
          d [i, j] = System.Math.Min (
            System.Math.Min (d [i - 1, j] + 1, d [i, j - 1] + 1),
            d [i - 1, j - 1] + cost);
        }
      }
      return d [n, m];
    }
  }
}
