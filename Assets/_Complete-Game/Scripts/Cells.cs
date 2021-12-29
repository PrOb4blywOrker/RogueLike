using UnityEngine;

namespace Completed {
    public abstract class Cell {
        public class Point {
            public Point(int x, int y) {
                X = x;
                Y = y;
            }

            public int X { get; set; }
            public int Y { get; set; }

            public Vector2Int GetVect2Int() => new Vector2Int(X, Y);
        }

        public Point Pos { get; set; }
        public int X => Pos.X;
        public int Y => Pos.Y;

        protected Cell(Point pos) {
            Pos = pos;
        }
    }


    public class EmptyCell : Cell {
        public EmptyCell(Point pos) : base(pos) { }
    }
    
    public class ExitCell : Cell {
        public ExitCell(Point pos) : base(pos) { }
    }

    public class FoodCell : Cell {
        public FoodCell(Point pos) : base(pos) { }
    }

    public class WallCell : Cell {
        public Wall wall;

        public WallCell(Point pos, Wall wall) : base(pos) {
            this.wall = wall;
        }
    }

    public class PlayerCell : Cell {
        public Player player;

        public PlayerCell(Point pos, Player player) : base(pos) {
            this.player = player;
        }
    }

    public class EnemyCell : Cell {
        public Enemy enemy;

        public EnemyCell(Point pos, Enemy enemy) : base(pos) {
            this.enemy = enemy;
        }
    }
}