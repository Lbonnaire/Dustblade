using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;



public class Player_Controller : MonoBehaviour
{
    public float dashDistance = 0.5f;
    public float moveSpeed = 1f;            // Speed of the player character
    public float runSpeedMultiplier = 1.5f;   // Multiplier for running speed
    public float jumpForce = 2.5f;           // Force applied when jumping
    public Transform groundCheck;           // Transform at the player's feet to check for ground
    public LayerMask groundLayer;           // Layermask for identifying ground objects

    //public CapsuleCollider2D cc;
    //Comments

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool facingRight = true;
    public float groundCheckRadius = 1f;
    private bool isRunning = false;
    private bool isLightAttacking = false;
    private bool isDashing = false;
    private Animator animator;

    public float health = 100;

    private SpriteRenderer sr;
    private GameObject l1Hitbox;
    private GameObject l2Hitbox;
    private GameObject l3Hitbox;
    //private GameObject blood;
    private GameObject blood2;

    private float runTime = 0f;
    private float runTimeBuffer = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        l1Hitbox = GameObject.Find("HitBoxL1");
        l2Hitbox = GameObject.Find("HitBoxL2");
        l3Hitbox = GameObject.Find("HitBoxL3");
        //blood = GameObject.Find("Blood");
        blood2 = GameObject.Find("Blood2");
        //cc = GetComponent<CapsuleCollider2D>();

    }

    // Update is called once per frame
    void Update()
    {
        checkHitboxes();
        // Check if the player is grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Handle player input
        float moveInput = Gamepad.current.leftStick.x.ReadValue();

        // Flip character direction if necessary
        if ((moveInput > 0 && !facingRight) || (moveInput < 0 && facingRight))
        {
            Flip();
        }

        // Set animator parameters based on movement
        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        animator.SetBool("IsGrounded", isGrounded);

        // Calculate movement velocity
        Vector2 moveVelocity = new(moveInput * moveSpeed, rb.velocity.y);

        if (Gamepad.current.bButton.wasReleasedThisFrame)
        {
            runTime = 0f;
            if (!isRunning)
            {
                Debug.Log("isDashing");
                isDashing = true;
                //gameObject.transform.position = new Vector3(facingRight ? gameObject.transform.position.x + dashDistance : gameObject.transform.position.x - dashDistance, gameObject.transform.position.y, gameObject.transform.position.z);
                StartCoroutine(Recover(0.1f));
                animator.SetBool("Dashing", isDashing);
            }
        }

        // Apply running speed multiplier
        if (Gamepad.current.bButton.isPressed)
        {
            RunBuffer(0.2f);
            if (runTime > runTimeBuffer)
            {
                Debug.Log("is Running");
                isRunning = true;
                moveVelocity.x *= runSpeedMultiplier;
            }
        }
        else
        {
            isRunning = false;
        }

        if (isDashing)
        {
            moveVelocity.x = facingRight ? moveVelocity.x + 10 : moveVelocity.x - 10;
        }
        //cc.isTrigger = isDashing;

        // Apply movement
        rb.velocity = moveVelocity;


        // Jump
        if (Gamepad.current.aButton.isPressed && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        if (Gamepad.current.rightShoulder.isPressed && !isLightAttacking)
        {
            isLightAttacking = true;
            animator.SetBool("IsLightAttacking", isLightAttacking);
            Debug.Log("ATTACK!");
            StartCoroutine(FinishAttack(0.5f));
        }


        // Set animator parameter for running
        animator.SetBool("IsRunning", isRunning);
    }

    private void RunBuffer(float maxTime)
    {
        if (runTime < maxTime)
        {
            runTime += Time.deltaTime;
        }
    }
    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void checkHitboxes()
    {
        l1Hitbox.SetActive(sr.sprite.name == "Attack1_4");
        l2Hitbox.SetActive(sr.sprite.name == "Attack2_3");
        l3Hitbox.SetActive(sr.sprite.name == "Attack3_4");
        //blood.SetActive(sr.sprite.name == "Take hit_0" || sr.sprite.name == "Take hit_2" || sr.sprite.name == "Take hit_1");
    }

    IEnumerator FinishAttack(float secs)
    {
        yield return new WaitForSecondsRealtime(secs);
        isLightAttacking = false;
        animator.SetBool("IsLightAttacking", isLightAttacking);
        Debug.Log("ATTACK over");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Player is in Trigger");
        if (collision.name == "SkeleHit1")
        {
            health -= 10;
            for (var i = 0; i < 30; i++)
            {
                var bloodParticle = Instantiate(blood2, gameObject.transform.position, gameObject.transform.rotation);
                bloodParticle.transform.position = new Vector2(bloodParticle.transform.position.x, bloodParticle.transform.position.y + 0.381f);
                bloodParticle.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            }
            animator.SetBool("TakeHit", collision.name == "SkeleHit1" || collision.name == "SkeleHit1" || collision.name == "SkeleHit1");
            StartCoroutine(Recover(0.5f));
        }
    }


    IEnumerator Recover(float secs)
    {
        yield return new WaitForSecondsRealtime(secs);
        animator.SetBool("TakeHit", false);
        isDashing = false;
        animator.SetBool("Dashing", false);

    }
}
