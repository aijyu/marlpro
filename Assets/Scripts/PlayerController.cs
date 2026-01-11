using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    private Rigidbody2D rb;
    private bool isGrounded;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private GameManager gameManager;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        if (isDead) return;

        // 接地判定
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 移動
        float moveInput = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        // 向きの反転
        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);

        // ジャンプ
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // 落下判定
        if (transform.position.y < -10f)
        {
            Die();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            // 敵の上から接触したか判定 (プレイヤーの足元が敵の中心より上)
            foreach (ContactPoint2D point in collision.contacts)
            {
                if (point.normal.y > 0.5f) // 上方向からの接触
                {
                    // 敵を踏んだ
                    EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
                    if (enemy != null)
                    {
                        enemy.Defeat();
                        // 踏んだ反動でジャンプ
                        rb.velocity = new Vector2(rb.velocity.x, jumpForce / 1.5f);
                    }
                    return;
                }
            }

            // 横や下から当たったらダメージ（ゲームオーバー）
            Die();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("Goal"))
        {
            gameManager.StageClear();
            enabled = false; // 操作無効化
            rb.velocity = Vector2.zero;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        gameManager.GameOver();
        // プレイヤーを少し跳ねさせてから落下させる演出（任意）
        rb.velocity = new Vector2(0, 5f);
        GetComponent<Collider2D>().enabled = false; // 当たり判定を消して落下
    }
}
