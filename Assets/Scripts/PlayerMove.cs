using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float maxSpeed = 5;
    public float jumpPower = 20;
    public float invincibleTimeInSec = 3;

    int LAYER_ID__PLAYER = 10;
    int LAYER_ID__PLAYER_DAMAGED = 11;
    Color PLAYER_COLOR = new Color(1, 1, 1, 1);
    Color PLAYER_DAMAGED_COLOR = new Color(1, 1, 1, 0.4f);

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator animator;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Jump
        if (Input.GetButtonDown("Jump") && !animator.GetBool("isJumping"))
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            animator.SetBool("isJumping", true);
        } 

        // 加档 decay
        if (Input.GetButtonUp("Horizontal"))
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y);

        // 规氢 傈券
        if (Input.GetButton("Horizontal"))
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;

        // Animation
        if (Mathf.Abs(rigid.velocity.x) < 0.3)
            animator.SetBool("isWalking", false);
        else
            animator.SetBool("isWalking", true);
    }

    void FixedUpdate()
    {
        // move
        float h = Input.GetAxisRaw("Horizontal");
        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);
        
        // limit max move spped
        if (rigid.velocity.x > maxSpeed)
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        else if (rigid.velocity.x < -maxSpeed)
            rigid.velocity = new Vector2(-maxSpeed, rigid.velocity.y);

        // Landing Platform
        if (rigid.velocity.y < 0)
        {
            Debug.DrawRay(rigid.position, Vector3.down, Color.green);
            RaycastHit2D raycastHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform"));
            if (raycastHit.collider != null)
            {
                if (raycastHit.distance < 0.5f)
                    animator.SetBool("isJumping", false);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            OnDamaged(collision.transform.position);
        }
    }

    void OnDamaged(Vector2 targetPosition)
    {
        Debug.Log(transform.position.x + " / " + targetPosition.x);
        // Change Layer (公利 惑怕)
        gameObject.layer = LAYER_ID__PLAYER_DAMAGED;

        // Change Alpha
        spriteRenderer.color = PLAYER_DAMAGED_COLOR;

        // Reaction Force
        int reactionDirection = transform.position.x - targetPosition.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(reactionDirection * 10, 1) * 14, ForceMode2D.Impulse);

        // animation
        animator.SetTrigger("doDamaged");

        // 老沥 矫埃 第 公利 秦力
        Invoke("OffDamaged", invincibleTimeInSec);
    }

    void OffDamaged()
    {
        gameObject.layer = LAYER_ID__PLAYER;
        spriteRenderer.color = PLAYER_COLOR;
    }
}
