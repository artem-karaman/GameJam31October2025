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
    }
}

