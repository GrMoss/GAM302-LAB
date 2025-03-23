using Fusion;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 100f;

    [Networked] 
    public float Health { get; set; }

    public override void Spawned()
    {
        Health = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        Debug.Log($"Player {Object.Id} took {damage} damage!");
        if (!HasStateAuthority) return;
        Health = Mathf.Max(Health - damage, 0);
        if (Health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (Runner.IsServer)
        {
            Debug.Log($"Player {Object.Id} died!");
        }
        Runner.Despawn(Object);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!HasStateAuthority) return;
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(10f);
        }
    }
}