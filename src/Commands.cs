using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using Pipliz.Threading;
using Pipliz.APIProvider.Recipes;
using Pipliz.APIProvider.Jobs;
using NPC;

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
      Pipliz.Log.Write ("Loaded Commands Mod 0.6.9 by Scarabol");
    }
  }

  public static class PlayerHelper
  {
    public static bool TryGetPlayer (string identifier, out Players.Player targetPlayer, out string error)
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
      for (int c = 0; c < Players.CountConnected; c++) {
        Players.Player player = Players.GetConnectedByIndex (c);
        if (player.Name != null && player.Name.ToLower ().Equals (identifier.ToLower ())) {
          if (targetPlayer == null) {
            targetPlayer = player;
          } else {
            targetPlayer = null;
            error = "duplicate player name, pls use SteamID";
            return false;
          }
        }
      }
      if (targetPlayer != null) {
        error = "";
        return true;
      }
      error = "player not found";
      return false;
    }
  }
}
