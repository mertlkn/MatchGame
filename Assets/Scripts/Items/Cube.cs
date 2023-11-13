using System;
using UnityEngine;

namespace Items
{
    public class Cube : GameItem
    {
        private CubeType _cubeType;
        public CubeType CubeType => _cubeType;

        private Sprite _tntSprite;
    
        public void Initialize(CubeType cubeType, Sprite[] cubeSprites, DamageType[] damageTypes, Sprite tntSprite)
        {
            health = 1;
            _cubeType = cubeType;
            sprites = cubeSprites;
            acceptableDamageTypes = damageTypes;
            canShift = true;
            _tntSprite = tntSprite;
        
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
        
            particleManager.PlayCubeExplosion(_cubeType, transform.position);
            if (health <= 0)
            {
                Destroy();
            }
        }
    
        public void SetTntCreateSprite()
        {
            spriteRenderer.sprite = _tntSprite;
        }

        public override void Destroy()
        {
            base.Destroy();
            levelPoolManager.ReleaseCube(this);
        }
    }
}