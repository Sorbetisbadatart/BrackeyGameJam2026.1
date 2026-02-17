using UnityEngine;

namespace Game
{
    public class Trap : MonoBehaviour
    {
        [SerializeField] bool instantKill = false;
        [SerializeField] int damageAmount = 10;
        [SerializeField] string playerTag = "Player";

        void TryAffect(GameObject go)
        {
            if (!go.CompareTag(playerTag)) return;
            var dmg = go.GetComponent<IDamageable>();
            if (dmg != null)
            {
                if (instantKill) dmg.Kill();
                else dmg.ApplyDamage(damageAmount);
            }
            else
            {
                if (instantKill) Destroy(go);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            TryAffect(other.gameObject);
        }

        void OnTriggerStay(Collider other)
        {
            TryAffect(other.gameObject);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            TryAffect(other.gameObject);
        }

        void OnTriggerStay2D(Collider2D other)
        {
            TryAffect(other.gameObject);
        }
    }
}
