using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Base;
using Assets.Scripts.LogicBase;

namespace Assets.Scripts.TacticalBattleScene.PathFinding
{
    #region AStar

    internal delegate bool StopCondition(HexReactor hex);

    /// <summary>
    /// The implementation of the AStar algorithm
    /// </summary>
    internal static class AStar
    {
        private static readonly Dictionary<HexPair, BackwardsAstarNode> s_knownPaths = new Dictionary<HexPair, BackwardsAstarNode>();

        #region public methods

        public static void Clear()
        {
            s_knownPaths.Clear();
        }

        public static Dictionary<HexReactor, MovementAction> FindAllAvailableHexes(HexReactor entry, double availableDistance, MovementType movementType)
        {
            Assert.NotNull(entry, "entry hex");
            var movingEntity = entry.Content as MovingEntity;
            Assert.NotNull(movingEntity, "moving entity in entry hex");
            var dict = new Dictionary<HexReactor, MovementAction>();
            //no heuristic here - we want accurate results
            var internalState = GenerateInternalState(entry, new AStarConfiguration(movementType, check => 0));
            while (internalState.OpenSet.Count > 0)
            {
                AstarNode current = internalState.OpenSet.Pop();
                current.Open = false;
                if (current.FValue >= availableDistance)
                {
                    break;
                }
                //if there's no parent, it's the first hex, and we don't add it to the result
                if (current.Parent != null)
                {
                    MovementAction action;
                    dict.Add(current.ChosenHex,
                             dict.TryGetValue(current.Parent.ChosenHex, out action)
                                 ? new MovementAction(action, current.ChosenHex, current.GValue)
                                 : new MovementAction(movingEntity, new[] {current.ChosenHex}, current.GValue));
                }
                foreach (var neighbour in current.ChosenHex.GetNeighbours())
                {
                    CheckHex(neighbour, current, internalState);
                }
            }
            return dict;
        }

        public static List<HexReactor> FindPath(HexReactor entry, HexReactor goal, AStarConfiguration configuration)
        {
            return ReconstructPath(FindPathNoReconstruction(entry, goal, configuration), goal, configuration);
        }

        public static double FindPathCost(HexReactor entry, HexReactor goal, AStarConfiguration configuration)
        {
            var node = FindPathNoReconstruction(entry, goal, configuration);
            if (node == null)
            {
                return Double.PositiveInfinity;
            }
            return node.GValue;
        }

        private static AstarNode FindPathNoReconstruction(HexReactor entry, HexReactor goal, AStarConfiguration configuration)
        {
            if (goal.Content == null)
            {
                //if the hex is empty, find a path to the hex
                return FindPathNoReconstruction(entry, goal, configuration, hex => hex.Equals(goal));
            }
            //find the cheapest path to one of its neighbours
            return FindPathNoReconstruction(entry, goal, configuration, hex => hex.GetNeighbours().Any(neighbour => neighbour.Equals(goal)));
        }

