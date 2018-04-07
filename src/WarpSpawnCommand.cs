using System.Text.RegularExpressions;
using Pipliz.Chatting;
using ChatCommands;
using Permissions;
using Server.TerrainGeneration;
using ChatCommands.Implementations;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class WarpSpawnChatCommand : IChatCommand
  {
    [ModLoader.ModCallback (ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.commands.warpspawn.registercommand")]
    public static void AfterItemTypesDefined ()
    {
      CommandManager.RegisterCommand (new WarpSpawnChatCommand ());
    }

    public bool IsCommand (string chat)
    {
      return chat.Equals ("/warpspawn") || chat.StartsWith ("/warpspawn ");
    }

    public bool TryDoCommand (Players.Player causedBy, string chattext)
    {
      if (!PermissionsManager.CheckAndWarnPermission (causedBy, CommandsModEntries.MOD_PREFIX + "warp.spawn")) {
        return true;
      }
      var m = Regex.Match (chattext, @"/warpspawn( ?<teleportplayername>['].+?[']|[^ ]+)?");
      if (!m.Success) {
        Chat.Send (causedBy, "Command didn't match, use /warpspawn [teleportplayername] or /warpspawn [teleportplayername]");
        return true;
      }
      var TeleportPlayer = causedBy;
      var TeleportPlayerName = m.Groups ["teleportplayername"].Value;
      if (TeleportPlayerName.Length > 0) {
        string Error;
        if (!PlayerHelper.TryGetPlayer (TeleportPlayerName, out TeleportPlayer, out Error)) {
          Chat.Send (causedBy, $"Could not find teleport player '{TeleportPlayerName}'; {Error}");
          return true;
        }
      }
      Teleport.TeleportTo (TeleportPlayer, TerrainGenerator.UsedGenerator.GetSpawnLocation (causedBy));
      return true;
    }
  }
}
