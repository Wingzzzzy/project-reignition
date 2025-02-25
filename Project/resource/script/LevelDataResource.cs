using Godot;
using Godot.Collections;
using Project.Core;

namespace Project.Gameplay;

[Tool]
[GlobalClass]
public partial class LevelDataResource : Resource
{
	public enum MissionTypes
	{
		None, // Add a goal node or a boss so the player doesn't get stuck!
		Objective, // Add custom nodes that call IncrementObjective()
		Ring, // Collect a certain amount of rings (set to zero for hands-off missions)
		Pearl, // Collect a certain amount of pearls (normally zero for pearless)
		Enemy, // Destroy a certain amount of enemies
		Race, // Race against an enemy
		Deathless, // Don't die
		Perfect, // Don't take any damage or die
		Chain, // Chain rings together
	}

	public enum CompletionAnimationType
	{
		None, // Sonic just stands there
		ThumbsUp, // Thumbs up variation is determined by character's speed...
	}

	#region Editor
	public override Array<Dictionary> _GetPropertyList()
	{
		Array<Dictionary> properties =
		[
			ExtensionMethods.CreateProperty("Level ID", Variant.Type.StringName),
			ExtensionMethods.CreateProperty("Level Path", Variant.Type.String),
			ExtensionMethods.CreateProperty("Story Event Index", Variant.Type.Int, PropertyHint.Range, "-1,31"),
			ExtensionMethods.CreateProperty("First Clear Bonus", Variant.Type.Int, PropertyHint.Range, "0,10000"),

			ExtensionMethods.CreateProperty("Is Side Mission", Variant.Type.Bool),
			ExtensionMethods.CreateProperty("Has Fire Souls", Variant.Type.Bool),

			ExtensionMethods.CreateProperty("Mission/Type", Variant.Type.Int, PropertyHint.Enum, MissionType.EnumToString()),
			ExtensionMethods.CreateProperty("Mission/Type Key", Variant.Type.String),
			ExtensionMethods.CreateProperty("Mission/Description Key", Variant.Type.String),
			ExtensionMethods.CreateProperty("Mission/Disable Countdown", Variant.Type.Bool),
			ExtensionMethods.CreateProperty("Mission/Time Limit", Variant.Type.Int, PropertyHint.Range, "0,640"),
		];

		if (MissionType != MissionTypes.None && MissionType != MissionTypes.Race
			&& MissionType != MissionTypes.Deathless && MissionType != MissionTypes.Perfect)
		{
			properties.Add(ExtensionMethods.CreateProperty("Mission/Objective Count", Variant.Type.Int, PropertyHint.Range, "0,256"));
		}

		properties.Add(ExtensionMethods.CreateProperty("Ranking/Skip Score", Variant.Type.Bool));
		properties.Add(ExtensionMethods.CreateProperty("Ranking/Gold Time", Variant.Type.Int));
		properties.Add(ExtensionMethods.CreateProperty("Ranking/Silver Time", Variant.Type.Int));
		properties.Add(ExtensionMethods.CreateProperty("Ranking/Bronze Time", Variant.Type.Int));

		if (!SkipScore)
			properties.Add(ExtensionMethods.CreateProperty("Ranking/Score Requirement", Variant.Type.Int, PropertyHint.Range, "0,999999,50"));

		properties.Add(ExtensionMethods.CreateProperty("Completion/Delay", Variant.Type.Float, PropertyHint.Range, "0,5,.1"));
		properties.Add(ExtensionMethods.CreateProperty("Completion/Lockout", Variant.Type.Object)); //, PropertyHint.ResourceType, "LockoutResource"));
		properties.Add(ExtensionMethods.CreateProperty("Completion/Animation", Variant.Type.Int, PropertyHint.Enum, CompletionAnimation.EnumToString()));
		properties.Add(ExtensionMethods.CreateProperty("Completion/Unlock Stage", Variant.Type.Array, PropertyHint.ArrayType,
		$"{Variant.Type.Object:D}/{PropertyHint.ResourceType:D}:LevelDataResource"));
		properties.Add(ExtensionMethods.CreateProperty("Completion/Unlock World", Variant.Type.Int, PropertyHint.Enum,
		"None, Sand Oasis, Dinosaur Jungle, Evil Foundry, Levitated Ruin, Pirate Storm, Skeleton Dome, Night Palace"));

		return properties;
	}

	public override Variant _Get(StringName property)
	{
		switch ((string)property)
		{
			case "Level ID":
				return LevelID;
			case "Level Path":
				return LevelPath;
			case "Story Event Index":
				return StoryEventIndex;
			case "First Clear Bonus":
				return FirstClearBonus;

			case "Is Side Mission":
				return IsSideMission;
			case "Has Fire Souls":
				return HasFireSouls;

			case "Mission/Type":
				return (int)MissionType;
			case "Mission/Type Key":
				return MissionTypeKey;
			case "Mission/Description Key":
				return MissionDescriptionKey;
			case "Mission/Disable Countdown":
				return DisableCountdown;
			case "Mission/Time Limit":
				return MissionTimeLimit;
			case "Mission/Objective Count":
				return MissionObjectiveCount;

			case "Ranking/Skip Score":
				return SkipScore;
			case "Ranking/Gold Time":
				return GoldTime;
			case "Ranking/Silver Time":
				return SilverTime;
			case "Ranking/Bronze Time":
				return BronzeTime;
			case "Ranking/Score Requirement":
				return Score;
			case "Completion/Delay":
				return CompletionDelay;
			case "Completion/Lockout":
				return CompletionLockout;
			case "Completion/Animation":
				return (int)CompletionAnimation;
			case "Completion/Unlock Stage":
				return UnlockStage;
			case "Completion/Unlock World":
				return (int)UnlockWorld;
		}

		return base._Get(property);
	}

