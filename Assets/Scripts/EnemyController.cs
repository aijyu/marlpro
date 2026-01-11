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
    private Collider2D myCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
        gameManager = FindObjectOfType<GameManager>();
    }

    void FixedUpdate()
    {
        // 自分以外のコライダーにヒットするかチェックする
        // 常に地面に接しているか (Platformの端判定)
        bool isGroundAhead = CheckCollision(groundCheck.position);
        
        // 壁があるか
        bool isWallAhead = CheckCollision(wallCheck.position);

        // デバッグ表示（Sceneビューで確認用）
        Debug.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * checkRadius, isGroundAhead ? Color.green : Color.red);
        Debug.DrawLine(wallCheck.position, wallCheck.position + Vector3.right * direction * checkRadius, isWallAhead ? Color.red : Color.green);

        // 「地面がない」または「壁がある」場合、反転
        if (!isGroundAhead || isWallAhead)
        {
            Flip();
        }

        rb.velocity = new Vector2(moveSpeed * direction, rb.velocity.y);
    }

    // 自分自身を除外してOverlapCircle判定を行う
    bool CheckCollision(Vector3 position)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, checkRadius, groundLayer);
        foreach (var col in colliders)
        {
            if (col != myCollider && !col.isTrigger) // 自分以外、かつTriggerでないもの
            {
                return true;
            }
        }
        return false;
    }

    void Flip()
    {
        direction *= -1;
        transform.localScale = new Vector3(direction, 1, 1);
    }

    public void Defeat()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(100);
        }
        Destroy(gameObject);
    }
}
