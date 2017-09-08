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
  public class WarpChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesServer, "scarabol.commands.warp.registercommand")]
    public static void AfterItemTypesServer ()
    {
      ChatCommands.CommandManager.RegisterCommand (new WarpChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/warp") || chat.StartsWith ("/warp ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "warp")) {
          return true;
        }
        var m = Regex.Match (chattext, @"/warp (?<targetplayername>['].+?[']|[^ ]+)( (?<teleportplayername>['].+?[']|[^ ]+))?");
        if (!m.Success) {
          Chat.Send (causedBy, "Command didn't match, use /warp [targetplayername] or /warp [targetplayername] [teleportplayername]");
          return true;
        }
        string TargetPlayerName = m.Groups ["targetplayername"].Value;
        Players.Player TargetPlayer;
        string Error;
        if (!PlayerHelper.TryGetPlayer (TargetPlayerName, out TargetPlayer, out Error)) {
          Chat.Send (causedBy, string.Format ("Could not find target player '{0}'; {1}", TargetPlayerName, Error));
          return true;
        }
        Players.Player TeleportPlayer = causedBy;
        string TeleportPlayerName = m.Groups ["teleportplayername"].Value;
        if (TeleportPlayerName.Length > 0) {
          if (!PlayerHelper.TryGetPlayer (TeleportPlayerName, out TeleportPlayer, out Error)) {
            Chat.Send (causedBy, string.Format ("Could not find teleport player '{0}'; {1}", TeleportPlayerName, Error));
            return true;
          }
        }
        ChatCommands.Implementations.Teleport.TeleportTo (TeleportPlayer, TargetPlayer.Position);
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }

  [ModLoader.ModManager]
  public class WarpBannerChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesServer, "scarabol.commands.warpbanner.registercommand")]
    public static void AfterItemTypesServer ()
    {
      ChatCommands.CommandManager.RegisterCommand (new WarpBannerChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/warpbanner") || chat.StartsWith ("/warpbanner ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "warp")) {
          return true;
        }
        var m = Regex.Match (chattext, @"/warpbanner (?<targetplayername>['].+?[']|[^ ]+)( (?<teleportplayername>['].+?[']|[^ ]+))?");
        if (!m.Success) {
          Chat.Send (causedBy, "Command didn't match, use /warpbanner [targetplayername] or /warpbanner [targetplayername] [teleportplayername]");
          return true;
        }
        string TargetPlayerName = m.Groups ["targetplayername"].Value;
        Banner TargetBanner = null;
        int closestDist = int.MaxValue;
        Banner closestMatch = null;
        foreach (Banner banner in BannerTracker.GetBanners()) {
          if (banner.Owner.Name.ToLower ().Equals (TargetPlayerName.ToLower ())) {
            if (TargetBanner == null) {
              TargetBanner = banner;
            } else {
              Chat.Send (causedBy, string.Format ("Duplicate player name", TargetPlayerName));
            }
          } else {
            int levDist = LevenshteinDistance.Compute (banner.Owner.Name.ToLower (), TargetPlayerName.ToLower ());
            if (levDist < closestDist) {
              closestDist = levDist;
              closestMatch = banner;
            } else if (levDist == closestDist) {
              closestMatch = null;
            }
          }
        }
        if (TargetBanner == null) {
          if (closestMatch != null) {
            TargetBanner = closestMatch;
            Pipliz.Log.Write (string.Format ("Name '{0}' did not match, picked closest match '{1}' instead", TargetPlayerName, TargetBanner.Owner.Name));
          } else {
            Chat.Send (causedBy, string.Format ("Banner not found for '{0}'", TargetPlayerName));
            return true;
          }
        }
        Players.Player TeleportPlayer = causedBy;
        string TeleportPlayerName = m.Groups ["teleportplayername"].Value;
        if (TeleportPlayerName.Length > 0) {
          string Error;
          if (!PlayerHelper.TryGetPlayer (TeleportPlayerName, out TeleportPlayer, out Error)) {
            Chat.Send (causedBy, string.Format ("Could not find teleport player '{0}'; {1}", TeleportPlayerName, Error));
            return true;
          }
        }
        ChatCommands.Implementations.Teleport.TeleportTo (TeleportPlayer, TargetBanner.KeyLocation.Vector);
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }

  [ModLoader.ModManager]
  public class WarpSpawnChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesServer, "scarabol.commands.warpspawn.registercommand")]
    public static void AfterItemTypesServer ()
    {
      ChatCommands.CommandManager.RegisterCommand (new WarpSpawnChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/warpspawn") || chat.StartsWith ("/warpspawn ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "warp")) {
          return true;
        }
        var m = Regex.Match (chattext, @"/warpspawn( ?<teleportplayername>['].+?[']|[^ ]+)?");
        if (!m.Success) {
          Chat.Send (causedBy, "Command didn't match, use /warpspawn [teleportplayername] or /warpspawn [teleportplayername]");
          return true;
        }
        Players.Player TeleportPlayer = causedBy;
        string TeleportPlayerName = m.Groups ["teleportplayername"].Value;
        if (TeleportPlayerName.Length > 0) {
          string Error;
          if (!PlayerHelper.TryGetPlayer (TeleportPlayerName, out TeleportPlayer, out Error)) {
            Chat.Send (causedBy, string.Format ("Could not find teleport player '{0}'; {1}", TeleportPlayerName, Error));
            return true;
          }
        }
        ChatCommands.Implementations.Teleport.TeleportTo (TeleportPlayer, TerrainGenerator.GetSpawnLocation ().Vector);
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }
}
