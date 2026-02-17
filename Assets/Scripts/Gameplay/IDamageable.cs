using UnityEngine;

namespace Game
{
    public interface IDamageable
    {
        int CurrentHealth { get; }
        int MaxHealth { get; }
        void ApplyDamage(int amount);
        void Kill();
    }
}
