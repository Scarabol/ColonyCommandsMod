using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using BlockTypes.Builtin;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class DrainChatCommand : ChatCommands.IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.drain.registercommand")]
    public static void AfterItemTypesDefined ()
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
        List<Vector3Int> toCheckWaterBlocks = new List<Vector3Int> () { };
        foreach (Vector3Int toCheckPosition in new Vector3Int[] {
          new Vector3Int(causedBy.Position),
          new Vector3Int(causedBy.Position).Add(0, 1, 0),
          new Vector3Int(causedBy.Position).Add(0, 2, 0)
        }) {
          ushort actualType;
          if (!World.TryGetTypeAt (toCheckPosition, out actualType)) {
            Chat.Send (causedBy, $"Could not get the item type at {toCheckPosition}");
          } else if (actualType == BuiltinBlocks.Water) {
            toCheckWaterBlocks.Add (toCheckPosition);
          }
        }
        if (toCheckWaterBlocks.Count < 1) {
          Chat.Send (causedBy, $"No water found at {causedBy.Position}. Please get your feet wet!");
          return true;
        }
        List<Vector3Int> toCleanBlocks = new List<Vector3Int> ();
        while (toCheckWaterBlocks.Count > 0 && toCleanBlocks.Count < 100000) {
          Vector3Int currentOrigin = toCheckWaterBlocks [0];
          toCheckWaterBlocks.RemoveAt (0);
          foreach (Vector3Int toCheckPosition in new List<Vector3Int> {
              new Vector3Int (-1, 0, 0), new Vector3Int (1, 0, 0),
              new Vector3Int (0, -1, 0) , new Vector3Int (0, 1, 0),
              new Vector3Int (0, 0, -1), new Vector3Int (0, 0, 1)
            }) {
            Vector3Int absCheck = currentOrigin + toCheckPosition;
            ushort type;
            if (World.TryGetTypeAt (absCheck, out type) && type == BuiltinBlocks.Water) {
              ServerManager.TryChangeBlock (absCheck, BuiltinBlocks.LeavesTemperate);
              toCheckWaterBlocks.Add (absCheck);
              toCleanBlocks.Add (absCheck);
            }
          }
        }
        Chat.Send (causedBy, string.Format ("Replaced {0} water blocks. Start cleaning up...", toCleanBlocks.Count));
        foreach (Vector3Int AbsRemove in toCheckWaterBlocks) {
          ServerManager.TryChangeBlock (AbsRemove, BuiltinBlocks.Air);
        }
        foreach (Vector3Int AbsRemove in toCleanBlocks) {
          ServerManager.TryChangeBlock (AbsRemove, BuiltinBlocks.Air);
        }
      } catch (Exception exception) {
        Pipliz.Log.WriteError (string.Format ("Exception while parsing command; {0}", exception.Message));
      }
      return true;
    }
  }
}
