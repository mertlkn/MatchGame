using System;
using System.Collections.Generic;
using ScriptableObjects;
using ServiceLocator;
using UnityEngine;
using UnityEngine.Serialization;

namespace Area
{
    public class AreasSerializableInfo
    {
        public List<AreaSerializableInfo> areas;
    }
    [Serializable]
    public class AreaSerializableInfo
    {
        public int areaID;
        public List<int> completedTaskIDs;
    }
    public class AreaManager : IService
    {
        private AreaScriptableObject[] _areaScriptableObjects;
        private AreasSerializableInfo _areaInfos = new AreasSerializableInfo();
        private int _currentAreaIndex;

        public event Action<int> OnTaskUnlocked;
        public event Action<int> OnTaskCompleted;
        
        public readonly string CurrentAreaIndexKey = "CurrentAreaIndex";
        public readonly string CompletedTasksKey = "CompletedTasks";
        
        // private LevelManager _levelManager;
        
        private void Awake()
        {
            // _levelManager = ServiceLocator.ServiceLocator.Instance.GetService<LevelManager>();
            // _levelManager.OnLevelChanged += CheckTasks;
            _areaScriptableObjects = Resources.LoadAll<AreaScriptableObject>("Areas");
            _currentAreaIndex = PlayerPrefs.GetInt(CurrentAreaIndexKey, 0);
            _areaInfos = JsonUtility.FromJson<AreasSerializableInfo>(PlayerPrefs.GetString(CompletedTasksKey, "{}"));

            foreach (var area in _areaScriptableObjects)
            {
                var hasArea = false;
                foreach (var areaInfo in _areaInfos.areas)
                {
                    if (area.areaID == areaInfo.areaID)
                    {
                        hasArea = true;
                        break;
                    }
                }
                if (!hasArea)
                {
                    _areaInfos.areas.Add(new AreaSerializableInfo
                    {
                        areaID = area.areaID,
                        completedTaskIDs = new List<int>()
                    });
                }
            }
            
            PlayerPrefs.SetString(CompletedTasksKey, JsonUtility.ToJson(_areaInfos));
        }
        
        public TaskState GetTaskState(int taskID)
        {
            var currentArea = GetCurrentArea();

            foreach (var areaInfo in _areaInfos.areas)
            {
                if (areaInfo.areaID != currentArea.areaID) continue;

                foreach (var task in currentArea.tasks)
                {
                    if (task.taskID == taskID)
                    {
                        if (areaInfo.completedTaskIDs.Contains(task.taskID))
                        {
                            return TaskState.Completed;
                        }
                        else if (task.unlockLevel <= PlayerPrefs.GetInt("LevelNumber", 0))
                        {
                            return TaskState.Unlocked;
                        }
                    }
                }
            }

            return TaskState.Locked;
        }

        public AreaScriptableObject GetCurrentArea()
        {
            return _areaScriptableObjects[_currentAreaIndex];
        }
        
        public AreaScriptableObject GetArea(int areaIndex)
        {
            return _areaScriptableObjects[areaIndex];
        }

        public void CompleteTask(int taskID)
        {
            var currentArea = GetCurrentArea();

            foreach (var areaInfo in _areaInfos.areas)
            {
                if (areaInfo.areaID != currentArea.areaID) continue;
                
                if (!areaInfo.completedTaskIDs.Contains(taskID))
                {
                    areaInfo.completedTaskIDs.Add(taskID);
                }
            }
            
            PlayerPrefs.SetString(CompletedTasksKey, JsonUtility.ToJson(_areaInfos));
            
            OnTaskCompleted?.Invoke(taskID);
        }
    }
}
