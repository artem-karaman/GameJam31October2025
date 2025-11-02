using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Управление UI игры: фон, HUD, счет
/// </summary>
public class CastleUIManager : MonoBehaviour
{
    public static CastleUIManager Instance { get; private set; }
    
    [Header("UI References")]
    public Canvas canvas;
    public Image backgroundImage;
    
    [Header("Settings")]
    public Color backgroundColor = new Color(0.2f, 0.4f, 0.6f);
    
    [Header("Victory Screen")]
    [Tooltip("Экран победы (показывается когда все монстры побеждены)")]
    public VictoryScreen victoryScreen;
    [Tooltip("Спрайт для экрана победы (можно назначить в инспекторе)")]
    public Sprite victorySprite;
    
    private int score = 0;
    private int monstersKilled = 0;
    private int totalMonstersToKill = 0;
    private bool isVictoryAchieved = false;
    
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
    
    void Start()
    {
        // Инициализируем экран победы
        SetupVictoryScreen();
        
        // Получаем общее количество монстров для победы
        UpdateTotalMonstersCount();
    }
    
    /// <summary>
    /// Настраивает экран победы
    /// </summary>
    void SetupVictoryScreen()
    {
        if (victoryScreen == null)
        {
            victoryScreen = FindObjectOfType<VictoryScreen>();
            if (victoryScreen == null)
            {
                GameObject victoryObj = new GameObject("VictoryScreen");
                
                // Создаем VictoryScreen (только Canvas версия)
                if (canvas != null)
                {
                    victoryObj.transform.SetParent(canvas.transform, false);
                    victoryScreen = victoryObj.AddComponent<VictoryScreen>();
                    victoryScreen.canvas = canvas;
                }
                else
                {
                    Debug.LogWarning("CastleUIManager: Canvas не найден для создания VictoryScreen!");
                    victoryScreen = victoryObj.AddComponent<VictoryScreen>();
                }
            }
        }
        
        if (victoryScreen != null && victorySprite != null)
        {
            victoryScreen.SetVictorySprite(victorySprite);
        }
    }
    
    /// <summary>
    /// Обновляет общее количество монстров для победы
    /// </summary>
    void UpdateTotalMonstersCount()
    {
        // Ищем спавнер монстров (проверяем оба варианта - SpriteRenderer и Canvas)
        MonsterSpawner spawner = MonsterSpawner.Instance;
        if (spawner != null)
        {
            totalMonstersToKill = spawner.monsterCount;
        }
        else
        {
            MonsterSpawnerUI spawnerUI = MonsterSpawnerUI.Instance;
            if (spawnerUI != null)
            {
                totalMonstersToKill = spawnerUI.monsterCount;
            }
        }
        
        Debug.Log($"CastleUIManager: Всего монстров для победы: {totalMonstersToKill}");
    }
    
    /// <summary>
    /// Обновляет счет убитых монстров и проверяет победу
    /// </summary>
    public void OnMonsterKilled()
    {
        // Не считаем монстров, если победа уже достигнута
        if (isVictoryAchieved) return;
        
        monstersKilled++;
        Debug.Log($"Монстр убит: {monstersKilled}/{totalMonstersToKill}");
        
        // Проверяем победу
        CheckVictory();
    }
    
    /// <summary>
    /// Проверяет условие победы (все монстры убиты) - можно вызвать вручную
    /// </summary>
    public void CheckVictory()
    {
        // Обновляем количество монстров на случай если оно изменилось
        if (totalMonstersToKill == 0)
        {
            UpdateTotalMonstersCount();
        }
        
        // Проверяем победу по количеству убитых монстров, а не по активным
        // (потому что спавнер постоянно создает новых)
        if (monstersKilled >= totalMonstersToKill && totalMonstersToKill > 0 && !isVictoryAchieved)
        {
            isVictoryAchieved = true;
            
            // Блокируем спавн новых монстров
            BlockMonsterSpawning();
            
            // Показываем экран победы
            ShowVictory();
        }
    }
    
    /// <summary>
    /// Блокирует спавн новых монстров после победы
    /// </summary>
    void BlockMonsterSpawning()
    {
        MonsterSpawner spawner = MonsterSpawner.Instance;
        if (spawner != null)
        {
            spawner.StopSpawning();
            Debug.Log("MonsterSpawner: Спавн заблокирован - победа достигнута!");
        }
        
        MonsterSpawnerUI spawnerUI = MonsterSpawnerUI.Instance;
        if (spawnerUI != null)
        {
            spawnerUI.StopSpawning();
            Debug.Log("MonsterSpawnerUI: Спавн заблокирован - победа достигнута!");
        }
    }
    
    /// <summary>
    /// Показывает экран победы
    /// </summary>
    void ShowVictory()
    {
        if (victoryScreen == null)
        {
            SetupVictoryScreen();
        }
        
        if (victoryScreen != null && !victoryScreen.IsVictoryShown)
        {
            Debug.Log("Все монстры побеждены! Показываю экран победы.");
            victoryScreen.ShowVictory();
        }
    }
    
    /// <summary>
    /// Скрывает экран победы
    /// </summary>
    public void HideVictory()
    {
        if (victoryScreen != null)
        {
            victoryScreen.HideVictory();
        }
    }
    
    public void ResetScore()
    {
        score = 0;
        monstersKilled = 0;
        totalMonstersToKill = 0;
        isVictoryAchieved = false;
        UpdateTotalMonstersCount();
        
        // Возобновляем спавн монстров
        MonsterSpawner spawner = MonsterSpawner.Instance;
        if (spawner != null)
        {
            spawner.ResumeSpawning();
        }
        
        MonsterSpawnerUI spawnerUI = MonsterSpawnerUI.Instance;
        if (spawnerUI != null)
        {
            spawnerUI.ResumeSpawning();
        }
        
        HideVictory();
    }
    
    public bool IsVictoryAchieved => isVictoryAchieved;
}


