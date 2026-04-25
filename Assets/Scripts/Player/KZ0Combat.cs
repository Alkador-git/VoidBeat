using UnityEngine;

public class KZ0Combat : MonoBehaviour
{
    // --- ATTAQUE ---

    [Header("Attaque")]
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;
    public Transform attackPoint;

    // --- MISE À JOUR ---

    /// Gère les entrées d'attaque du joueur
    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
        {
            Slash();
        }
    }

    // --- LOGIQUE ---

    /// Détecte et frappe les ennemis dans la zone d'attaque
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

    // --- DÉBOGAGE ---

    /// Visualisation de la portée d'attaque dans l'éditeur
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}