using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class DrainChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesServer, "scarabol.commands.drain.registercommand")]
    public static void AfterItemTypesServer ()
    {
      ChatCommands.CommandManager.RegisterCommand (new DrainChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/drain");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        if (!Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "drain")) {
          return true;
        }
        Vector3Int origin = new Vector3Int (causedBy.Position);
        ushort actualType;
        if (!World.TryGetTypeAt (origin, out actualType)) {
          Chat.Send (causedBy, string.Format ("Could not get the item type at {0}", origin));
        } else if (actualType != BlockTypes.Builtin.BuiltinBlocks.Water) {
          Chat.Send (causedBy, string.Format ("No water found at {0}", origin));
        } else {
          List<Vector3Int> toClean = new List<Vector3Int> ();
          List<Vector3Int> currentWaters = new List<Vector3Int> () { origin };
          while (currentWaters.Count > 0 && toClean.Count < 100000) {
            Vector3Int currentOrigin = currentWaters [0];
            currentWaters.RemoveAt (0);
            foreach (Vector3Int toCheck in new List<Vector3Int> {
              new Vector3Int (-1, 0, 0), new Vector3Int (1, 0, 0),
              new Vector3Int (0, -1, 0) , new Vector3Int (0, 1, 0),
              new Vector3Int (0, 0, -1), new Vector3Int (0, 0, 1)
            }) {
              Vector3Int absCheck = currentOrigin + toCheck;
              ushort type;
              if (World.TryGetTypeAt (absCheck, out type) && type == BlockTypes.Builtin.BuiltinBlocks.Water) {
                ServerManager.TryChangeBlock (absCheck, BlockTypes.Builtin.BuiltinBlocks.LeavesTemperate);
                currentWaters.Add (absCheck);
                toClean.Add (absCheck);
              }
            }
          }
          Chat.Send (causedBy, string.Format ("Replaced {0} water blocks. Start cleaning up...", toClean.Count));
          ServerManager.TryChangeBlock (origin, BlockTypes.Builtin.BuiltinBlocks.Air);
          foreach (Vector3Int AbsRemove in toClean) {
            ServerManager.TryChangeBlock (AbsRemove, BlockTypes.Builtin.BuiltinBlocks.Air);
          }
        }
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }
}
