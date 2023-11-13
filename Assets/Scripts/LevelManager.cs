using System;
using ServiceLocator;
using UnityEngine;

public enum LevelState
{
    None,
    Started,
    Finished
}

public class LevelManager : IService
{
    private readonly string _levelNumberKey = "LevelNumber";
    
    private LevelInfo[] _levels;
    private LevelInfo _currentLevel;
    private int _currentLevelIndex;
    private LevelState _levelState;
    
    public int CurrentLevelNumber => _currentLevelIndex + 1;
    
    public event Action<LevelInfo> OnLevelCreated;
    public event Action OnGameStarted;
    public event Action<bool> OnGameEnded;
    public event Action OnLevelChanged;
    private GameManager _gameManager;
    
    protected void Awake()
    {
        if (ServiceLocator.ServiceLocator.Instance.GetService<LevelManager>() != null
            && ServiceLocator.ServiceLocator.Instance.GetService<LevelManager>() != this)
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
        
        var levels = Resources.LoadAll<TextAsset>("Levels");
        _levels = new LevelInfo[levels.Length];
        for (var i = 0; i < levels.Length; i++)
        {
            _levels[i] = JsonUtility.FromJson<LevelInfo>(levels[i].text);
        }
        _currentLevelIndex = PlayerPrefs.GetInt(_levelNumberKey, 0);
        
        _gameManager = ServiceLocator.ServiceLocator.Instance.GetService<GameManager>();
        
        _gameManager.OnLevelLoaded += OnLevelLoaded;
    }
    
    private void OnLevelLoaded()
    {
        CreateLevel();
    }

    private void CreateLevel()
    {
        var levelInfo = GetCurrentLevel();
        _currentLevel = levelInfo;
        ServiceLocator.ServiceLocator.Instance.GetService<BoardManager>().CreateBoard(_currentLevel);
        _levelState = LevelState.Started;
        OnLevelCreated?.Invoke(levelInfo);
    }
     
    public LevelInfo GetCurrentLevel()
    {
        return _levels[_currentLevelIndex];
    }
    
    public void EndGame(bool success)
    {
        if (_levelState != LevelState.Started) return;
        
        if (!success)
        {
            _levelState = LevelState.Finished;
            OnGameEnded?.Invoke(false);
            return;
        }

        SetNextLevel();
        
        _levelState = LevelState.Finished;
        OnGameEnded?.Invoke(true);
    }

    public void UnloadLevel()
    {
        _levelState = LevelState.None;
        _gameManager.LoadMainMenu();
    }
    
    public void SetNextLevel()
    {
        _currentLevelIndex++;
        PlayerPrefs.SetInt(_levelNumberKey, _currentLevelIndex);
        OnLevelChanged?.Invoke();
    }
    
    public bool IsLevelsCompleted()
    {
        return _currentLevelIndex >= _levels.Length;
    }
}
