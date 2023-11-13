using System;
using UnityEngine;

namespace Items
{
    public class Blaster : GameItem
    {
        private BlasterType _blasterType;
        public BlasterType BlasterType => _blasterType;
        public void Initialize(Sprite[] blasterSprites, DamageType[] damageTypes, int health, BlasterType blasterType)
        {
            sprites = blasterSprites;
            acceptableDamageTypes = damageTypes;
            this.health = health;
            canShift = true;
            _blasterType = blasterType;
        
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
                particleManager.PlayBlasterExplosion(transform.position);
            if (health <= 0)
            {
                Destroy();
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            levelPoolManager.ReleaseBlaster(this);
        }
    }
}