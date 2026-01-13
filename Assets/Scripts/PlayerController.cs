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

    private float moveInput;
    private bool jumpRequest;

    private Camera mainCamera;
    private float screenHeight;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameManager = FindObjectOfType<GameManager>();
        mainCamera = Camera.main;
        // 画面の高さ（ワールド座標）の半分を計算
        if (mainCamera != null)
        {
            screenHeight = mainCamera.orthographicSize;
        }
    }

    void Update()
    {
        if (isDead) return;

        // 入力の受付はUpdateで行う
        moveInput = Input.GetAxis("Horizontal");
        
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            jumpRequest = true;
        }

         // 向きの反転 (Visual Only)
        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);

        // 落下判定: カメラの下端より下に行ったらアウト
        // カメラ位置 - 画面高さの半分 - マージン(1.0f)
        if (mainCamera != null && transform.position.y < (mainCamera.transform.position.y - screenHeight - 1.5f))
        {
            Die();
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // 接地判定
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 移動処理
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        // ジャンプ処理
        if (jumpRequest)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpRequest = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            foreach (ContactPoint2D point in collision.contacts)
            {
                if (point.normal.y > 0.5f) 
                {
                    EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
                    if (enemy != null)
                    {
                        enemy.Defeat();
                        rb.velocity = new Vector2(rb.velocity.x, jumpForce / 1.5f);
                    }
                    return;
                }
            }
            Die();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("Goal"))
        {
            // Goal.csで処理するのでここでの呼び出しは不要、あるいは重複防止
            // Goal.csのOnTriggerEnter2Dが動くので、ここでは何もしないか、
            // GameManager側で重複チェックする
            enabled = false;
            rb.velocity = Vector2.zero;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        GameManager.Instance.GameOver(); // Singleton経由に変更
        rb.velocity = new Vector2(0, 5f);
        GetComponent<Collider2D>().enabled = false;
    }
}
