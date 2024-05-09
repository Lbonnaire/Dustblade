using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Monster_Skeleton : MonoBehaviour
{
    public float moveSpeed = 1f;

    public float health = 100;

    private bool facingRight = true;
    private Animator animator;
    private Rigidbody2D rb;

    //true = right, false = left
    public bool direction = true;
    private bool idle = false;
    private GameObject Hero;
    private bool aggro = false;
    public float distanceToHero;
    public bool isAttacking;
    private GameObject a1Hitbox;
    private SpriteRenderer sr;
    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        Hero = GameObject.Find("Hero");
        a1Hitbox = GameObject.Find("SkeleHit1");
        InvokeRepeating(nameof(Move), 2.0f, 2f);
    }

    // Update is called once per frame
    void Update()
    {
        CheckHitboxes();
        if (!idle & !aggro)
        {
            rb.velocity = new Vector2(direction ? Random.Range(0, 1f) : Random.Range(-1f, 0), 0);
        }
        if ((rb.velocity.x > 0 && !facingRight) || (rb.velocity.x < 0 && facingRight))
        {
            Flip();
        }
        //float currentSpeed = rb.velocity.x;
        animator.SetFloat("Speed", idle ? 0 : 1);
        if (health <= 0)
        {
            Die();
        }


    }

    void Move()
    {
        distanceToHero = Vector2.Distance((Vector2)Hero.transform.position, (Vector2)gameObject.transform.position);
        if (distanceToHero < 10) { aggro = true; }
        if (!aggro)
        {
            direction = Random.Range(0, 1f) > 0.5f;
            idle = Random.Range(0, 1f) > 0.6f;
        }
        else if (distanceToHero < 1 && !isAttacking)
        {
            idle = false;
            direction = facingRight;
            isAttacking = true;
            Debug.Log((Hero.transform.position.x < gameObject.transform.position.x && facingRight) || (Hero.transform.position.x > gameObject.transform.position.x && !facingRight));
            //Skeleton is not facing hero
            if ((Hero.transform.position.x < gameObject.transform.position.x && facingRight) || (Hero.transform.position.x > gameObject.transform.position.x && !facingRight))
            {
                Flip();
            }
            animator.SetBool("isAttacking", isAttacking);
            StartCoroutine(Recover(1.5f));
        }

    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void Die()
    {
        animator.SetBool("Dead", true);
        StartCoroutine(Death(2f));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "HitBoxL1" || collision.name == "HitBoxL2" || collision.name == "HitBoxL3")
        {
            Debug.Log("Skeleton is in Trigger");
            health -= 30;
            animator.SetBool("TakeHit", true);
            StartCoroutine(Recover(0.75f));
        }
    }
    IEnumerator Recover(float secs)
    {
        isAttacking = false;
        yield return new WaitForSecondsRealtime(secs);
        animator.SetBool("TakeHit", false);
        animator.SetBool("isAttacking", isAttacking);
    }

    IEnumerator Death(float secs)
    {
        yield return new WaitForSecondsRealtime(secs);
        Destroy(gameObject);
    }

    private void CheckHitboxes()
    {
        a1Hitbox.SetActive(sr.sprite.name == "Attack_6" || sr.sprite.name == "Attack_7");
    }
}