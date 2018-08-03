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
    public const double WAIT_DELAY = 0.5;
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

      string permission = AntiGrief.MOD_PREFIX + "deletejobs";
      if (target == causedBy) {
        permission += ".self";
      }
      if (!PermissionsManager.CheckAndWarnPermission(causedBy, permission)) {
        return true;
      }

      string beds = "";
      int amount = DeleteAreaJobs(causedBy, target);
      amount += DeleteBlockJobs(causedBy, target);
      if (includeBeds) {
        amount += DeleteBeds(causedBy, target);
        beds = "/Beds";
      }
      Chat.Send(causedBy, $"{amount} Jobs{beds} of player {targetName} will get deleted in the background");

      return true;
    }

    // Delete Area Jobs of a player
    private int DeleteAreaJobs(Players.Player causedBy, Players.Player target)
    {
      int amount = 0;
      Dictionary<Players.Player, List<IAreaJob>> allAreaJobs = typeof(AreaJobTracker).GetField("playerTrackedJobs",
        BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as Dictionary<Players.Player, List<IAreaJob>>;
      if (!allAreaJobs.ContainsKey(target)) {
        return 0;
      }
      List<IAreaJob> playerAreaJobs = allAreaJobs[target];

      // go through the list to get amounts per type (just for log output)
      Dictionary<string, int> jobTypes = new Dictionary<string, int>();
      for (int i = playerAreaJobs.Count - 1; i >= 0; --i) {
        string ident = playerAreaJobs[i].AreaType.ToString();
        if (jobTypes.ContainsKey(ident)) {
          jobTypes[ident]++;
        } else {
          jobTypes.Add(ident, 1);
        }
        ++amount;
      }
      foreach (KeyValuePair<string, int> kvp in jobTypes) {
        Log.Write($"Deleting {kvp.Value} jobs of type {kvp.Key} of player {target.Name}");
      }

      // this is the real delete
      DelegatedAreaDelete areaDeletor = new DelegatedAreaDelete(playerAreaJobs, causedBy);
      ThreadManager.InvokeOnMainThread(delegate() {
        areaDeletor.DeleteAreas();
      }, WAIT_DELAY + 0.100);

      return amount;
    }

    // Delete Block Jobs of a player
    private int DeleteBlockJobs(Players.Player causedBy, Players.Player target)
    {
      int amount = 0;
      DelegatedBlockDelete blockDeletor = new DelegatedBlockDelete(causedBy, EDeleteType.BlockJobs);

      List<IBlockJobManager> allBlockJobs = typeof(BlockJobManagerTracker).GetField("InstanceList",
        BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as List<IBlockJobManager>;

      foreach (IBlockJobManager mgr in allBlockJobs) {
        object tracker = mgr.GetType().GetField("tracker", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(mgr);
        MethodInfo methodGetList = tracker.GetType().GetMethod("GetList", new Type[] { typeof(Players.Player) } );
        object jobList = methodGetList.Invoke(tracker, new object[] { target } );

        // happens if no jobs of this type exist
        if (jobList == null) {
          continue;
        }

        Type jobType = jobList.GetType().GetGenericArguments()[1];
        MethodInfo methodKeys = jobList.GetType().GetMethod("get_Keys");
        int count = blockDeletor.Add(methodKeys.Invoke(jobList, null) as ICollection<Vector3Int>);
        amount += count;

        if (count > 0) {
          Log.Write(string.Format("Deleting {0} jobs {1} of {2}", count, jobType, target.Name));
        }
      }
      ThreadManager.InvokeOnMainThread(delegate() {
        blockDeletor.DeleteBlocks();
      }, WAIT_DELAY + 0.200);

      return amount;
    }

    // Delete Beds of a player
    private int DeleteBeds(Players.Player causedBy, Players.Player target)
    {
      DelegatedBlockDelete blockDeletor = new DelegatedBlockDelete(causedBy, EDeleteType.Beds);

      BlockTracker<BedBlock> tracker = typeof(BedBlockTracker).GetField("tracker", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as BlockTracker<BedBlock>;
      Pipliz.Collections.SortedList<Vector3Int, BedBlock> bedCollection = tracker.GetList(target);

      int amount = bedCollection.Count;
      
      blockDeletor.Add(bedCollection.Keys);
      ThreadManager.InvokeOnMainThread(delegate() {
        blockDeletor.DeleteBlocks();
      }, WAIT_DELAY + 0.300);

      Log.Write($"Deleting {amount} Beds of {target.Name}");

      return amount;
    }

  }

  public enum EDeleteType: byte
  {
    AreaJobs,
    BlockJobs,
    Beds
  }

  // Class for the actual delete
  public class DelegatedBlockDelete
  {
    List<Vector3Int> blockList;
    Players.Player causedBy;
    EDeleteType Type;

    public DelegatedBlockDelete(Players.Player causedBy, EDeleteType type)
    {
      this.blockList = new List<Vector3Int>();
      this.causedBy = causedBy;
      this.Type = type;
    }

    public int Add(ICollection<Vector3Int> blocks)
    {
      int count = 0;
      foreach (Vector3Int pos in blocks) {
        blockList.Add(pos);
        ++count;
      }
      return count;
    }

    public void DeleteBlocks()
    {
      if (blockList.Count == 0) {
        return;
      }

      Vector3Int oneBlock = blockList[blockList.Count - 1];
      blockList.Remove(oneBlock);
      ServerManager.TryChangeBlock(oneBlock, BlockTypes.Builtin.BuiltinBlocks.Air, null);

      if (blockList.Count > 0) {
        ThreadManager.InvokeOnMainThread(delegate() {
          this.DeleteBlocks();
        }, DeleteJobsCommand.WAIT_DELAY);
      } else {
        if (causedBy.IsConnected) {
          Chat.Send(causedBy, $"Finished deleting {Type}");
        }
      }
    }
  }

  // Class for the actual delete
  public class DelegatedAreaDelete
  {
    List<IAreaJob> areaList;
    Players.Player causedBy;

    public DelegatedAreaDelete(List<IAreaJob> areas, Players.Player causedBy)
    {
      this.areaList = areas;
      this.causedBy = causedBy;
    }

    public void DeleteAreas()
    {
      if (areaList.Count == 0) {
        return;
      }

      // areaList is a reference. AreaJobTracker will remove the elements from it
      IAreaJob area = areaList[areaList.Count - 1];
      AreaJobTracker.RemoveJob(area);

      if (areaList.Count > 0) {
        ThreadManager.InvokeOnMainThread(delegate() {
          this.DeleteAreas();
        }, DeleteJobsCommand.WAIT_DELAY);
      } else {
        if (causedBy.IsConnected) {
          Chat.Send(causedBy, "Finished deleting AreaJobs");
        }
      }
    }
  }

}

