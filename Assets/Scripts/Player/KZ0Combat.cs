using UnityEngine;

public class KZ0Combat : MonoBehaviour
{
    // --- ATTACK CONFIGURATION ---

    [Header("Attaque")]
    public float attackRange = 1.5f;

    public LayerMask enemyLayer;

    public Transform attackPoint;

    // --- UPDATE LOOP ---

    /// Handles player attack input.
    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
        {
            Slash();
        }
    }

    // --- ATTACK LOGIC ---

    /// Detects and damages enemies in attack range.
    void Slash()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<VoidEnemy>(out VoidEnemy voidEnemy))
            {
                voidEnemy.TakeHit();
            }
        }
    }

    // --- DEBUGGING ---

    /// Visualizes attack range in the editor.
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}