using UnityEngine;
using System.Collections;

namespace Pathfinding {
	/// <summary>
	/// Sets the destination of an AI to the position of a specified object.
	/// This component should be attached to a GameObject together with a movement script such as AIPath, RichAI or AILerp.
	/// This component will then make the AI move towards the <see cref="target"/> set on this component.
	///
	/// See: <see cref="Pathfinding.IAstarAI.destination"/>
	///
	/// [Open online documentation to see images]
	/// </summary>
	[UniqueComponent(tag = "ai.destination")]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_a_i_destination_setter.php")]
	public class TopDownEnemy : VersionedMonoBehaviour {

        [SerializeField] float      health = 3.0f;
        [SerializeField] float      startDazedTime = 1.5f;
        [SerializeField] int        damage = 1;
        [SerializeField] bool       noBlood = false;
        [SerializeField] bool       dazed = false;

        // This is the target the object is going to move towards
        public Vector2              target;
        public Transform            player;
        public GameObject           hitEffect;
        public IAstarAI             ai;

        // Location enemy will go to
        public Vector2[]            waypoints;
        private Vector2[]           newWaypoints;
        private int                 currentTargetIndex;
        
        private Animator            animator;
        private Rigidbody2D         body2d;
        private Sensor_HeroKnight   groundSensor;
        private Vector3             previousPosition;
        private Vector3             lastMoveDirection;
        public float                direction;
        private bool                grounded = true;
        private int                 facingDirection = 1;
        private float               timeSinceAttack = 0.0f;
        private float               delayToIdle = 0.0f;
        private float               dazedTime = 0;

        // Start is called before the first frame update
        void Start()
        {
            animator = GetComponent<Animator>();
            body2d = GetComponent<Rigidbody2D>();
            player = GameObject.FindWithTag("Player").GetComponent<Transform>();
            groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();

            // Set the waypoints to be followed
            currentTargetIndex = 0;
            newWaypoints = new Vector2[waypoints.Length+1];
            int w = 0;
            for(int i=0; i<waypoints.Length; i++)
            {
                newWaypoints[i] = waypoints[i];
                w = i;
            }

            // Add the starting position at the end, only if there is at least another point in the queue - otherwise it's on index 0
            int v = (newWaypoints.Length > 1) ? w+1 : 0;
            newWaypoints[v] = transform.position;
            waypoints = newWaypoints;

            // Set the last place and direction of enemy
            previousPosition = transform.position;
            lastMoveDirection = Vector3.zero;

            animator.SetBool("Grounded", grounded);
        }

		void OnEnable () {
			ai = GetComponent<IAstarAI>();
			// Update the destination right before searching for a path as well.
			// This is enough in theory, but this script will also update the destination every
			// frame as the destination is used for debugging and may be used for other things by other
			// scripts as well. So it makes sense that it is up to date every frame.
			if (ai != null) ai.onSearchPath += Update;
		}

		void OnDisable () {
			if (ai != null) ai.onSearchPath -= Update;
		}

        // Update is called once per frame
        void Update()
        {
            direction = lastMoveDirection.x;

            // Updates the AI's destination every frame
            if (target != null && ai != null) ai.destination = target;
		
            // Increase timer that controls attack time
            timeSinceAttack += Time.deltaTime;

            // Change value for changing direction of enemy sprite
            if (lastMoveDirection.x > 0) {
                    facingDirection = 1;
                } 
            if (lastMoveDirection.x < 0) {
                    facingDirection = -1;
                } 

            // Change direction of enemy sprite
            if (facingDirection == 1) {
                    GetComponent<SpriteRenderer>().flipX = true;
                } else if (facingDirection == -1) {
                    GetComponent<SpriteRenderer>().flipX = false;
                } 

            // Stop enemy from moving when dazed
            if(dazedTime <= 0 && health > 0) {
                dazed = false;
            } else {
                body2d.velocity = new Vector2(0, 0);
                dazedTime -= Time.deltaTime;
                dazed = true;
            }

            Vector2 currentTarget = newWaypoints[currentTargetIndex];

            // If player is too far away then don't move
            if (Vector2.Distance(transform.position, player.position) >= 50){
                body2d.velocity = new Vector2(0, 0);
                dazed = true;
            //}
            
            // If player is not within range start moving between waypoints
            // else if (Vector2.Distance(transform.position, player.position) >= 15){
                // target = currentTarget;
            } else if (health > 0 && dazed == false) {
                target = player.position;
            } else {
                target = this.transform.position;
            }

            // If waypoint target is close enough
            if(Vector2.Distance(transform.position, currentTarget) <= .1f)
            {
                // New waypoint has been reached
                currentTargetIndex = (currentTargetIndex<newWaypoints.Length-1) ? currentTargetIndex +1 : 0;
            }

            // Work out direction enemy is moving
            if(transform.position != previousPosition) {
                lastMoveDirection = (transform.position - previousPosition).normalized;
                previousPosition = transform.position;
            }

            // Attack
            if(Vector2.Distance(transform.position, player.position) <= 1.8 && timeSinceAttack > 1.5f && health > 0 && dazed == false)
            {
                body2d.velocity = new Vector2(0, 0);

                // Call attack
                animator.SetTrigger("Attack");

                // Reset timer
                timeSinceAttack = 0.0f;

                StartCoroutine(Attack());
            }

            // Run
            else if (Mathf.Abs(lastMoveDirection.x) > Mathf.Epsilon && health > 0 && dazed == false)
            {
                // Reset timer
                delayToIdle = 0.05f;
                animator.SetInteger("AnimState", 2);
            }

            // Idle
            else if (health > 0)
            {
                // Prevents flickering transitions to idle
                delayToIdle -= Time.deltaTime;
                    if(delayToIdle < 0 && health > 0)
                        animator.SetInteger("AnimState", 1);
            }

            if (health == 0)
                {
                    body2d.velocity = new Vector2(0, 0);
                } 

        }

        public void TakeDamage(int damage) {
            // Damaged
            if (health >= 0) {
                animator.SetTrigger("Hurt");
                dazedTime = startDazedTime;
                Instantiate(hitEffect, transform.position, Quaternion.identity);
                health -= damage;
                Debug.Log("damage");
                // Death
                if (health == 0)
                {
                    dazedTime = 3;
                    health -= damage;
                    animator.SetBool("noBlood", noBlood);
                    animator.SetTrigger("Death");
                    Destroy(gameObject, 2f);
                    Destroy(gameObject.GetComponent<Collider2D>());
                } 
            }
        }

        IEnumerator Attack() {
            if (Player.blocking == false) {
                // Do damage to player
                yield return new WaitForSeconds(0.5f);
                TopDownMovement.Hit();
                Player.health = Player.health -1;
            }
        }

    }
}
