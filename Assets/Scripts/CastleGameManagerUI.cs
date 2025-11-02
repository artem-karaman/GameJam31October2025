using UnityEngine;

/// <summary>
/// Главный менеджер игры с UI элементами на Canvas
/// </summary>
public class CastleGameManagerUI : MonoBehaviour
{
    public static CastleGameManagerUI Instance { get; private set; }
    
    [Header("References")]
    public CastlePlayerUI player;
    public HookUI hook;
    public CastleGameTouchControllerUI touchController;
    public MonsterSpawnerUI monsterSpawner;
    public ArrowIndicatorUI arrowIndicator;
    public Canvas canvas;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        // Для тестирования - можно менять анимации монстров во время игры
        if (Input.GetKeyDown(KeyCode.C) && monsterSpawner != null)
        {
            monsterSpawner.CycleMonsterAnimations();
        }
        
        // Периодически проверяем победу (на случай если монстры исчезли другим способом)
        // Проверяем не каждый кадр, а раз в секунду
        if (Time.frameCount % 60 == 0 && CastleUIManager.Instance != null && monsterSpawner != null)
        {
            CheckVictoryCondition();
        }
    }
    
    /// <summary>
    /// Проверяет условие победы (все монстры побеждены)
    /// </summary>
    void CheckVictoryCondition()
    {
        if (monsterSpawner == null || monsterSpawner.ActiveMonsters == null) return;
        
        // Если активных монстров нет и игра идет, проверяем победу
        if (monsterSpawner.ActiveMonsters.Count == 0)
        {
            CastleUIManager.Instance?.CheckVictory();
        }
    }
}

