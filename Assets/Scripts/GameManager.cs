using System;
using ServiceLocator;
using UnityEngine.SceneManagement;

public class GameManager : IService
{
    public event Action OnMenuLoaded;
    public event Action OnLevelLoaded;
    public event Action OnAreaLoaded;

    private readonly string _mainSceneName = "MainScene";
    private readonly string _levelSceneName = "LevelScene";
    private readonly string _areaSceneName = "AreaScene";
    
    protected void Awake()
    {
        if (ServiceLocator.ServiceLocator.Instance.GetService<GameManager>() != null
            && ServiceLocator.ServiceLocator.Instance.GetService<GameManager>() != this)
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
        
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(_areaSceneName, LoadSceneMode.Additive);
    }

    private void OnSceneLoaded(Scene sceneName, LoadSceneMode arg1)
    {
        if (sceneName.name == _mainSceneName)
        {
            OnMenuLoaded?.Invoke();
        }
        else if (sceneName.name == _levelSceneName)
        {
            OnLevelLoaded?.Invoke();
        }
        else if (sceneName.name == _areaSceneName)
        {
            OnAreaLoaded?.Invoke();
        }
    }

    public void StartGame()
    {
        SceneManager.UnloadSceneAsync(_mainSceneName);
        SceneManager.LoadScene(_levelSceneName, LoadSceneMode.Additive);
    }

    public void LoadMainMenu()
    {
        SceneManager.UnloadSceneAsync(_levelSceneName);
        SceneManager.LoadScene(_mainSceneName, LoadSceneMode.Additive);
    }
}