	public override bool _Set(StringName property, Variant value)
	{
		switch ((string)property)
		{
			case "Level ID":
				LevelID = (StringName)value;
				break;
			case "Level Path":
				LevelPath = (string)value;
				break;
			case "Story Event Index":
				StoryEventIndex = (int)value;
				break;
			case "First Clear Bonus":
				FirstClearBonus = (int)value;
				break;

			case "Is Side Mission":
				IsSideMission = (bool)value;
				break;
			case "Has Fire Souls":
				HasFireSouls = (bool)value;
				break;

			case "Mission/Type":
				MissionType = (MissionTypes)(int)value;
				NotifyPropertyListChanged();
				break;
			case "Mission/Type Key":
				MissionTypeKey = (string)value;
				break;
			case "Mission/Description Key":
				MissionDescriptionKey = (string)value;
				break;
			case "Mission/Disable Countdown":
				DisableCountdown = (bool)value;
				break;
			case "Mission/Time Limit":
				MissionTimeLimit = (int)value;
				break;
			case "Mission/Objective Count":
				MissionObjectiveCount = (int)value;
				break;

			case "Ranking/Skip Score":
				SkipScore = (bool)value;
				NotifyPropertyListChanged();
				break;
			case "Ranking/Gold Time":
				GoldTime = (int)value;
				break;
			case "Ranking/Silver Time":
				SilverTime = (int)value;
				break;
			case "Ranking/Bronze Time":
				BronzeTime = (int)value;
				break;
			case "Ranking/Score Requirement":
				Score = (int)value;
				break;

			case "Completion/Delay":
				CompletionDelay = (float)value;
				break;
			case "Completion/Lockout":
				CompletionLockout = (LockoutResource)value;
				break;
			case "Completion/Animation":
				CompletionAnimation = (CompletionAnimationType)(int)value;
				break;
			case "Completion/Unlock Stage":
				UnlockStage = (Array<LevelDataResource>)value;
				break;
			case "Completion/Unlock World":
				UnlockWorld = (SaveManager.WorldEnum)(int)value;
				break;
			default:
				return false;
		}

		return true;
	}
	#endregion

	/// <summary> Level's id - used for save data. </summary>
	public StringName LevelID { get; private set; }
	/// <summary> Path to the level's scene. </summary>
	public string LevelPath { get; private set; }
	/// <summary> Story event index to play after completing the stage. Set to -1 if no story event is meant to be played. </summary>
	public int StoryEventIndex = -1;

	/// <summary> Does this mission contain fire souls? </summary>
	public bool HasFireSouls { get; private set; }
	/// <summary> Should this mission be shown as optional? </summary>
	public bool IsSideMission { get; private set; }
	/// <summary> Amount of extra experience to grant when the player first clears the level. </summary>
	public int FirstClearBonus { get; private set; }

	/// <summary> Type of mission. </summary>
	public MissionTypes MissionType { get; private set; }
	/// <summary> Localization key for the type of mission (Goal, Rampage, Rings, etc.). </summary>
	public string MissionTypeKey { get; private set; }
	/// <summary> Localization key for the more specific description. </summary>
	public string MissionDescriptionKey { get; private set; }
	/// <summary> Should the countdown be disabled for this stage (i.e. bosses, control test, etc.)? </summary>
	public bool DisableCountdown { get; private set; }
	/// <summary> Level time limit, in seconds. </summary>
	public float MissionTimeLimit { get; private set; }
	/// <summary> What's the target amount for the mission objective? </summary>
	public int MissionObjectiveCount { get; private set; }

	// Rank
	/// <summary> Enable this to ignore score when calculating rank (i.e. for bosses) </summary>
	public bool SkipScore { get; private set; }

	// Requirements for time rank. Format is in seconds.
	public int GoldTime { get; private set; }
	public int SilverTime { get; private set; }
	public int BronzeTime { get; private set; }
	// Requirement for score rank
	public int Score { get; private set; }

	/// <summary> How long to wait before transitioning to the completion camera. </summary>
	public float CompletionDelay { get; private set; }
	/// <summary> Control lockout to apply when the level is completed. Leave null to use Runtime.Instance.StopLockout. </summary>
	public LockoutResource CompletionLockout { get; private set; }
	/// <summary> Should Sonic give a thumbs up when the level is completed? </summary>
	public CompletionAnimationType CompletionAnimation { get; private set; }
	/// <summary> List of stages to unlock when the level is completed. </summary>
	public Array<LevelDataResource> UnlockStage { get; private set; }
	/// <summary> World to unlock when the level is completed. </summary>
	public SaveManager.WorldEnum UnlockWorld { get; private set; }
}
