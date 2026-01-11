using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public Transform groundCheck;
    public Transform wallCheck;
    public LayerMask collisionLayer;

    private Rigidbody2D rb;
    private bool movingRight = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Check for walls or edges (Sensor logic can stay in Update or FixedUpdate, FixedUpdate is better for physics sync)
    }

    private void FixedUpdate()
    {
        // Move
        rb.velocity = new Vector2(movingRight ? moveSpeed : -moveSpeed, rb.velocity.y);

        // Check for walls (Filter out triggers to avoid UI/other weird hits)
        Collider2D hit = Physics2D.OverlapCircle(wallCheck.position, 0.1f, collisionLayer);
        // Ensure we hit something that is NOT ourselves (just in case) and NOT a trigger
        bool wallHit = hit != null && hit.gameObject != gameObject && !hit.isTrigger;
        
        if (wallHit)
        {
            Flip();
        }
    }

    private void Flip()
    {
        movingRight = !movingRight;
        transform.localScale = new Vector3(movingRight ? 1 : -1, 1, 1);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Simple logic: if player is above, enemy dies. Otherwise player dies.
            // This relies on contact points or relative position.
            
            // For simplicity, let's say if collision normal is mostly down (-1 y), player jumped on top.
            foreach (ContactPoint2D point in collision.contacts)
            {
                if (point.normal.y < -0.5f) 
                {
                    // Player is above
                    GameManager.Instance.AddScore(100);
                    Destroy(gameObject);
                    // Add bounce to player?
                    collision.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 10f, ForceMode2D.Impulse);
                    return;
                }
            }
            
            // If we are here, player got hit from side/bottom
            GameManager.Instance.GameOver();
        }
    }
}
