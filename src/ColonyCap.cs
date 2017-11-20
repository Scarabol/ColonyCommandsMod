using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using System.Threading;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class ColonyCap : ChatCommands.IChatCommand
  {
    private static int maxNumberOfColonistsPerColony = -1;
    private static int checkIntervalSeconds = 30;

    private static string ConfigFilepath {
      get {
        return Path.Combine (Path.Combine ("gamedata", "savegames"), Path.Combine (ServerManager.WorldName, "colonycap.json"));
      }
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.colonycap.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      ChatCommands.CommandManager.RegisterCommand (new ColonyCap ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/colonycap") || chat.StartsWith ("/colonycap ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "colonycap")) {
          return true;
        }
        var m = Regex.Match (chattext, @"/colonycap (?<colonistslimit>-?\d+)( (?<checkintervalseconds>\d+))?");
        if (!m.Success) {
          Chat.Send (causedBy, "Command didn't match, use /colonycap [colonistslimit] [checkintervalseconds]");
          return true;
        }
        string strLimit = m.Groups ["colonistslimit"].Value;
        if (strLimit.Length < 1) {
          Chat.Send (causedBy, "No limit given; use /colonycap [colonistslimit] [checkintervalseconds]");
          return true;
        }
        int limit;
        if (!int.TryParse (strLimit, out limit)) {
          Chat.Send (causedBy, "Could not parse limit");
          return true;
        }
        maxNumberOfColonistsPerColony = limit;
        if (maxNumberOfColonistsPerColony >= 0) {
          Chat.SendToAll (string.Format ("Colony population limit set to {0}", maxNumberOfColonistsPerColony));
        } else {
          Chat.SendToAll (string.Format ("Colony population limit disabled"));
        }
        string strInterval = m.Groups ["checkintervalseconds"].Value;
        if (strInterval.Length > 0) {
          int interval;
          if (!int.TryParse (strInterval, out interval)) {
            Chat.Send (causedBy, "Could not parse interval");
            return true;
          }
          checkIntervalSeconds = System.Math.Max (1, interval);
          Chat.Send (causedBy, string.Format ("Check interval seconds set to {0}", checkIntervalSeconds));
        }
        Save ();
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterWorldLoad, "scarabol.commands.colonycap.starttimers")]
    public static void AfterWorldLoad ()
    {
      Load ();
      new Thread (() => {
        Thread.CurrentThread.IsBackground = true;
        while (true) {
          try {
            int cachedLimit = maxNumberOfColonistsPerColony;
            if (cachedLimit >= 0) {
              Players.PlayerDatabase.ForeachValue (player => {
                Colony colony = Colony.Get (player);
                while (colony.FollowerCount > cachedLimit) {
                  Chat.Send (player, string.Format ("<color=red>Colonists are dieing, because of overpopulation. Limit is {0}</color>", cachedLimit));
                  if (colony.LaborerCount > 0) {
                    colony.RemoveNPC (colony.FindLaborer ());
                  } else {
                    for (int c = 0; c < 8; c++) { // colony takes 8 hits to kill a colonist
                      colony.TakeMonsterHit (0, int.MaxValue);
                    }
                  }
                  Thread.Sleep (1000);
                }
              });
            }
          } catch (Exception exception) {
            Pipliz.Log.WriteError (string.Format ("Exception in cap loop; {0}", exception.Message));
          }
          Thread.Sleep (checkIntervalSeconds * 1000);
        }
      }).Start ();
    }

    public static void Load ()
    {
      try {
        JSONNode json;
        if (JSON.Deserialize (ConfigFilepath, out json, false)) {
          int maxNumber;
          if (json.TryGetAs ("maxNumberOfColonistsPerColony", out maxNumber)) {
            maxNumberOfColonistsPerColony = maxNumber;
          }
          int intervalSeconds;
          if (json.TryGetAs ("checkIntervalSeconds", out intervalSeconds)) {
            checkIntervalSeconds = System.Math.Max (1, intervalSeconds);
          }
        }
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while loading colonycap; {0}", exception.Message));
      }
    }

    private static void Save ()
    {
      try {
        JSONNode json = new JSONNode ();
        json.SetAs ("maxNumberOfColonistsPerColony", maxNumberOfColonistsPerColony);
        json.SetAs ("checkIntervalSeconds", checkIntervalSeconds);
        JSON.Serialize (ConfigFilepath, json, 3);
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while saving colonycap; {0}", exception.Message));
      }
    }
  }
}
