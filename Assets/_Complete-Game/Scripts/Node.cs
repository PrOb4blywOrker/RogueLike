using UnityEngine;

namespace BattleCity.AI
{
    public class Node
    {
        public Vector2Int Position { get; set; }

        public Node CameFrom { get; set; }

        public float DistanceFromStart { get; set; }
        public float ApproximatePathLength { get; set; }
        public float FullPathLength => DistanceFromStart + ApproximatePathLength;
    }
}