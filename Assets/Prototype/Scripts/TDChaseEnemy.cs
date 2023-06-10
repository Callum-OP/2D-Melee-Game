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
	public class TDChaseEnemy : VersionedMonoBehaviour {

        [SerializeField] float      speed = 2.0f;
        [SerializeField] float      health = 3.0f;
        [SerializeField] float      startDazedTime = 0.6f;
        [SerializeField] bool       noBlood = false;
        [SerializeField] bool       dazed = false;

        // This is the target the object is going to move towards
        public Transform            target;
        public Transform            player;
        public GameObject           hitEffect;
        public IAstarAI             ai;
        
        private Animator            animator;
        private Rigidbody2D         body2d;
        private Sensor_HeroKnight   groundSensor;
        private Vector3             previousPosition;
        private Vector3              lastMoveDirection;
        private bool                grounded = true;
        private int                 facingDirection = 1;
        private int                 currentAttack = 0;
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
            // Updates the AI's destination every frame
            if (target != null && ai != null) ai.destination = target.position;
		
            // Increase timer that controls attack combo
            timeSinceAttack += Time.deltaTime;

            if(dazedTime <= 0 && health > 0) {
                speed = 2;
                dazed = false;
            } else {
                speed = 0;
                dazedTime -= Time.deltaTime;
                dazed = true;
            }

            // If player is too far away then don't move
                if (Vector2.Distance(transform.position, player.position) >= 50){
                    target = this.transform;
                }
                // If player is not within range start moving between waypoints
                else if (Vector2.Distance(transform.position, player.position) >= 6){
                    target = this.transform;
                } else {
                    target = player;
                }

                if (target == this.transform) {
                    dazed = true;
                }

            // Work out direction enemy is moving
                if(transform.position != previousPosition) {
                    lastMoveDirection = (transform.position - previousPosition).normalized;
                    previousPosition = transform.position;
                }

            // Change direction of enemy sprite
            if (lastMoveDirection.x > 0) {
                    GetComponent<SpriteRenderer>().flipX = false;
                    facingDirection = 1;
                } else {
                    GetComponent<SpriteRenderer>().flipX = true;
                    facingDirection = -1;
                }

            // Attack
            if(Vector2.Distance(transform.position, player.position) <= 1.5 && timeSinceAttack > 0.25f && health > 0 && dazed == false)
            {
                currentAttack++;

                // Loop back to one after third attack
                if (currentAttack > 3)
                    currentAttack = 1;

                // Reset Attack combo if time since last attack is too large
                if (timeSinceAttack > 1.0f)
                    currentAttack = 1;

                // Call one of three attack animations "Attack1", "Attack2", "Attack3"
                animator.SetTrigger("Attack" + currentAttack);

                // Reset timer
                timeSinceAttack = 0.0f;
            }

            // Run
            else if (Mathf.Abs(lastMoveDirection.x) > Mathf.Epsilon && health > 0 && speed > 0 && dazed == false)
            {
                // Reset timer
                delayToIdle = 0.05f;
                animator.SetInteger("AnimState", 1);
            }

            // Idle
            else if (health > 0)
            {
                // Prevents flickering transitions to idle
                delayToIdle -= Time.deltaTime;
                    if(delayToIdle < 0 && health > 0)
                        animator.SetInteger("AnimState", 0);
            }

        }

        public void TakeDamage(int damage) {
            // Damaged
            if (health > 0) {
                animator.SetTrigger("Hurt");
                dazedTime = startDazedTime;
                Instantiate(hitEffect, transform.position, Quaternion.identity);
                health -= damage;
                Debug.Log("damage");
            }

            // Death
            if (health == 0)
            {
                dazedTime = startDazedTime;
                health -= damage;
                animator.SetBool("noBlood", noBlood);
                animator.SetTrigger("Death");
                speed = 0;
                Destroy(gameObject, 2f);
                Destroy(gameObject.GetComponent<Collider2D>());
            } 
        }
    }
}
