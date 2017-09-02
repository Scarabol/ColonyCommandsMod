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
  public class DrainChatCommand : ChatCommands.IChatCommand
  {
    public bool IsCommand (string chat)
    {
      return chat.Equals ("/drain") || chat.StartsWith ("/drain ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      try {
        if (causedBy == null || !Permissions.PermissionsManager.CheckAndWarnPermission (causedBy, "mods.scarabol.commands.drain")) {
          return true;
        }
        var m = Regex.Match (chattext, @"/drain( (?<startx>-?\d+) (?<starty>-?\d+) (?<startz>-?\d+))?");
        if (!m.Success) {
          Chat.Send (causedBy, "Command didn't match, use /drain or /drain [startx] [starty] [startz]");
          return true;
        }
        Vector3Int Origin = new Vector3Int (causedBy.Position);
        ushort ActualType;
        if (!World.TryGetTypeAt (Origin, out ActualType)) {
          Chat.Send (causedBy, string.Format ("Could not get the item type at {0}", Origin));
        } else if (ActualType != BlockTypes.Builtin.BuiltinBlocks.Water) {
          Chat.Send (causedBy, string.Format ("No water found at {0}", Origin));
        } else {
          List<Vector3Int> ToClean = new List<Vector3Int> ();
          List<Vector3Int> CurrentWaters = new List<Vector3Int> () { Origin };
          while (CurrentWaters.Count > 0) {
            Vector3Int CurrentOrigin = CurrentWaters [0];
            CurrentWaters.RemoveAt (0);
            ushort Type;
            foreach (Vector3Int ToCheck in new List<Vector3Int> {
              new Vector3Int (-1, 0, 0), new Vector3Int (1, 0, 0),
              new Vector3Int (0, -1, 0) , new Vector3Int (0, 1, 0),
              new Vector3Int (0, 0, -1), new Vector3Int (0, 0, 1)
            }) {
              Vector3Int AbsCheck = CurrentOrigin + ToCheck;
              if (World.TryGetTypeAt (AbsCheck, out Type) && Type == BlockTypes.Builtin.BuiltinBlocks.Water) {
                ServerManager.TryChangeBlock (AbsCheck, BlockTypes.Builtin.BuiltinBlocks.LeavesTemperate);
                CurrentWaters.Add (AbsCheck);
                ToClean.Add (AbsCheck);
              }
            }
          }
          Chat.Send (causedBy, string.Format ("Replaced {0} water blocks. Start cleaning up...", ToClean.Count));
          ServerManager.TryChangeBlock (Origin, BlockTypes.Builtin.BuiltinBlocks.Air);
          foreach (Vector3Int AbsRemove in ToClean) {
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
