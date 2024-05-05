using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    enum PLAYER_SOUND_TYPE
    {
        JUMP,
        ATTACK,
        DAMAGED,
        ITEM,
        DIE,
        FINISH,
    }

    public float maxSpeed = 5;
    public float jumpPower = 20;
    public float invincibleTimeInSec = 3;
    public int healthPoint = 3;

    public GameManager gameManager;
    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamaged;
    public AudioClip audioItem;
    public AudioClip audioDie;
    public AudioClip audioFinish;

    int PLAYER__LAYER_ID = 10;
    int PLAYER_DAMAGED__LAYER_ID = 11;
    Color PLAYER__COLOR = new Color(1, 1, 1, 1);
    Color PLAYER__DAMAGED_COLOR = new Color(1, 1, 1, 0.4f);

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator animator;
    CapsuleCollider2D capsuleCollider;
    AudioSource audioSource;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>();
    }

    void PlaySound(PLAYER_SOUND_TYPE playerSoundType)
    {
        switch (playerSoundType)
        {
            case PLAYER_SOUND_TYPE.JUMP:
                audioSource.clip = audioJump;
                break;
            case PLAYER_SOUND_TYPE.ATTACK:
                audioSource.clip = audioAttack;
                break;
            case PLAYER_SOUND_TYPE.DAMAGED:
                audioSource.clip = audioDamaged;
                break;
            case PLAYER_SOUND_TYPE.ITEM:
                audioSource.clip = audioItem;
                break;
            case PLAYER_SOUND_TYPE.DIE:
                audioSource.clip = audioDie;
                break;
            case PLAYER_SOUND_TYPE.FINISH:
                audioSource.clip = audioFinish;
                break;
        }
        audioSource.Play();
    }

    private void Update()
    {
        // Jump
        if (Input.GetButtonDown("Jump") && !animator.GetBool("isJumping"))
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            animator.SetBool("isJumping", true);
            PlaySound(PLAYER_SOUND_TYPE.JUMP);
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
            if (rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y)
            {
                // Attack
                OnAttack(collision.transform);
            }
            else
            {
                // Damaged
                OnDamaged(collision.transform);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Item")
        {
            // Point
            gameManager.OnTakeItem(collision.gameObject);

            // Deactive Item
            collision.gameObject.SetActive(false);

            // play sound
            PlaySound(PLAYER_SOUND_TYPE.ITEM);
        }
        else if (collision.gameObject.tag == "Finish")
        {
            // Next Stage
            gameManager.OnFinishStage();
            
            // player reposition
            OnPlayerInitPosition();

            // play sound
            PlaySound(PLAYER_SOUND_TYPE.FINISH);
        }
        else if (collision.gameObject.tag == "GameManager")
        {
            // Damaged
            OnDamaged(collision.transform);

            // player reposition
            if (healthPoint > 0)
                OnPlayerInitPosition();
        }
    }

    void OnPlayerInitPosition()
    {
        rigid.velocity = Vector2.zero;
        transform.position = new Vector3(0, 0, 0);

    }

    void OnAttack(Transform enemyTransform)
    {
        // Point
        gameManager.OnKillEnemy();

        // Reaction Force
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

        // Enemy Die
        EnemyMove enemyMove = enemyTransform.GetComponent<EnemyMove>();
        if (enemyMove != null)
        {
            enemyMove.OnDamaged();
        }

        // play sound
        PlaySound(PLAYER_SOUND_TYPE.ATTACK);
    }

    public void OnPlayerHealthPointDown()
    {
        // health down
        healthPoint--;

        // UI update
        gameManager.OnHealthPointDown(healthPoint);

        if (healthPoint <= 0)
        {   
            // player die effect
            OnDie();

            // update ui
            gameManager.OnGameOver();
        }
    }

    void OnDamaged(Transform enemyTransform)
    {
        // Health Point Down
        OnPlayerHealthPointDown();

        // Change Layer (公利 惑怕)
        gameObject.layer = PLAYER_DAMAGED__LAYER_ID;

        // Change Alpha
        spriteRenderer.color = PLAYER__DAMAGED_COLOR;

        // Reaction Force
        int reactionDirection = transform.position.x - enemyTransform.transform.position.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(reactionDirection * 10, 1) * 14, ForceMode2D.Impulse);

        // animation
        animator.SetTrigger("doDamaged");

        // 老沥 矫埃 第 公利 秦力
        Invoke("OffDamaged", invincibleTimeInSec);

        // play sound
        PlaySound(PLAYER_SOUND_TYPE.DAMAGED);
    }

    void OffDamaged()
    {
        gameObject.layer = PLAYER__LAYER_ID;
        spriteRenderer.color = PLAYER__COLOR;
    }

    void OnDie()
    {
        // Sprite Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        // Sprite Flip Y
        spriteRenderer.flipY = true;

        // Collider Disable
        capsuleCollider.enabled = false;

        // Die Effect Jump
        rigid.AddForce(Vector2.up * 3, ForceMode2D.Impulse);

        // play sound
        PlaySound(PLAYER_SOUND_TYPE.DIE);
    }
}
