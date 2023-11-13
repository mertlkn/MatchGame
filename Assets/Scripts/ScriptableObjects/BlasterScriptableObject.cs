using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Blaster", menuName = "ScriptableObjects/BlasterScriptableObject", order = 4)]
    public class BlasterScriptableObject : ScriptableObject
    {
        public Sprite[] blasterSprites;
        public int health;
        public DamageType[] acceptableDamageTypes;
    }
}