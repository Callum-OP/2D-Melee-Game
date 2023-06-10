using UnityEngine;
using System.Collections;

public class TopDownMovement : MonoBehaviour {

    [SerializeField] float      m_speed = 5.0f;
    [SerializeField] float      m_rollForce = 10.0f;
    [SerializeField] float      startHealTime = 5f;
    [SerializeField] bool       m_noBlood = false;

    private Animator            m_animator;
    private Rigidbody2D         m_body2d;
    private Sensor_HeroKnight   m_groundSensor;
    private Sensor_HeroKnight   m_wallSensorR1;
    private Sensor_HeroKnight   m_wallSensorR2;
    private Sensor_HeroKnight   m_wallSensorL1;
    private Sensor_HeroKnight   m_wallSensorL2;
    private bool                m_grounded = true;
    private bool                m_rolling = false;
    private bool                m_running = false;
    private int                 m_facingDirection = 1;
    private int                 m_currentAttack = 0;
    private float               m_timeSinceAttack = 0.0f;
    private float               m_delayToIdle = 0.0f;
    private float               m_rollDuration = 8.0f / 14.0f;
    private float               m_rollCurrentTime;
    private float               healTime;


    // Use this for initialization
    void Start ()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<Sensor_HeroKnight>();

        m_animator.SetBool("Grounded", m_grounded);
    }

    // Update is called once per frame
    void Update ()
    {
        // Increase timer that controls attack combo
        m_timeSinceAttack += Time.deltaTime;

        // Increase timer that checks roll duration
        if(m_rolling)
            m_rollCurrentTime += Time.deltaTime;

        // Disable rolling if timer extends duration
        if(m_rollCurrentTime > m_rollDuration)
            m_rolling = false;

        // -- Handle input and movement --
        float inputX = Input.GetAxis("Vertical");
        float inputY = Input.GetAxis("Horizontal");

        // Swap direction of sprite depending on walk direction
        if (inputY > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;
        }
            
        else if (inputY < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
        }

        // Move
        if (!m_rolling )
            m_body2d.velocity = new Vector2(inputX * m_speed, m_body2d.velocity.y);
        if (!m_rolling )
            m_body2d.velocity = new Vector2(inputY * m_speed, m_body2d.velocity.x);

        // -- Handle Animations --

        // Death
        if (Input.GetKeyDown("e") && !m_rolling)
        {
            m_animator.SetBool("noBlood", m_noBlood);
            m_animator.SetTrigger("Death");
        }
            
        // Hurt
        else if (Input.GetKeyDown("q") && !m_rolling)
            m_animator.SetTrigger("Hurt");

        // Attack
        else if(Input.GetMouseButtonDown(0) && m_timeSinceAttack > 0.3f && !m_rolling)
        {
            m_currentAttack++;

            // Loop back to one after third attack
            if (m_currentAttack > 3)
                m_currentAttack = 1;

            // Reset Attack combo if time since last attack is too large
            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;

            // Call one of three attack animations "Attack1", "Attack2", "Attack3"
            m_animator.SetTrigger("Attack" + m_currentAttack);

            // Reset timer
            m_timeSinceAttack = 0.0f;
        }

        else if (Input.GetMouseButtonUp(1))
        {
            m_animator.SetBool("IdleBlock", false);
            Player.blocking = false;
        }

        // Roll
        else if (Input.GetKeyDown("space") && !m_rolling)
        {
            m_rolling = true;
            m_animator.SetTrigger("Roll");
            m_body2d.velocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.velocity.y);
        }

        // Run
        else if (Mathf.Abs(inputX) > Mathf.Epsilon || Mathf.Abs(inputY) > Mathf.Epsilon)
        {
            // Reset timer
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }

        // Block
        else if (Input.GetMouseButtonDown(1) && !m_rolling)
        {
            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
            Player.blocking = true;
        }

        // Idle
        else
        {
            // Prevents flickering transitions to idle
            m_delayToIdle -= Time.deltaTime;
                if(m_delayToIdle < 0)
                    m_animator.SetInteger("AnimState", 0);
        }

        // Heal
        if (Player.health != Player.maxHealth) {
            healTime -= Time.deltaTime;
            if (healTime < 0) {
                healTime = startHealTime;
                Player.health = Player.health + 1;
            }
        }
    }

    public static void Hit() 
    {
        Animator m_animator = GameObject.FindWithTag("Player").GetComponent<Animator>();
        if (Player.health == 1)
        {
            m_animator.SetTrigger("Death");
        } else {
            m_animator.SetTrigger("Hurt");
        }
    }
}
