using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Completed
{

    public class Player : MovingObject
    {
        public float restartLevelDelay = 1f;
        public int pointsPerFood = 10;
        public int pointsPerSoda = 20;
        public int wallDamage = 1;
        public Text foodText;
        public AudioClip moveSound1;
        public AudioClip moveSound2;
        public AudioClip eatSound1;
        public AudioClip eatSound2;
        public AudioClip drinkSound1;
        public AudioClip drinkSound2;
        public AudioClip gameOverSound;

        private Animator animator;
        private int food;
        private Pathfinder pathfinder = new Pathfinder();

        private BoardManager boardManager;


        protected override void Start()
        {
            boardManager = FindObjectOfType<BoardManager>();
            animator = GetComponent<Animator>();
            food = GameManager.instance.playerFoodPoints;
            foodText.text = "Food: " + food;
            base.Start();
        }


        private void OnDisable()
        {
            GameManager.instance.playerFoodPoints = food;
        }


        private void Update()
        {
            if (!GameManager.instance.playersTurn) return;

            var playerFieldPos = BoardManager.ConvertToFieldPos(transform.position);
            var exitFieldPos = BoardManager.ConvertToFieldPos(boardManager.Exit.transform.position);
            var playerVect2Int = playerFieldPos.GetVect2Int();
            var path = pathfinder.FindShortestPath(playerVect2Int, exitFieldPos.GetVect2Int(),
                boardManager.Field, Pathfinder.playerWeightDict);
            if ((path?.Length ?? 0) <= 1)
            {
                return;
            }

            var delta = path[1] - playerVect2Int;
            Debug.Log(delta.x != 1 && delta.y != 1);
            AttemptMove<Wall>(delta.x, delta.y);

        }

        protected override void AttemptMove<T>(int xDir, int yDir)
        {
            food--;

            foodText.text = "Food: " + food;

            base.AttemptMove<T>(xDir, yDir);

            RaycastHit2D hit;

            if (Move(xDir, yDir, out hit))
            {
                SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
            }

            CheckIfGameOver();

            GameManager.instance.playersTurn = false;
        }


        protected override void OnCantMove<T>(T component)
        {
            Wall hitWall = component as Wall;

            hitWall.DamageWall(wallDamage);

            animator.SetTrigger("playerChop");
        }


        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "Exit")
            {
                Invoke("Restart", restartLevelDelay);

                enabled = false;
            }

            else if (other.tag == "Food")
            {
                food += pointsPerFood;

                foodText.text = "+" + pointsPerFood + " Food: " + food;

                SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);

                other.gameObject.SetActive(false);
            }

            else if (other.tag == "Soda")
            {
                food += pointsPerSoda;

                foodText.text = "+" + pointsPerSoda + " Food: " + food;

                SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);

                other.gameObject.SetActive(false);
            }
        }


        private void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }


        public void LoseFood(int loss)
        {
            animator.SetTrigger("playerHit");

            food -= loss;

            foodText.text = "-" + loss + " Food: " + food;

            CheckIfGameOver();
        }


        private void CheckIfGameOver()
        {
            if (food <= 0)
            {
                SoundManager.instance.PlaySingle(gameOverSound);

                SoundManager.instance.musicSource.Stop();

                GameManager.instance.GameOver();
            }
        }
    }
}