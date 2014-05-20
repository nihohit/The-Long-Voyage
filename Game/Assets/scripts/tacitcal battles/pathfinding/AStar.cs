using System;
using System.Collections.Generic;

#region AStar

internal static class AStar
{
    private static readonly Dictionary<HexPair, BackwardsAstarNode> s_knownPaths = new Dictionary<HexPair, BackwardsAstarNode>();

	#region public methods

	public static void Clear()
	{
		s_knownPaths.Clear();
	}

    public static Dictionary<Hex, MovementAction> FindAllAvailableHexes(Hex entry, double availableDistance, MovementType movementType)
    {
        Assert.NotNull(entry, "entry hex");
        var movingEntity = entry.Content as MovingEntity;
        Assert.NotNull(movingEntity, "moving entity in entry hex");
        var dict = new Dictionary<Hex, MovementAction>();
        //no heuristic here - we want accurate results
        var internalState = GenerateInternalState(entry, new AStarConfiguration(movementType, (check) => 0));
        while (internalState.OpenSet.Count > 0)
        {
            AstarNode current = internalState.OpenSet.Pop();
            current.Open = false;
            if (current.FValue >= availableDistance)
            {
                break;
            }
            //if there's no parent, it's the first hex, and we don't add it to the result
            if(current.Parent != null)
            {
                MovementAction action;
                if(dict.TryGetValue(current.Parent.ChosenHex, out action))
                {
                    dict.Add(current.ChosenHex, new MovementAction(action, current.ChosenHex, current.GValue));
                }
                else
                {
                    dict.Add(current.ChosenHex, new MovementAction(movingEntity, new[] { current.ChosenHex }, current.GValue));
                }
            }
            foreach (var neighbour in current.ChosenHex.GetNeighbours())
            {
                CheckHex(neighbour, current, internalState);
            }
        }
        return dict;
    }

	public static List<Hex> FindPath(Hex entry, Hex goal, AStarConfiguration configuration)
	{
		return ReconstructPath(FindPathNoReconstruction(entry, goal, configuration), goal, configuration);
	}

    public static double FindPathCost(Hex entry, Hex goal, AStarConfiguration configuration)
    {
        var node = FindPathNoReconstruction(entry, goal, configuration);
        if(node == null)
        {
            return Double.PositiveInfinity;
        }
        return node.GValue;
    }

	private static AstarNode FindPathNoReconstruction(Hex entry, Hex goal, AStarConfiguration configuration)
	{
		var internalState = GenerateInternalState(entry, configuration);

		while (internalState.OpenSet.Count > 0)
		{
			AstarNode current = internalState.OpenSet.Pop();
			if (current.ChosenHex == goal) 
				return current;
			BackwardsAstarNode testNode;
			var pair = new HexPair(current.ChosenHex, goal, configuration.TraversalMethod);
			lock (s_knownPaths)
			{
				if (s_knownPaths.TryGetValue(pair, out testNode))
				{
					bool check = false;
					while (current.Parent != null)
					{
						current = current.Parent;
						pair = new HexPair(current.ChosenHex, goal, configuration.TraversalMethod);
						testNode = new BackwardsAstarNode(current, testNode);
						if (!check)
						{
							check = s_knownPaths.ContainsKey(pair);
							if(!check)
								s_knownPaths.Add(pair, testNode);
						}
					}
					return ConvertToAstarNode(testNode); //this is wrong - we return the beginning point. 
				}
			}
			current.Open = false;
			CheckAllNeighbours(current, internalState);
		}

        return null;
	}

	public static void FeedResults(AstarNode rudamentaryList, Hex goal, AStarConfiguration configuration)
	{
		ReconstructPath(rudamentaryList, goal, configuration);
	}

	#endregion

	#region private methods

	private static AstarNode ConvertToAstarNode(BackwardsAstarNode backwardsNode)
	{
		AstarNode node = null;
		while(backwardsNode != null)
		{
			node = new AstarNode(backwardsNode.ChosenHex, node);
			backwardsNode = backwardsNode.Son;
		}

		if(node == null)
		{
			node = new AstarNode(backwardsNode.ChosenHex, null);
		}

		return node;
	}

	private static AStarInternalState GenerateInternalState(Hex entry, AStarConfiguration configuration)
	{
		var internalState = new AStarInternalState(configuration);

        internalState.AddNode(new AstarNode(entry, configuration.Heuristic(entry)));

		return internalState;
	}
	
	private static void CheckAllNeighbours(AstarNode current, AStarInternalState state)
	{
		foreach(var neighbour in current.ChosenHex.GetNeighbours())
		{
			CheckHex(neighbour, current, state);
		}
	}

