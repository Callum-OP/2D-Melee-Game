using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerEnemy : MonoBehaviour
{
    [SerializeField] float      speed = 2.0f;
    [SerializeField] float      health = 3.0f;
    [SerializeField] float      startDazedTime = 0.6f;
    [SerializeField] bool       noBlood = false;
    [SerializeField] bool       dazed = false;

    // This is the target the object is going to move towards
	public Transform            target;
    public Transform            player;
    public GameObject           hitEffect;
    
    private Animator            animator;
    private Rigidbody2D         body2d;
    private Sensor_HeroKnight   groundSensor;
    private Vector3             previousPosition;
    private Vector3             lastMoveDirection;
    private bool                grounded = false;
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
    }

    // Update is called once per frame
    void Update()
    {
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
            
        //Move towards the target
		body2d.MovePosition(Vector2.Lerp(transform.position, target.position, Time.fixedDeltaTime * speed));

        //Check if character just landed on the ground
        if (!grounded && groundSensor.State())
        {
            grounded = true;
            animator.SetBool("Grounded", grounded);
        }

        //Check if character just started falling
        if (grounded && !groundSensor.State())
        {
            grounded = false;
            animator.SetBool("Grounded", grounded);
        }

        // Work out direction enemy is moving
            if(transform.position != previousPosition) {
                lastMoveDirection = (transform.position - previousPosition).normalized;
                previousPosition = transform.position;
            }

        // Change direction of enemy sprite
        if (lastMoveDirection.x == 1) {
                GetComponent<SpriteRenderer>().flipX = false;
                facingDirection = 1;
            }
        if (lastMoveDirection.x == -1) {
                GetComponent<SpriteRenderer>().flipX = true;
                facingDirection = -1;
            }

        // Set AirSpeed in animator
        animator.SetFloat("AirSpeedY", body2d.velocity.y);

        //Attack
        if(Vector2.Distance(transform.position, player.position) <= 1 && timeSinceAttack > 0.25f && health > 0 && dazed == false)
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

        //Run
        else if (Mathf.Abs(lastMoveDirection.x) > Mathf.Epsilon && health > 0 && speed > 0 && dazed == false)
        {
            // Reset timer
            delayToIdle = 0.05f;
            animator.SetInteger("AnimState", 1);
        }

        //Idle
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
        } 
    }
}
