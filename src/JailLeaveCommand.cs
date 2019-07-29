using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Chatting;
using Chatting.Commands;
using TerrainGeneration;
using UnityEngine;

namespace ColonyCommands
{

  public class JailLeaveCommand : IChatCommand
  {

    public bool TryDoCommand(Players.Player causedBy, string chattext, List<string> splits)
    {
		if (!splits[0].Equals("/jailleave")) {
			return false;
		}
      Vector3 oldPos;
      if (JailManager.visitorPreviousPos.TryGetValue(causedBy, out oldPos)) {
        Helper.TeleportPlayer(causedBy, oldPos);
      } else {
        Chat.Send(causedBy, "Found no old position record, looks like you have to walk");
      }

      return true;
    }
  }
}

