using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenuPanel : MonoBehaviour
    {
        private const string FinishedText = "Finished";
        private const string LevelText = "Level {0}";
        
        [SerializeField] private Button playButton;
        [SerializeField] private TextMeshProUGUI levelText;
        
        private GameManager _gameManager;
        private LevelManager _levelManager;

        private void Awake()
        {
            _gameManager = ServiceLocator.ServiceLocator.Instance.GetService<GameManager>();
            _levelManager = ServiceLocator.ServiceLocator.Instance.GetService<LevelManager>();
        }

        private void Start()
        {
            var finished = _levelManager.IsLevelsCompleted();

            if (finished)
            {
                levelText.text = FinishedText;
            }
            else
            {
                playButton.onClick.AddListener(OnPlayButtonClicked);
                levelText.text = string.Format(LevelText, _levelManager.CurrentLevelNumber);
            }
        }

        private void OnPlayButtonClicked()
        {
            _gameManager.StartGame();
        }
    }
}