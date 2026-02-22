using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class FakeInventory : MonoBehaviour
    {
        [System.Serializable]
        public class ItemChangedEvent : UnityEvent<ItemId, bool> { }

        [SerializeField] bool hasKey;
        [SerializeField] bool hasCoin;
        [SerializeField] bool hasPotion;
        [SerializeField] bool hasMap;
        [SerializeField] bool hasGem;
        [SerializeField] ItemChangedEvent onItemChanged;
        [SerializeField] bool persistBetweenScenes;
        [SerializeField] bool usePlayerPrefs;
        [SerializeField] string prefsPrefix = "FakeInv_";

        static FakeInventory _instance;
        public static FakeInventory Instance => _instance;

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                if (persistBetweenScenes) DontDestroyOnLoad(gameObject);
                if (usePlayerPrefs) Load();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        public bool Has(ItemId id)
        {
            switch (id)
            {
                case ItemId.Key: return hasKey;
                case ItemId.Coin: return hasCoin;
                case ItemId.Potion: return hasPotion;
                case ItemId.Map: return hasMap;
                case ItemId.Gem: return hasGem;
                default: return false;
            }
        }

        public void Set(ItemId id, bool value)
        {
            bool changed = false;
            switch (id)
            {
                case ItemId.Key: if (hasKey != value) { hasKey = value; changed = true; } break;
                case ItemId.Coin: if (hasCoin != value) { hasCoin = value; changed = true; } break;
                case ItemId.Potion: if (hasPotion != value) { hasPotion = value; changed = true; } break;
                case ItemId.Map: if (hasMap != value) { hasMap = value; changed = true; } break;
                case ItemId.Gem: if (hasGem != value) { hasGem = value; changed = true; } break;
            }
            if (changed)
            {
                onItemChanged?.Invoke(id, value);
                if (usePlayerPrefs) SaveSingle(id, value);
            }
        }

        public void Acquire(ItemId id) => Set(id, true);
        public void Remove(ItemId id) => Set(id, false);
        public void Toggle(ItemId id) => Set(id, !Has(id));

        public void Save()
        {
            SaveSingle(ItemId.Key, hasKey);
            SaveSingle(ItemId.Coin, hasCoin);
            SaveSingle(ItemId.Potion, hasPotion);
            SaveSingle(ItemId.Map, hasMap);
            SaveSingle(ItemId.Gem, hasGem);
            PlayerPrefs.Save();
        }

        void SaveSingle(ItemId id, bool value)
        {
            PlayerPrefs.SetInt(prefsPrefix + id, value ? 1 : 0);
        }

        public void Load()
        {
            hasKey = PlayerPrefs.GetInt(prefsPrefix + ItemId.Key, hasKey ? 1 : 0) == 1;
            hasCoin = PlayerPrefs.GetInt(prefsPrefix + ItemId.Coin, hasCoin ? 1 : 0) == 1;
            hasPotion = PlayerPrefs.GetInt(prefsPrefix + ItemId.Potion, hasPotion ? 1 : 0) == 1;
            hasMap = PlayerPrefs.GetInt(prefsPrefix + ItemId.Map, hasMap ? 1 : 0) == 1;
            hasGem = PlayerPrefs.GetInt(prefsPrefix + ItemId.Gem, hasGem ? 1 : 0) == 1;
        }

        public void Clear()
        {
            hasKey = hasCoin = hasPotion = hasMap = hasGem = false;
            if (usePlayerPrefs)
            {
                PlayerPrefs.DeleteKey(prefsPrefix + ItemId.Key);
                PlayerPrefs.DeleteKey(prefsPrefix + ItemId.Coin);
                PlayerPrefs.DeleteKey(prefsPrefix + ItemId.Potion);
                PlayerPrefs.DeleteKey(prefsPrefix + ItemId.Map);
                PlayerPrefs.DeleteKey(prefsPrefix + ItemId.Gem);
            }
        }
    }
}
