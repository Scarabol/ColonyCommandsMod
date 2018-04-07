using System;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using ChatCommands;
using Permissions;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class ColonyCap : IChatCommand
  {
    static int MaxNumberOfColonistsPerColony = -1;
    static int CheckIntervalSeconds = 30;

    static string ConfigFilepath {
      get {
        return Path.Combine (Path.Combine ("gamedata", "savegames"), Path.Combine (ServerManager.WorldName, "colonycap.json"));
      }
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.colonycap.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      CommandManager.RegisterCommand (new ColonyCap ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/colonycap") || chat.StartsWith ("/colonycap ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "colonycap")) {
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
      MaxNumberOfColonistsPerColony = limit;
      if (MaxNumberOfColonistsPerColony >= 0) {
        Chat.SendToAll ($"Colony population limit set to {MaxNumberOfColonistsPerColony}");
      } else {
        Chat.SendToAll ("Colony population limit disabled");
      }
      string strInterval = m.Groups ["checkintervalseconds"].Value;
      if (strInterval.Length > 0) {
        int interval;
        if (!int.TryParse (strInterval, out interval)) {
          Chat.Send (causedBy, "Could not parse interval");
          return true;
        }
        CheckIntervalSeconds = System.Math.Max (1, interval);
        Chat.Send (causedBy, $"Check interval seconds set to {CheckIntervalSeconds}");
      }
      Save ();
      return true;
    }

    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterWorldLoad, "scarabol.commands.colonycap.starttimers")]
    public static void AfterWorldLoad ()
    {
      Load ();
      new Thread (() => {
        Thread.CurrentThread.IsBackground = true;
        System.Random rnd = new System.Random ();
        while (true) {
          try {
            int cachedLimit = MaxNumberOfColonistsPerColony;
            if (cachedLimit >= 0) {
              bool killed;
              do {
                killed = false;
                Players.PlayerDatabase.ForeachValue (player => {
                  Colony colony = Colony.Get (player);
                  if (colony.FollowerCount > cachedLimit) {
                    Chat.Send (player, $"<color=red>Colonists are dieing, because of overpopulation. Limit is {cachedLimit}</color>");
                    if (colony.LaborerCount > 0) {
                      colony.FindLaborer ().OnDeath ();
                    } else {
                      colony.Followers [rnd.Next (colony.Followers.Count)].OnDeath ();
                    }
                    killed = true;
                  }
                });
                Thread.Sleep (1000);
              } while (killed);
            }
          } catch (Exception exception) {
            Log.WriteError ($"Exception in cap loop; {exception.Message}");
          }
          Thread.Sleep (CheckIntervalSeconds * 1000);
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
            MaxNumberOfColonistsPerColony = maxNumber;
          }
          int intervalSeconds;
          if (json.TryGetAs ("checkIntervalSeconds", out intervalSeconds)) {
            CheckIntervalSeconds = System.Math.Max (1, intervalSeconds);
          }
        }
      } catch (Exception exception) {
        Log.WriteError ($"Exception while loading colonycap; {exception.Message}");
      }
    }

    static void Save ()
    {
      try {
        JSONNode json = new JSONNode ();
        json.SetAs ("maxNumberOfColonistsPerColony", MaxNumberOfColonistsPerColony);
        json.SetAs ("checkIntervalSeconds", CheckIntervalSeconds);
        JSON.Serialize (ConfigFilepath, json, 3);
      } catch (Exception exception) {
        Log.WriteError ($"Exception while saving colonycap; {exception.Message}");
      }
    }
  }
}
