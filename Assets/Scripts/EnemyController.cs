using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 2f;
    private int direction = 1; // 1: Right, -1: Left
    private Rigidbody2D rb;
    public Transform groundCheck;
    public Transform wallCheck;
    public float checkRadius = 0.1f;
    public LayerMask groundLayer;
    private GameManager gameManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameManager = FindObjectOfType<GameManager>();
    }

    void FixedUpdate()
    {
        // 地面の切れ目検知（進行方向に地面があるか）
        bool isGroundAhead = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        // 壁検知（進行方向に壁があるか）
        bool isWallAhead = Physics2D.OverlapCircle(wallCheck.position, checkRadius, groundLayer);

        // 地面がない、または壁がある場合、反転
        if (!isGroundAhead || isWallAhead)
        {
            Flip();
        }

        rb.velocity = new Vector2(moveSpeed * direction, rb.velocity.y);
    }

    void Flip()
    {
        direction *= -1;
        transform.localScale = new Vector3(direction, 1, 1);
    }

    public void Defeat()
    {
        // スコア加算
        if (gameManager != null)
        {
            gameManager.AddScore(100);
        }
        
        // 倒された演出（パーティクルなどあればここで生成）
        Destroy(gameObject);
    }
}
