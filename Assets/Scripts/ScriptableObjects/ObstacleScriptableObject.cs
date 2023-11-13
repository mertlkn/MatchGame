using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Obstacle", menuName = "ScriptableObjects/ObstacleScriptableObject", order = 2)]
    public class ObstacleScriptableObject : ScriptableObject
    {
        public ObstacleType obstacleType;
        public Sprite[] obstacleSprites;
        public int health;
        public DamageType[] acceptableDamageTypes;
        public bool canShift;
    }
}