	private static void CheckHex(Hex temp, AstarNode current, AStarInternalState state)
	{
		AstarNode newNode;
        var cost = CostOfMovement(temp, state);
        if (cost > -1)
        {
            var costToMove = cost + current.GValue;
	        //check if the hex is in the list
	        if (state.Hexes.TryGetValue(temp, out newNode))
	        {
                if (!newNode.Open)
                {
                    return;

                }
                if (costToMove < newNode.GValue) 
		        {
			        newNode.Parent = current;
                    newNode.GValue = costToMove;
			        state.OpenSet.RemoveLocation(newNode);
			        state.OpenSet.Push(newNode);
		        }
	        }
	        else
	        {
		        newNode = new AstarNode(temp,
                                        costToMove,
                                        0, 
			                            state.Configuration.Heuristic(temp), 
			                            current);
                state.AddNode(newNode);
	        }
        }
	}

	private static int CostOfMovement(Hex temp, AStarInternalState state)
	{
		if(temp.Content != null)
		{
			return -1;
		}
		if (state.Configuration.TraversalMethod == MovementType.Flyer) return 1;
		var cost = (int)state.Configuration.TraversalMethod + (int)temp.Conditions;
		if(cost >= 6)
		{
			return -1;
		}
		else return Math.Max(1, cost - 2);
	}

	private static List<Hex> ReconstructPath(AstarNode current, Hex goal, AStarConfiguration configuration)
	{
        Assert.NotNull(current, "current");
		var ans = new List<Hex>();

#if DEBUG
		var nodes = new List<AstarNode>();
#endif
		BackwardsAstarNode testNode = null;
		//we don't take the first direction, because it's already computed by leaving the building
		while(current.Parent!= null)
		{
#if DEBUG
			nodes.Insert(0, current);
#endif
			testNode = new BackwardsAstarNode(current, testNode);
			lock (s_knownPaths)
			{
				if (testNode != null)
				{
					var pair = new HexPair(current.ChosenHex, goal, configuration.TraversalMethod);
					if (!s_knownPaths.ContainsKey(pair))
					{
						s_knownPaths.Add(pair, testNode);
					}
				}
			}
			ans.Insert(0, current.ChosenHex);
			current = current.Parent;
		}

		return ans;
	}

	#endregion

	#region AStarInternalState

	private class AStarInternalState
	{
		public AStarInternalState(AStarConfiguration configuration)
		{
			Configuration = configuration;
            Hexes = new Dictionary<Hex, AstarNode>();
			OpenSet = new PriorityQueueB<AstarNode>();
		}

		public AStarConfiguration Configuration { get; private set; }
		public Dictionary<Hex, AstarNode> Hexes { get; private set; }
		public PriorityQueueB<AstarNode> OpenSet { get; private set; }

        public void AddNode(AstarNode node)
        {
            OpenSet.Push(node);
            Hexes.Add(node.ChosenHex, node);
        }
	}

	#endregion

	#region BackwardsAstarNode

	private class BackwardsAstarNode
	{
		public BackwardsAstarNode(AstarNode node)
		{
			ChosenHex = node.ChosenHex;
		}

		public BackwardsAstarNode(AstarNode node, BackwardsAstarNode son)
		{
			ChosenHex = node.ChosenHex;
			Son = son;
		}

		public BackwardsAstarNode Son { get; private set; }

		public Hex ChosenHex { get; private set; }
	}

	#endregion

	#region HexPair

	private class HexPair
	{

		public Hex Goal { get; private set; }
		public Hex Current { get; private set; }

		public MovementType TraversalMethod { get; private set; }

		public HexPair(Hex current, Hex goal, MovementType movement)
		{
			Current = current;
			Goal = goal;
			TraversalMethod = movement;
		}

		public override bool Equals(object obj)
		{
			 var other = obj as HexPair;
			return other != null &&
				other.Current.Equals(Current) &&
				other.Goal.Equals(Goal) &&
				other.TraversalMethod == TraversalMethod;
		}

		public override int GetHashCode()
		{
			 return Hasher.GetHashCode(Current , Goal , TraversalMethod);
		}

		public override string ToString()
		{
			return "from: {0}, to: {1}, via: {2}".FormatWith(Current, Goal, TraversalMethod);
		}
	}

	#endregion

}

#endregion

#region AStarConfiguration

internal class AStarConfiguration
{
    public AStarConfiguration(MovementType traversalMethod, Heuristic heuristic)
    {
        TraversalMethod = traversalMethod;
        Heuristic = heuristic;
    }

    public MovementType TraversalMethod { get; private set; }
    public Heuristic Heuristic { get; private set; }
}

#endregion

