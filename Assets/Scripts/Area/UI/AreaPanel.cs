using System;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Area.UI
{
    public class AreaPanel : MonoBehaviour
    {
        [SerializeField] private AreaTaskUIItem areaTaskUIItemPrefab;
        [SerializeField] private Image background;
        [SerializeField] private Camera areaCamera;

        private Dictionary<int, AreaTaskUIItem> _tasks = new Dictionary<int, AreaTaskUIItem>();

        private GameManager _gameManager;
        private AreaManager _areaManager;
        private IObjectPool<AreaTaskUIItem> _taskPool;

        private void Awake()
        {
            _areaManager = ServiceLocator.ServiceLocator.Instance.GetService<AreaManager>();
            _gameManager = ServiceLocator.ServiceLocator.Instance.GetService<GameManager>();
            _taskPool = new ObjectPool<AreaTaskUIItem>(CreateTask, OnGetTask, OnReleaseTask);
            _gameManager.OnLevelLoaded += OnLevelLoaded;
            _gameManager.OnMenuLoaded += OnMenuLoaded;
            _areaManager.OnTaskCompleted += UpdateTask;
            _areaManager.OnTaskUnlocked += UpdateTask;
        }
        
        private void OnDestroy()
        {
            _gameManager.OnLevelLoaded -= OnLevelLoaded;
            _gameManager.OnMenuLoaded -= OnMenuLoaded;
            _areaManager.OnTaskCompleted -= UpdateTask;
            _areaManager.OnTaskUnlocked -= UpdateTask;
        }

        private void UpdateTask(int taskID)
        {
            if (_tasks.TryGetValue(taskID, out var task))
            {
                task.SetTask(_areaManager.GetCurrentArea().tasks[taskID], _areaManager.GetTaskState(taskID));
            }
        }

        private void Start()
        {
            var currentArea = _areaManager.GetCurrentArea();
            
            // background.sprite = currentArea.backgroundSprite;
            
            foreach (var task in currentArea.tasks)
            {
                var taskInstance = _taskPool.Get();
                taskInstance.transform.SetParent(background.transform, false);
                taskInstance.SetTask(task, _areaManager.GetTaskState(task.taskID));
                taskInstance.transform.localScale = Vector3.one * 1.5f;
                taskInstance.OnCompleteButtonClickedEvent += OnCompleteButtonClick;
                _tasks[task.taskID] = taskInstance;
            }
        }

        private void OnCompleteButtonClick(int taskID)
        {
            _areaManager.CompleteTask(taskID);
        }

        private void OnLevelLoaded()
        {
            foreach (var task in _tasks)
            {
                task.Value.HideButtons();
            }
        }

        private void OnMenuLoaded()
        {
            foreach (var task in _tasks)
            {
                task.Value.UpdateState(_areaManager.GetTaskState(task.Key));
            }
        }
        
        private void OnReleaseTask(AreaTaskUIItem obj)
        {
            obj.gameObject.SetActive(false);
        }
        
        private void OnGetTask(AreaTaskUIItem obj)
        {
            obj.gameObject.SetActive(true);
        }
        
        private AreaTaskUIItem CreateTask()
        {
            return Instantiate(areaTaskUIItemPrefab);
        }
        
#if UNITY_EDITOR
        
        [Header("Editor Only")]
        [SerializeField] private RectTransform content;
        [SerializeField] private int areaIndex;
        [SerializeField] private int taskIndex;
        [SerializeField] private bool set;
        private void OnValidate()
        {
            if (!set) return;

            if (content == null)
            {
                set = false;
                return;
            }
            
            var allAreas = Resources.LoadAll<AreaScriptableObject>("Areas");
            
            if (areaIndex > allAreas.Length - 1)
            {
                set = false;
                return;
            }
            
            var area = allAreas[areaIndex];
            
            if (taskIndex > area.tasks.Count - 1)
            {
                set = false;
                return;
            }
            
            area.tasks[taskIndex].taskPosition = content.anchoredPosition;
            set = false;
            
            content = null;
            areaIndex = 0;
            taskIndex = 0;
            
        }
#endif
    }
}