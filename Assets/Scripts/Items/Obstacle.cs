using System;
using UnityEngine;

namespace Items
{
    public class Obstacle : GameItem
    {
        private ObstacleType _obstacleType;
        public ObstacleType ObstacleType => _obstacleType;
    
        public void Initialize(ObstacleType obstacleType, Sprite[] obstacleSprites, DamageType[] damageTypes, int health,
            bool canShift)
        {
            this.health = health;
            _obstacleType = obstacleType;
            sprites = obstacleSprites;
            acceptableDamageTypes = damageTypes;
            this.canShift = canShift;
        
            UpdateVisuals();
        }

        public override bool CanTakeDamage(DamageType damageType)
        {
            return Array.Exists(acceptableDamageTypes, type => type == damageType);
        }

        public override void UpdateVisuals()
        {
            var spriteIndex = Math.Clamp(health - 1, 0, sprites.Length - 1);
            spriteRenderer.sprite = sprites[spriteIndex];
        }

        public override void TakeDamage(DamageType damageType)
        {
            health--;
            UpdateVisuals();
        
            particleManager.PlayObstacleExplosion(_obstacleType, transform.position);
            if (health <= 0)
            {
                Destroy();
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            levelPoolManager.ReleaseObstacle(this);
        }
    }
}