        private static AstarNode FindPathNoReconstruction(HexReactor entry, HexReactor goal, AStarConfiguration configuration, StopCondition stopCondition)
        {
            var internalState = GenerateInternalState(entry, configuration);

            while (internalState.OpenSet.Count > 0)
            {
                AstarNode current = internalState.OpenSet.Pop();
                if (stopCondition(current.ChosenHex))
                    return current;
                var pair = new HexPair(current.ChosenHex, goal, configuration.TraversalMethod);
                lock (s_knownPaths)
                {
                    BackwardsAstarNode testNode;
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
                                if (!check)
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

        public static void FeedResults(AstarNode rudamentaryList, HexReactor goal, AStarConfiguration configuration)
        {
            ReconstructPath(rudamentaryList, goal, configuration);
        }

        #endregion public methods

        #region private methods

        private static AstarNode ConvertToAstarNode(BackwardsAstarNode backwardsNode)
        {
            AstarNode node = null;
            while (backwardsNode != null)
            {
                node = new AstarNode(backwardsNode.ChosenHex, node);
                backwardsNode = backwardsNode.Son;
            }

            return node ?? new AstarNode(backwardsNode.ChosenHex, null);
        }

        private static AStarInternalState GenerateInternalState(HexReactor entry, AStarConfiguration configuration)
        {
            var internalState = new AStarInternalState(configuration);

            internalState.AddNode(new AstarNode(entry, configuration.Heuristic(entry)));

            return internalState;
        }

        private static void CheckAllNeighbours(AstarNode current, AStarInternalState state)
        {
            foreach (var neighbour in current.ChosenHex.GetNeighbours())
            {
                CheckHex(neighbour, current, state);
            }
        }

        private static void CheckHex(HexReactor temp, AstarNode current, AStarInternalState state)
        {
            var cost = CostOfMovement(temp, state);
            if (cost <= -1) return;

            var costToMove = cost + current.GValue;
            //check if the hex is in the list
            AstarNode newNode;
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

        private static int CostOfMovement(HexReactor temp, AStarInternalState state)
        {
            if (temp.Content != null)
            {
                return -1;
            }
            if (state.Configuration.TraversalMethod == MovementType.Flyer) return 1;
            var cost = (int)state.Configuration.TraversalMethod + (int)temp.Conditions;
            if (cost >= 6)
            {
                return -1;
            }
            return Math.Max(1, cost - 2);
        }

        private static List<HexReactor> ReconstructPath(AstarNode current, HexReactor goal, AStarConfiguration configuration)
        {
            Assert.NotNull(current, "current");
            var ans = new List<HexReactor>();

#if DEBUG
            var nodes = new List<AstarNode>();
#endif
            BackwardsAstarNode testNode = null;
            //we don't take the first direction, because it's already computed by leaving the building
            while (current.Parent != null)
            {
#if DEBUG
                nodes.Insert(0, current);
#endif
                testNode = new BackwardsAstarNode(current, testNode);
                lock (s_knownPaths)
                {
                    var pair = new HexPair(current.ChosenHex, goal, configuration.TraversalMethod);
                    if (!s_knownPaths.ContainsKey(pair))
                    {
                        s_knownPaths.Add(pair, testNode);
                    }
                }
                ans.Insert(0, current.ChosenHex);
                current = current.Parent;
            }

            return ans;
        }

        #endregion private methods

        #region AStarInternalState

        private class AStarInternalState
        {
            public AStarInternalState(AStarConfiguration configuration)
            {
                Configuration = configuration;
                Hexes = new Dictionary<HexReactor, AstarNode>();
                OpenSet = new PriorityQueue<AstarNode>();
            }

            public AStarConfiguration Configuration { get; private set; }

            public Dictionary<HexReactor, AstarNode> Hexes { get; private set; }

            public PriorityQueue<AstarNode> OpenSet { get; private set; }

            public void AddNode(AstarNode node)
            {
                OpenSet.Push(node);
                Hexes.Add(node.ChosenHex, node);
            }
        }

        #endregion AStarInternalState

        #region BackwardsAstarNode

        private class BackwardsAstarNode
        {
            public BackwardsAstarNode(AstarNode node, BackwardsAstarNode son)
            {
                ChosenHex = node.ChosenHex;
                Son = son;
            }

            public BackwardsAstarNode Son { get; private set; }

            public HexReactor ChosenHex { get; private set; }
        }

        #endregion BackwardsAstarNode

        #region HexPair

        private class HexPair
        {
            private readonly HexReactor m_goal;

            private readonly HexReactor m_current;

            private readonly MovementType m_traversalMethod;

            public HexPair(HexReactor current, HexReactor goal, MovementType movement)
            {
                m_current = current;
                m_goal = goal;
                m_traversalMethod = movement;
            }

            public override bool Equals(object obj)
            {
                var other = obj as HexPair;
                return other != null &&
                    other.m_current.Equals(m_current) &&
                    other.m_goal.Equals(m_goal) &&
                    other.m_traversalMethod == m_traversalMethod;
            }

            public override int GetHashCode()
            {
                return Hasher.GetHashCode(m_current, m_goal, m_traversalMethod);
            }

            public override string ToString()
            {
                return "from: {0}, to: {1}, via: {2}".FormatWith(m_current, m_goal, m_traversalMethod);
            }
        }

        #endregion HexPair
    }

    #endregion AStar

    #region AStarConfiguration

    internal class AStarConfiguration
    {
        public AStarConfiguration(MovementType traversalMethod, Heuristic heuristic)
        {
            Assert.NotEqual((int)traversalMethod, (int)MovementType.Unmoving, "Unmoving entities shouldn't be pathfinding");
            TraversalMethod = traversalMethod;
            Heuristic = heuristic;
        }

        public MovementType TraversalMethod { get; private set; }

        public Heuristic Heuristic { get; private set; }
    }

    #endregion AStarConfiguration
}