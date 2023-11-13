using System;
using UnityEngine;

namespace Items
{
    public class Tnt : GameItem
    {
        private float _nestedDelay;
        public float NestedDelay => _nestedDelay;
        public void Initialize(Sprite[] tntSprites, DamageType[] damageTypes, int health, float nestedDelay)
        {
            sprites = tntSprites;
            acceptableDamageTypes = damageTypes;
            this.health = health;
            canShift = true;
            _nestedDelay = nestedDelay;
        
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
        
            if (damageType != DamageType.Match) 
                particleManager.PlayTntExplosion(transform.position);
            if (health <= 0)
            {
                Destroy();
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            levelPoolManager.ReleaseTnt(this);
        }
    }
}