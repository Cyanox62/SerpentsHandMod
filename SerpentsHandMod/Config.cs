using Exiled.API.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;

namespace SerpentsHandMod
{
    public class Config : IConfig
    {
		[Description("If Serpents Hand is enabled.")]
		public bool IsEnabled { get; set; } = true;

		[Description("The items Serpents Hand spawn with.")]
		public List<int> SpawnItems { get; set; } = new List<int>() { 20, 33, 10, 12, 27, 26, 19 };

		[Description("Determines how many players equate to one Serpent spawning.")]
		public int SpawnRate { get; set; } = 5;
		[Description("The amount of health Serpents Hand has.")]
		public int Health { get; set; } = 200;

		[Description("The distance a Class-D must be within a Serpents Hand to become one.")]
		public float SpawnDistance { get; set; } = 10f;

		[Description("The message announced by CASSIE when Serpents hand spawn.")]
		public string EntryAnnouncement { get; set; } = "";
		[Description("The message announced by CASSIE when Chaos spawn.")]
		public string CiEntryAnnouncement { get; set; } = "";
		[Description("The broadcast sent to Serpents Hand when they spawn.")]
		public string SpawnBroadcast { get; set; } = "<size=60>You are <color=#03F555><b>Serpents Hand</b></color></size>\n<i>Help the <color=\"red\">SCPs</color> by killing all other classes!</i>";

		[Description("Determines if friendly fire between Serpents Hand and SCPs is enabled.")]
		public bool FriendlyFire { get; set; } = false;
		[Description("Determines if Serpents Hand should teleport to SCP-106 after exiting his pocket dimension.")]
		public bool TeleportTo106 { get; set; } = true;
		[Description("[IMPORTANT] Set this config to false if Chaos and SCPs CANNOT win together on your server.")]
		public bool ScpsWinWithChaos { get; set; } = true;
    }
}
