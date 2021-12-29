using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
namespace Completed
{
    public class BoardManager : MonoBehaviour
    {
        public enum CellType
        {
            Empty,
            Exit,
            Food,
            Wall,
            Player,
            Enemy,
        }


        [Serializable]
        public class Count
        {
            public int minimum; public int maximum;

            public Count(int min, int max)
            {
                minimum = min;
                maximum = max;
            }
        }


        public const int columns = 8; public const int rows = 8; public Count wallCount = new Count(5, 9); public Count foodCount = new Count(1, 5); public GameObject exit; public GameObject[] floorTiles; public GameObject[] wallTiles; public GameObject[] foodTiles; public GameObject[] enemyTiles; public GameObject[] outerWallTiles;
        private Transform boardHolder; private List<Vector3> gridPositions = new List<Vector3>(); private List<Wall> walls = new List<Wall>(); private List<GameObject> food = new List<GameObject>(); private List<Enemy> enemies = new List<Enemy>(); private Player _player;
        public GameObject Exit { get; private set; }
        public Cell[,] Field { get; private set; } = new Cell[columns, rows];


        private void FindPlayer()
        {
            _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        void InitialiseList()
        {
            gridPositions.Clear();

            for (int x = 1; x < columns - 1; x++)
            {
                for (int y = 1; y < rows - 1; y++)
                {
                    gridPositions.Add(new Vector3(x, y, 0f));
                }
            }
        }



        void BoardSetup()
        {
            boardHolder = new GameObject("Board").transform;

            for (int x = -1; x < columns + 1; x++)
            {
                for (int y = -1; y < rows + 1; y++)
                {
                    GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];

                    if (x == -1 || x == columns || y == -1 || y == rows)
                        toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];

                    GameObject instance =
    Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;

                    instance.transform.SetParent(boardHolder);
                }
            }
        }



        Vector3 RandomPosition()
        {
            int randomIndex = Random.Range(0, gridPositions.Count);

            Vector3 randomPosition = gridPositions[randomIndex];

            gridPositions.RemoveAt(randomIndex);

            return randomPosition;
        }



        void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum, CellType cellType)
        {
            int objectCount = Random.Range(minimum, maximum + 1);

            for (int i = 0; i < objectCount; i++)
            {
                Vector3 randomPosition = RandomPosition();

                GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];

                SpawnCellObject(cellType, tileChoice, randomPosition);
            }
        }

        private void SpawnCellObject(CellType cellType, GameObject prefab, Vector3 pos)
        {
            var gameObj = Instantiate(prefab, pos, Quaternion.identity);
            var point = ConvertToFieldPos(pos);

            switch (cellType)
            {
                case CellType.Empty:
                    Field[point.X, point.Y] = new EmptyCell(point);
                    break;
                case CellType.Exit:
                    Field[point.X, point.Y] = new ExitCell(point);
                    Exit = gameObj;
                    break;
                case CellType.Food:
                    Field[point.X, point.Y] = new FoodCell(point);
                    food.Add(gameObj);
                    break;
                case CellType.Wall:
                    var wall = gameObj.GetComponent<Wall>();
                    Field[point.X, point.Y] = new WallCell(point, wall);
                    walls.Add(wall);
                    break;
                case CellType.Player:
                    Field[point.X, point.Y] = new PlayerCell(point, gameObj.GetComponent<Player>());
                    break;
                case CellType.Enemy:
                    var enemy = gameObj.GetComponent<Enemy>();
                    Field[point.X, point.Y] = new EnemyCell(point, enemy);
                    enemies.Add(enemy);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cellType), cellType, null);
            }
        }



        public void SetupScene(int level)
        {
            BoardSetup();

            InitialiseList();

            FindPlayer();

            AddPlayerToField();

            SpawnWalls();

            SpawnFood();

            SpawnEnemies(level);

            SpawnExit();

            AddEmptyCells();

            StartCoroutine(UpdateField());
        }

        private void AddEmptyCells()
        {
            var n = Field.GetLength(0);
            var m = Field.GetLength(1);
            for (int x = 0; x < n; x++)
            {
                for (int y = 0; y < m; y++)
                {
                    if (Field[x, y] == null)
                    {
                        Field[x, y] = new EmptyCell(new Cell.Point(x, y));
                    }
                }
            }
        }

        private void AddPlayerToField()
        {
            var point = ConvertToFieldPos(_player.transform.position);
            Field[point.X, point.Y] = new PlayerCell(point, _player);
        }

        private void SpawnExit()
        {
            SpawnCellObject(CellType.Exit, exit, new Vector3(columns - 1, rows - 1, 0f));
        }

        private void SpawnEnemies(int level)
        {
            enemies.Clear();
            int enemyCount = (int)Mathf.Log(level, 2f);

            LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount, CellType.Enemy);
        }

        private void SpawnFood()
        {
            food.Clear();
            LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum, CellType.Food);
        }

        private void SpawnWalls()
        {
            walls.Clear();
            LayoutObjectAtRandom(wallTiles, wallCount.minimum, wallCount.maximum, CellType.Wall);
        }

        private IEnumerator UpdateField()
        {
            while (true)
            {
                Field.Nullify();

                UpdateWalls();
                UpdateFood();
                UpdateEnemies();
                UpdatePlayer();
                UpdateExit();
                AddEmptyCells();
                yield return new WaitForSeconds(_player.moveTime);
            }
        }

        private void UpdateExit()
        {
            var exitPoint = ConvertToFieldPos(Exit.transform.position);
            Field[exitPoint.X, exitPoint.Y] = new ExitCell(exitPoint);
        }

        private void UpdatePlayer()
        {
            var playerPoint = ConvertToFieldPos(_player.transform.position);
            Field[playerPoint.X, playerPoint.Y] = new PlayerCell(playerPoint, _player);
        }

        private void UpdateFood()
        {
            food.ForEach(_food =>
            {
                var point = ConvertToFieldPos(_food.transform.position);
                Field[point.X, point.Y] = new FoodCell(point);
            });
        }

        private void UpdateEnemies()
        {
            enemies.ForEach(enemy =>
            {
                var point = ConvertToFieldPos(enemy.transform.position);
                Field[point.X, point.Y] = new EnemyCell(point, enemy);
            });
        }

        private void UpdateWalls()
        {
            walls.ForEach(wall =>
            {
                var point = ConvertToFieldPos(wall.transform.position);
                Field[point.X, point.Y] = new WallCell(point, wall);
            });
        }

        public static Cell.Point ConvertToFieldPos(Vector3 scenePos)
        {
            return new Cell.Point((int)scenePos.x, (int)scenePos.y);
        }

        public static Vector3 ConvertToScenePos(Cell.Point point)
        {
            return new Vector3(point.X, point.Y, 0);
        }
    }
}