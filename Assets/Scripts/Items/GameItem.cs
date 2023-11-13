using DG.Tweening;
using Pools;
using UnityEngine;

namespace Items
{
    public abstract class GameItem : MonoBehaviour
    {
        private static readonly float Gravity = 20;
        
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] private Rigidbody2D rigidbody2D;
    
        public SpriteRenderer SpriteRenderer => spriteRenderer;
    
        protected int health;
        public int Health => health;
    
        protected DamageType[] acceptableDamageTypes;
        public DamageType[] AcceptableDamageTypes => acceptableDamageTypes;
    
        protected Sprite[] sprites;
        public Sprite[] Sprites => sprites;
    
        protected bool canShift;
        public bool CanShift => canShift;
    
        protected Tween shiftTween;
        protected Tween shakeTween;
    
        protected LevelPoolManager levelPoolManager;
        protected ParticleManager particleManager;
        
        private float freeFallDuration;

        private void Awake()
        {
            levelPoolManager = ServiceLocator.ServiceLocator.Instance.GetService<LevelPoolManager>();
            particleManager = ServiceLocator.ServiceLocator.Instance.GetService<ParticleManager>();
        }

        public abstract bool CanTakeDamage(DamageType damageType);
        public abstract void UpdateVisuals();
        public abstract void TakeDamage(DamageType damageType);

        public virtual void Destroy()
        {
            KillTweens();
        }
    
        public void KillTweens()
        {
            shiftTween?.Kill();
            shakeTween?.Kill();
        }  
        
        private void Update()
        {
            if (!canShift) return;
        
            if (transform.localPosition.y > 0)
            {
                freeFallDuration += Time.deltaTime;
                rigidbody2D.velocity = new Vector2(0, -freeFallDuration * Gravity);
            }
            else
            {
                freeFallDuration = 0f;
                rigidbody2D.velocity = Vector2.zero;
                transform.localPosition = Vector2.zero;
            }
        }

        public void Shake()
        {
            shakeTween?.Kill();
            shakeTween = transform.DOShakeRotation(0.15f, 20, 30, 99)
                .OnComplete(() => transform.rotation = Quaternion.identity)
                .OnKill(() => transform.rotation = Quaternion.identity);
        }
    }
}