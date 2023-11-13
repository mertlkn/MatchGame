using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace UI
{
    public class LevelPanel : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup gridLayoutGroup;
        [SerializeField] private LevelTargetItem targetItemPrefab;
        [SerializeField] private TextMeshProUGUI moveCountText;
        
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;
        
        [SerializeField] private Button skipButton;
        
        [SerializeField] private float autoSkipDelay = 3f;

        private IObjectPool<LevelTargetItem> _levelTargetItemPool;
        private List<LevelTargetItem> _levelTargetItems;
        
        private LevelManager _levelManager;
        private BoardManager _boardManager;
        
        private Coroutine _unloadLevelCoroutine;

        private void Awake()
        {
            _levelManager = ServiceLocator.ServiceLocator.Instance.GetService<LevelManager>();
            _boardManager = ServiceLocator.ServiceLocator.Instance.GetService<BoardManager>();
            
            _levelManager.OnLevelCreated += OnLevelCreated;
            _boardManager.OnTargetUpdated += OnTargetUpdated;
            _boardManager.OnMove += OnMove;
            _levelManager.OnGameEnded += OnGameEnded;
            _levelTargetItemPool = new ObjectPool<LevelTargetItem>(CreateTargetItem, OnGetTargetItem,
                OnReleaseTargetItem, OnDestroyTargetItem, false);
            _levelTargetItems = new List<LevelTargetItem>();
        }

        private void OnDestroy()
        {
            _levelManager.OnLevelCreated -= OnLevelCreated;
            _boardManager.OnTargetUpdated -= OnTargetUpdated;
            _boardManager.OnMove -= OnMove;
            _levelManager.OnGameEnded -= OnGameEnded;
        }

        private void OnMove()
        {
            moveCountText.text = $"{_boardManager.MovesLeft}";
        }

        private void OnTargetUpdated()
        {
            var targets = _boardManager.CurrentLevelTarget;
            foreach (var target in targets)
            {
                var targetItem = _levelTargetItems.Find(item => item.ObstacleType == target.Key);
                
                if (targetItem == null) continue;
                
                targetItem.SetTargetCount(target.Value);
            }
        }

        private void OnLevelCreated(LevelInfo levelInfo)
        {
            winPanel.SetActive(false);
            losePanel.SetActive(false);
            skipButton.gameObject.SetActive(false);
            
            foreach (var kvp in _levelTargetItems)
            {
                _levelTargetItemPool.Release(kvp);
            }
            
            _levelTargetItems.Clear();

            var targets = _boardManager.CurrentLevelTarget;
            foreach (var target in targets)
            {
                var targetItem = _levelTargetItemPool.Get();
                targetItem.SetTarget(target.Key, target.Value);
            }
            
            moveCountText.text = $"{levelInfo.move_count}";
        }
        
        
        private void OnGameEnded(bool success)
        {
            if (success)
            {
                winPanel.SetActive(true);
            }
            else
            {
                losePanel.SetActive(true);
            }
            
            skipButton.gameObject.SetActive(true);
            skipButton.onClick.AddListener(OnSkipButtonClicked);
            
            _unloadLevelCoroutine = StartCoroutine(UnloadLevel());
        }
        
        IEnumerator UnloadLevel()
        {
            yield return new WaitForSeconds(autoSkipDelay);
            _levelManager.UnloadLevel();
        }
        
        private void OnSkipButtonClicked()
        {
            skipButton.onClick.RemoveListener(OnSkipButtonClicked);
            
            if (_unloadLevelCoroutine != null)
            {
                StopCoroutine(_unloadLevelCoroutine);
            }
            
            _levelManager.UnloadLevel();
        }

        private LevelTargetItem CreateTargetItem()
        {
            return Instantiate(targetItemPrefab, gridLayoutGroup.transform);
        }

        private void OnDestroyTargetItem(LevelTargetItem obj)
        {
            Destroy(obj.gameObject);
        }

        private void OnReleaseTargetItem(LevelTargetItem obj)
        {
            obj.gameObject.SetActive(false);
            _levelTargetItems.Remove(obj);
        }

        private void OnGetTargetItem(LevelTargetItem obj)
        {
            obj.gameObject.SetActive(true);
            _levelTargetItems.Add(obj);
        }
    }
}