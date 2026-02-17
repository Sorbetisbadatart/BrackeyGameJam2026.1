using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] int maxHealth = 100;
        [SerializeField] bool destroyOnDeath = true;
        [SerializeField] UnityEvent onDamaged;
        [SerializeField] UnityEvent onDied;

        int currentHealth;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        void Awake()
        {
            currentHealth = maxHealth;
        }

        public void ApplyDamage(int amount)
        {
            if (amount <= 0) return;
            if (currentHealth <= 0) return;
            currentHealth = Mathf.Max(0, currentHealth - amount);
            onDamaged?.Invoke();
            if (currentHealth == 0)
            {
                onDied?.Invoke();
                if (destroyOnDeath) Destroy(gameObject);
            }
        }

        public void Kill()
        {
            if (currentHealth <= 0) return;
            currentHealth = 0;
            onDamaged?.Invoke();
            onDied?.Invoke();
            if (destroyOnDeath) Destroy(gameObject);
        }
    }
}
