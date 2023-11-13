using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Tnt", menuName = "ScriptableObjects/TntScriptableObject", order = 4)]
    public class TntScriptableObject : ScriptableObject
    {
        public Sprite[] tntSprites;
        public int health;
        public DamageType[] acceptableDamageTypes;
        public int damageArea;
        public int comboDamageArea;
        public float nestedDelay;
    }
}