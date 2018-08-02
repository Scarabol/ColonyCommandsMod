using Pipliz;
using Pipliz.Chatting;
using Pipliz.Mods.APIProvider.Jobs;
using Pipliz.Threading;
using Server.NPCs;
using ChatCommands;
using Permissions;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Reflection;

namespace ColonyCommands
{

  public class DeleteJobsCommand : IChatCommand
  {
    private const int WAIT_DELAY = 250;
    private bool includeBeds = false;

    public bool IsCommand(string chat)
    {
      return (chat.Equals("/deletejobs") || chat.StartsWith("/deletejobs "));
    }

    public bool TryDoCommand(Players.Player causedBy, string chattext)
    {

      var m = Regex.Match(chattext, @"/deletejobs (?<beds>includebeds)? ?(?<player>['].+[']|[^ ]+)$");
      if (!m.Success) {
        Chat.Send(causedBy, "Syntax error, use /deletejobs <player>");
        return true;
      }

      Players.Player target;
      string targetName = m.Groups["player"].Value;
      string error;
      if (!PlayerHelper.TryGetPlayer(targetName, out target, out error, true)) {
        Chat.Send(causedBy, $"Could not find player {targetName}: {error}");
        return true;
      }

      if (m.Groups["beds"].Value.Equals("includebeds")) {
        includeBeds = true;
      }

      if (target == causedBy) {
        if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + "deletejobs.self")) {
          return true;
        }
      } else if (!PermissionsManager.CheckAndWarnPermission(causedBy, AntiGrief.MOD_PREFIX + "deletejobs")) {
        return true;
      }

      Chat.Send(causedBy, $"Jobs of player {targetName} will get deleted in the background");
      ThreadManager.InvokeOnMainThread(delegate() {
        DeleteAllJobs(causedBy, target);
      }, 1);

      return true;
    }

    public void DeleteAllJobs(Players.Player causedBy, Players.Player target)
    {
      int amountAreaJobs = 0;
      int amountBlockJobs = 0;
      int amountBeds = 0;
      string deleteMessage = "Deleted ";

      // AreaJobs (Farms)
      Dictionary<Players.Player, List<IAreaJob>> allAreaJobs = typeof(AreaJobTracker).GetField("playerTrackedJobs",
        BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as Dictionary<Players.Player, List<IAreaJob>>;
      List<IAreaJob> playerAreaJobs = allAreaJobs[target];

      Dictionary<string, int> jobTypes = new Dictionary<string, int>();
      // remove jobs backwards to avoid index / null reference problems
      for (int i = playerAreaJobs.Count - 1; i >= 0; --i) {
        string ident = playerAreaJobs[i].AreaType.ToString();
        if (jobTypes.ContainsKey(ident)) {
          jobTypes[ident]++;
        } else {
          jobTypes.Add(ident, 1);
        }
        AreaJobTracker.RemoveJob(playerAreaJobs[i]);
        ++amountAreaJobs;
        ThreadManager.Sleep(WAIT_DELAY);
      }
      foreach (KeyValuePair<string, int> kvp in jobTypes) {
        Log.Write($"Deleted {kvp.Value} jobs of type {kvp.Key} of player {target.Name}");
      }
      deleteMessage += string.Format("{0} AreaJobs", amountAreaJobs);

      // BlockJobs (everything else, including Guards, Miners and so on)
      List<IBlockJobManager> allBlockJobs = typeof(BlockJobManagerTracker).GetField("InstanceList",
        BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as List<IBlockJobManager>;
      foreach (IBlockJobManager mgr in allBlockJobs) {
        object tracker = mgr.GetType().GetField("tracker", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(mgr);
        MethodInfo methodGetList = tracker.GetType().GetMethod("GetList", new Type[] { typeof(Players.Player) } );
        object jobList = methodGetList.Invoke(tracker, new object[] { target } );

        // happens if a player does not have any jobs of this type
        if (jobList == null) {
          continue;
        }

        int count = (int)jobList.GetType().GetField("count", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(jobList);
        amountBlockJobs += count;

        Type jobType = jobList.GetType().GetGenericArguments()[1];
        MethodInfo methodKeys = jobList.GetType().GetMethod("get_Keys");

        // create a copy of the Collection to allow thread safe deletion
        List<Vector3Int> jobPositions = new List<Vector3Int>();
        foreach (Vector3Int pos in methodKeys.Invoke(jobList, null) as ICollection<Vector3Int>) {
          jobPositions.Add(pos);
        }

        // again, traverse backwards to avoid index / null problems on removing items
        for (int i = jobPositions.Count - 1; i >= 0; --i) {
          mgr.OnRemove(jobPositions[i], 0, null);
          // World.SetTypeAt(jobPositions[i], BlockTypes.Builtin.BuiltinBlocks.Air);
          ServerManager.TryChangeBlock(jobPositions[i], BlockTypes.Builtin.BuiltinBlocks.Air, causedBy);
          ThreadManager.Sleep(WAIT_DELAY);
        }
        Log.Write(string.Format("Deleted {0} jobs {1} of {2}", count, jobType, target.Name));
      }
      if (!deleteMessage.EndsWith("Deleted ")) {
        deleteMessage += ", ";
      }
      deleteMessage += string.Format("{0} BlockJobs", amountBlockJobs);

      // Beds, if requested
      if (includeBeds) {
        BlockTracker<BedBlock> tracker = typeof(BedBlockTracker).GetField("tracker", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as BlockTracker<BedBlock>;
        Pipliz.Collections.SortedList<Vector3Int, BedBlock> bedCollection = tracker.GetList(target);

        List<Vector3Int> bedPositions = new List<Vector3Int>();
        foreach (Vector3Int pos in bedCollection.Keys) {
          bedPositions.Add(pos);
        }
        // again, traverse backwards to avoid index / null problems on removing items
        for (int i = bedPositions.Count - 1; i >= 0; --i) {
          ServerManager.TryChangeBlock(bedPositions[i], BlockTypes.Builtin.BuiltinBlocks.Air, causedBy);
          ThreadManager.Sleep(WAIT_DELAY);
          ++amountBeds;
        }
        Log.Write($"Deleted {amountBeds} Beds of {target.Name}");

        if (!deleteMessage.EndsWith("Deleted ")) {
          deleteMessage += ", ";
        }
        deleteMessage += string.Format("{0} Beds", amountBeds);
      }


      Chat.Send(causedBy, deleteMessage + " of player " + target.Name);
      return;
    }
  }

}

