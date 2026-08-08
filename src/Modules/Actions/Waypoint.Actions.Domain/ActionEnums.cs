namespace Waypoint.Actions.Domain;

public enum ActionPriority { Low, Medium, High }

public enum ActionDifficulty { Easy, Medium, Hard }

public enum ActionImpact { Low, Medium, High }

public enum ActionStatus { NotStarted, InProgress, Completed, Blocked, Cancelled }
