using UnityEngine;
using System.Collections;

namespace Completed
{
    public class Enemy : MovingObject
    {
        public int playerDamage; public AudioClip attackSound1; public AudioClip attackSound2;

        private Animator animator; private Transform target; private bool skipMove;
        private Pathfinder pathfinder = new Pathfinder();
        private BoardManager boardManager;


        protected override void Start()
        {
            boardManager = FindObjectOfType<BoardManager>();
            GameManager.instance.AddEnemyToList(this);

            animator = GetComponent<Animator>();

            target = GameObject.FindGameObjectWithTag("Player").transform;

            base.Start();
        }


        protected override void AttemptMove<T>(int xDir, int yDir)
        {
            if (skipMove)
            {
                skipMove = false;
                return;

            }

            base.AttemptMove<T>(xDir, yDir);

            skipMove = true;
        }


        public void MoveEnemy()
        {
            var myPosition = BoardManager.ConvertToFieldPos(transform.position);
            var myVect2Int = myPosition.GetVect2Int();
            var targetPos = BoardManager.ConvertToFieldPos(target.position);
            var path = pathfinder.FindShortestPath(myVect2Int, targetPos.GetVect2Int(),
                boardManager.Field, Pathfinder.enemyWeightDict);
            if ((path?.Length ?? 0) <= 1)
            {
                return;
            }

            var delta = path[1] - myVect2Int;
            Debug.Log(Mathf.Abs(delta.x) != 1 && Mathf.Abs(delta.y) != 1);
            AttemptMove<Player>(delta.x, delta.y);

        }


        protected override void OnCantMove<T>(T component)
        {
            Player hitPlayer = component as Player;

            hitPlayer.LoseFood(playerDamage);

            animator.SetTrigger("enemyAttack");

            SoundManager.instance.RandomizeSfx(attackSound1, attackSound2);
        }
    }
}
