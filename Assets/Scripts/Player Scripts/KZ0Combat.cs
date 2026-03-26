using UnityEngine;

public class KZ0Combat : MonoBehaviour
{
    [Header("Attaque")]
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;
    public Transform attackPoint;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
        {
            Slash();
        }
    }

    void Slash()
    {
        // Détection des ennemis dans la zone
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<VoidEnemy>(out VoidEnemy voidEnemy))
            {
                voidEnemy.TakeHit();
            }
        }
    }

    // Visualisation de la portée dans l'éditeur
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}