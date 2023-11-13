using System;
using UnityEngine;
using UnityEngine.UI;

namespace Area.UI
{
    public class AreaTaskUIItem : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private GameObject locked;
        [SerializeField] private GameObject unlocked;
        [SerializeField] private Button unlockButton;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private ParticleSystem unlockEffect;

        private LevelManager _levelManager;
        
        private int _taskID;
        private TaskState _taskState;
        
        public event Action<int> OnCompleteButtonClickedEvent;

        private void Awake()
        {
            unlockButton.onClick.AddListener(OnCompleteButtonClick);
        }

        public void SetTask(AreaTask task, TaskState state)
        {
            _taskID = task.taskID;
            image.sprite = task.taskSprite;
            image.enabled = false;
            
            UpdateState(state);
            
            // set position of the task
            rectTransform.anchoredPosition = task.taskPosition;
            image.SetNativeSize();
            
            locked.transform.localPosition = Vector3.zero;
            unlocked.transform.localPosition = Vector3.zero;
        }
        
        public void UpdateState(TaskState state)
        {
            _taskState = state;
            
            if (state == TaskState.Locked)
            {
                locked.SetActive(true);
                unlocked.SetActive(false);
            }
            else if (state == TaskState.Unlocked)
            {
                locked.SetActive(false);
                unlocked.SetActive(true);
            }
            else if (state == TaskState.Completed)
            {
                locked.SetActive(false);
                unlocked.SetActive(false);
                image.enabled = true;
            }
        }
        
        private void OnCompleteButtonClick()
        {
            unlockEffect.Play();
            OnCompleteButtonClickedEvent?.Invoke(_taskID);
        }

        public void HideButtons()
        {
            locked.SetActive(false);
            unlocked.SetActive(false);
        }
    }
}