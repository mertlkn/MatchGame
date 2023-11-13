using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Cube", menuName = "ScriptableObjects/CubeScriptableObject", order = 1)]
    public class CubeScriptableObject : ScriptableObject
    {
        public CubeType cubeType;
        public Sprite[] cubeSprites;
        public DamageType[] acceptableDamageTypes;
        public Sprite tntSprite;
    }
}