using UnityEngine;

namespace Game
{
    [System.Serializable]
    public class Item
    {
        [SerializeField] ItemId id = ItemId.Key;
        [SerializeField] string displayName = "Item";
        [SerializeField] Sprite icon;
        [SerializeField] string description = "";

        public ItemId Id => id;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public string Description => description;
    }
}
