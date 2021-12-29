using System;
using System.Collections.Generic;
using System.Linq;
using BattleCity.AI;
using JetBrains.Annotations;
using UnityEngine;

namespace Completed {
    public class Pathfinder {
        private const int MAX_WEIGHT = 30000;

        public static readonly Dictionary<Type, float> playerWeightDict = new Dictionary<Type, float>() {
            {typeof(FoodCell), 0.1f},
            {typeof(WallCell), 5f},
            {typeof(EnemyCell), 10000f},
            {typeof(ExitCell), 0.001f},
            {typeof(EmptyCell), 1f},
            {typeof(PlayerCell), 1f},
        };

        public static readonly Dictionary<Type, float> enemyWeightDict = new Dictionary<Type, float>() {
            {typeof(FoodCell), 0.5f},
            {typeof(WallCell), MAX_WEIGHT},
            {typeof(EnemyCell), MAX_WEIGHT},
            {typeof(ExitCell), MAX_WEIGHT},
            {typeof(EmptyCell), 1f},
            {typeof(PlayerCell), 0.0001f},
        };

        public Vector2Int[] FindShortestPath(in Vector2Int start, in Vector2Int goal,
            in Cell[,] field, Dictionary<Type, float> weightDict) {
            return TryGetShortestPath(start, goal, field, out _, out var shortestPath, weightDict) ? shortestPath : null;
        }

        public Vector2Int[] FindShortestPathOrPathToClosest(in Vector2Int start, in Vector2Int goal, in Cell[,] field,
            out bool goalCanBeReached, Dictionary<Type, float> weightDict) {
            goalCanBeReached = true;
            if (TryGetShortestPath(start, goal, field, out List<Node> closedSet, out var shortestPath, weightDict)) {
                return shortestPath;
            }

            goalCanBeReached = false;
            Node closestNode = null;
            var minDistance = float.MaxValue;
            foreach (var node in closedSet.Where(node =>
                (Math.Abs(node.ApproximatePathLength - minDistance) < float.Epsilon
                 && node.DistanceFromStart < (closestNode?.DistanceFromStart ??
                                              int.MaxValue))
                || node.ApproximatePathLength < minDistance)) {
                closestNode = node;
                minDistance = node.ApproximatePathLength;
            }

            return GetPathFromStartToNode(closestNode);
        }

        private bool TryGetShortestPath(Vector2Int start, Vector2Int goal, Cell[,] field, out List<Node> closedSet,
            [CanBeNull] out Vector2Int[] shortestPath, Dictionary<Type, float> weightDict) {
            shortestPath = null;
            closedSet = new List<Node>();
            var openSet = new List<Node>();

            var startNode = new Node {
                Position = start,
                CameFrom = null,
                DistanceFromStart = 0,
                ApproximatePathLength = CalculateApproximatePathLength(start, goal)
            };
            openSet.Add(startNode);

            while (openSet.Count > 0) {
                Node currentNode = openSet.OrderBy(node => node.FullPathLength).First();

                if (currentNode.Position == goal) {
                    {
                        shortestPath = GetPathFromStartToNode(currentNode);
                        return true;
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                foreach (Node neighbourNode in GetValidNeighbours(currentNode, goal, field, weightDict)) {
                    if (closedSet.Count(node => node.Position == neighbourNode.Position) > 0) {
                        continue;
                    }

                    Node openNode = openSet.FirstOrDefault(node => node.Position == neighbourNode.Position);

                    if (openNode == null) {
                        openSet.Add(neighbourNode);
                    }
                    else if (openNode.DistanceFromStart > neighbourNode.DistanceFromStart) {
                        openNode.CameFrom = currentNode;
                        openNode.DistanceFromStart = neighbourNode.DistanceFromStart;
                    }
                }
            }

            return false;
        }

        private IEnumerable<Node> GetValidNeighbours(Node node, Vector2Int goal, Cell[,] field, Dictionary<Type, float> weightDict) {
            List<Node> result = new List<Node>();
            var validUncheckedNodesPositions = GetUncheckedNeighbours(node)
                .Where(uncheckedNeighbour => NeighbourIsValid(uncheckedNeighbour, field, weightDict));
            foreach (var validNodePos in validUncheckedNodesPositions) {
                var _node = new Node();
                _node.Position = validNodePos;
                _node.CameFrom = node;
                _node.DistanceFromStart =
                    node.DistanceFromStart + weightDict[field[validNodePos.x, validNodePos.y].GetType()];
                _node.ApproximatePathLength = CalculateApproximatePathLength(validNodePos, goal);

                result.Add(_node);
            }

            return result;
                                                                                                                    }

        private IEnumerable<Vector2Int> GetUncheckedNeighbours(in Node node) {
            Vector2Int[] uncheckedNeighbours = {
                new Vector2Int(node.Position.x + 1, node.Position.y),
                new Vector2Int(node.Position.x - 1, node.Position.y),
                new Vector2Int(node.Position.x, node.Position.y + 1),
                new Vector2Int(node.Position.x, node.Position.y - 1)
            };

            return uncheckedNeighbours;
        }

        private bool NeighbourIsValid(in Vector2Int position, in Cell[,] field, Dictionary<Type, float> weightDict) {
            return position.x >= 0
                   && position.x < field.GetLength(0)
                   && position.y >= 0
                   && position.y < field.GetLength(1)
                   && PointIsWalkable(weightDict[field[position.x, position.y].GetType()]);
        }

        private bool PointIsWalkable(in float weight) {
            return weight < MAX_WEIGHT;
        }

        private Vector2Int[] GetPathFromStartToNode(in Node node) {
            var path = new List<Vector2Int>();

            Node currentNode = node;
            while (currentNode != null) {
                path.Add(currentNode.Position);
                currentNode = currentNode.CameFrom;
            }

            path.Reverse();
            return path.ToArray();
        }

        private int CalculateApproximatePathLength(in Vector2Int start, in Vector2Int goal) {
            int xDistance = Math.Abs(goal.x - start.x);
            int yDistance = Math.Abs(goal.y - start.y);

            return xDistance + yDistance;
        }
    }
}