using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Спавнер монстров на Canvas с поддержкой пула разных типов монстров
/// </summary>
public class MonsterSpawnerUI : MonoBehaviour
{
    public static MonsterSpawnerUI Instance { get; private set; }
    
    [Header("Settings")]
    [Tooltip("Общее количество монстров на сцене")]
    public int monsterCount = 5;
    [Tooltip("Радиус патрулирования вокруг замка")]
    public float spawnRadius = 6f;
    [Tooltip("Центр замка (в единицах Canvas)")]
    public Vector2 castleCenter = Vector2.zero;
    
    [Header("Monster Pool System")]
    [Tooltip("Массив типов монстров (можно задать разные префабы)")]
    public MonsterPoolData[] monsterTypes = new MonsterPoolData[0];
    
    [Header("Legacy Support (для обратной совместимости)")]
    [Tooltip("Старый способ - один префаб (если массив типов пуст)")]
    public GameObject monsterPrefab;
    
    [Header("Monster Pools")]
    [Tooltip("Доступные аниматоры (применяются ко всем типам монстров)")]
    public RuntimeAnimatorController[] availableAnimators;
    
    private Canvas parentCanvas;
    private List<GameObject> activeMonsters = new List<GameObject>();
    
    // Пул для каждого типа монстра отдельно
    private Dictionary<GameObject, Queue<GameObject>> monsterPools = new Dictionary<GameObject, Queue<GameObject>>();
    
    // Словарь для быстрого поиска типа монстра по GameObject
    private Dictionary<GameObject, GameObject> monsterTypeMap = new Dictionary<GameObject, GameObject>();
    
    public List<GameObject> ActiveMonsters => activeMonsters;
    
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
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            parentCanvas = FindObjectOfType<Canvas>();
        }
        
        // Инициализируем пулы
        InitializePools();
        
        SpawnMonsters();
    }
    
    /// <summary>
    /// Инициализирует пулы для всех типов монстров
    /// </summary>
    void InitializePools()
    {
        // Проверяем, есть ли типы монстров
        if (monsterTypes == null || monsterTypes.Length == 0)
        {
            // Если нет, используем старый способ с одним префабом
            if (monsterPrefab != null)
            {
                Queue<GameObject> pool = new Queue<GameObject>();
                monsterPools[monsterPrefab] = pool;
                
                // Создаем предварительные объекты для пула
                for (int i = 0; i < 3; i++)
                {
                    GameObject precreated = CreateMonsterFromPrefab(monsterPrefab);
                    precreated.SetActive(false);
                    pool.Enqueue(precreated);
                }
            }
            return;
        }
        
        // Инициализируем пулы для каждого типа
        foreach (var monsterType in monsterTypes)
        {
            if (monsterType.monsterPrefab == null) continue;
            
            Queue<GameObject> pool = new Queue<GameObject>();
            monsterPools[monsterType.monsterPrefab] = pool;
            
            // Создаем минимальное количество для пула
            for (int i = 0; i < monsterType.poolMinSize; i++)
            {
                GameObject precreated = CreateMonsterFromPrefab(monsterType.monsterPrefab);
                precreated.SetActive(false);
                pool.Enqueue(precreated);
            }
            
            Debug.Log($"✓ Пул инициализирован для {monsterType.monsterTypeName}: {monsterType.poolMinSize} объектов");
        }
    }
    
    void SpawnMonsters()
    {
        for (int i = 0; i < monsterCount; i++)
        {
            SpawnMonster();
        }
    }
    
    /// <summary>
    /// Выбирает случайный тип монстра на основе весов спавна
    /// </summary>
    GameObject GetRandomMonsterPrefab()
    {
        // Если есть массив типов, используем его
        if (monsterTypes != null && monsterTypes.Length > 0)
        {
            // Фильтруем валидные типы
            var validTypes = monsterTypes.Where(t => t != null && t.monsterPrefab != null).ToArray();
            if (validTypes.Length == 0) return null;
            
            // Вычисляем общий вес
            int totalWeight = validTypes.Sum(t => t.spawnWeight);
            if (totalWeight == 0) return validTypes[0].monsterPrefab;
            
            // Выбираем случайный тип на основе весов
            int random = Random.Range(0, totalWeight);
            int currentWeight = 0;
            
            foreach (var type in validTypes)
            {
                currentWeight += type.spawnWeight;
                if (random < currentWeight)
                {
                    return type.monsterPrefab;
                }
            }
            
            return validTypes[validTypes.Length - 1].monsterPrefab;
        }
        
        // Иначе используем старый способ
        return monsterPrefab;
    }
    
    GameObject SpawnMonster()
    {
        GameObject prefab = GetRandomMonsterPrefab();
        if (prefab == null)
        {
            Debug.LogWarning("Не найден префаб монстра для спавна!");
            return null;
        }
        
        GameObject monster = null;
        
        // Пытаемся взять из пула
        if (monsterPools.ContainsKey(prefab))
        {
            Queue<GameObject> pool = monsterPools[prefab];
            if (pool.Count > 0)
            {
                monster = pool.Dequeue();
                monster.SetActive(true);
            }
        }
        
        // Если в пуле нет, создаем новый
        if (monster == null)
        {
            monster = CreateMonsterFromPrefab(prefab);
        }
        
        // Сохраняем связь монстра с его типом
        if (!monsterTypeMap.ContainsKey(monster))
        {
            monsterTypeMap[monster] = prefab;
        }
        
        // Устанавливаем позицию на окружности вокруг замка
        // Монстры ходят на уровне земли (y = 200px от низа экрана)
        // Распределяем монстров равномерно по окружности
        float angleStep = 360f / monsterCount;
        int currentIndex = activeMonsters.Count;
        float angle = (angleStep * currentIndex + Random.Range(-20f, 20f)) * Mathf.Deg2Rad;
        
        float groundLevel = 200f; // Уровень земли от низа Canvas
        Vector2 position = new Vector2(
            castleCenter.x + Mathf.Cos(angle) * spawnRadius * 100f, // Масштаб: 1 unit = 100px
            groundLevel + Mathf.Sin(angle) * spawnRadius * 30f // Небольшая вариация по Y
        );
        
        MonsterUI monsterUI = monster.GetComponent<MonsterUI>();
        if (monsterUI == null)
        {
            monsterUI = monster.AddComponent<MonsterUI>();
        }
        
        monsterUI.Position = position;
        // centerPosition должен быть в пикселях для Canvas (0,0 - центр Canvas)
        monsterUI.centerPosition = new Vector2(castleCenter.x * 100f, 0f); // Конвертируем в пиксели
        monsterUI.patrolRadius = spawnRadius;
        
        // Убеждаемся что у монстра есть RectTransform и он правильно настроен
        RectTransform monsterRect = monster.GetComponent<RectTransform>();
        if (monsterRect == null)
        {
            monsterRect = monster.AddComponent<RectTransform>();
        }
        
        // Настраиваем якоря для правильного позиционирования на Canvas
        monsterRect.anchorMin = new Vector2(0.5f, 0f);
        monsterRect.anchorMax = new Vector2(0.5f, 0f);
        monsterRect.pivot = new Vector2(0.5f, 0.5f);
        monsterRect.anchoredPosition = position;
        
        Debug.Log($"Монстр создан: позиция={position}, центр={monsterUI.centerPosition}, радиус патруля={spawnRadius}");
        
        if (availableAnimators != null && availableAnimators.Length > 0)
        {
            int animIndex = Random.Range(0, availableAnimators.Length);
            monsterUI.SetAnimatorController(animIndex);
        }
        
        // Убеждаемся что монстр на Canvas
        if (parentCanvas != null && monster.transform.parent != parentCanvas.transform)
        {
            monster.transform.SetParent(parentCanvas.transform, false);
        }
        
        // Проверяем что монстр не дублируется в списке
        if (!activeMonsters.Contains(monster))
        {
            activeMonsters.Add(monster);
        }
        
        // Обновляем счетчики
        UpdateActiveCounts();
        
        string monsterTypeName = "Unknown";
        if (monsterTypes != null && monsterTypes.Length > 0)
        {
            var type = monsterTypes.FirstOrDefault(t => t.monsterPrefab == prefab);
            if (type != null) monsterTypeName = type.monsterTypeName;
        }
        
        Debug.Log($"Спавн монстра #{currentIndex + 1} (тип: {monsterTypeName}) на позиции {position}, всего активных: {activeMonsters.Count}");
        
        return monster;
    }
    
    /// <summary>
    /// Создает монстра из указанного префаба
    /// </summary>
    GameObject CreateMonsterFromPrefab(GameObject prefab)
    {
        if (prefab == null)
        {
            // Если префаб не указан, создаем монстра программно
            GameObject mons = new GameObject("Monster");
            MonsterUI monUI = mons.AddComponent<MonsterUI>();
            monUI.SetupMonsterComponents();
            
            if (parentCanvas != null)
            {
                mons.transform.SetParent(parentCanvas.transform, false);
            }
            
            return mons;
        }
        
        // Создаем из префаба
        var monster = Instantiate(prefab);
        
        // Если префаб не настроен, автоматически настраиваем
        MonsterUI monsterUI = monster.GetComponent<MonsterUI>();
        if (monsterUI == null)
        {
            monsterUI = monster.AddComponent<MonsterUI>();
        }
        
        // Автоматически настраиваем компоненты если нужно
        monsterUI.SetupMonsterComponents();
        
        // Убеждаемся что монстр на Canvas
        if (parentCanvas != null)
        {
            monster.transform.SetParent(parentCanvas.transform, false);
        }
        
        return monster;
    }
    
    /// <summary>
    /// Возвращает монстра в соответствующий пул
    /// </summary>
    public void ReturnMonster(GameObject monster)
    {
        if (monster == null) return;
        
        activeMonsters.Remove(monster);
        
        // Находим тип монстра и возвращаем в соответствующий пул
        if (monsterTypeMap.ContainsKey(monster))
        {
            GameObject prefab = monsterTypeMap[monster];
            
            if (monsterPools.ContainsKey(prefab))
            {
                monsterPools[prefab].Enqueue(monster);
            }
            else
            {
                // Создаем новый пул если его нет
                Queue<GameObject> newPool = new Queue<GameObject>();
                newPool.Enqueue(monster);
                monsterPools[prefab] = newPool;
            }
            
            monsterTypeMap.Remove(monster);
        }
        else
        {
            // Если тип не определен, пробуем найти по старому способу
            if (monsterPrefab != null)
            {
                if (!monsterPools.ContainsKey(monsterPrefab))
                {
                    monsterPools[monsterPrefab] = new Queue<GameObject>();
                }
                monsterPools[monsterPrefab].Enqueue(monster);
            }
            else
            {
                // Если нет пула, просто деактивируем
                Destroy(monster);
                StartCoroutine(SpawnMonsterDelayed());
                return;
            }
        }
        
        monster.SetActive(false);
        
        // Обновляем счетчик активных монстров для типа
        UpdateActiveCounts();
        
        StartCoroutine(SpawnMonsterDelayed());
    }
    
    /// <summary>
    /// Обновляет счетчики активных монстров для каждого типа
    /// </summary>
    void UpdateActiveCounts()
    {
        if (monsterTypes == null) return;
        
        foreach (var type in monsterTypes)
        {
            type.activeCount = activeMonsters.Count(m => 
                m != null && monsterTypeMap.ContainsKey(m) && monsterTypeMap[m] == type.monsterPrefab);
        }
    }
    
    System.Collections.IEnumerator SpawnMonsterDelayed()
    {
        yield return new WaitForSeconds(1f);
        SpawnMonster();
    }
    
    [ContextMenu("Cycle Monster Animations")]
    public void CycleMonsterAnimations()
    {
        if (availableAnimators == null || availableAnimators.Length <= 1) return;
        
        foreach (var monster in activeMonsters)
        {
            if (monster == null || !monster.activeSelf) continue;
            
            MonsterUI monsterUI = monster.GetComponent<MonsterUI>();
            if (monsterUI != null && !monsterUI.IsDead)
            {
                int newIndex = (monsterUI.currentAnimatorIndex + 1) % availableAnimators.Length;
                monsterUI.SetAnimatorController(newIndex);
            }
        }
    }
    
    /// <summary>
    /// Получает информацию о состоянии пулов (для отладки)
    /// </summary>
    [ContextMenu("Print Pool Status")]
    public void PrintPoolStatus()
    {
        Debug.Log("=== Статус пулов монстров ===");
        Debug.Log($"Всего активных монстров: {activeMonsters.Count}");
        Debug.Log($"Всего монстров в пулах: {monsterPools.Values.Sum(p => p.Count)}");
        
        if (monsterTypes != null && monsterTypes.Length > 0)
        {
            foreach (var type in monsterTypes)
            {
                if (type == null || type.monsterPrefab == null) continue;
                
                int inPool = 0;
                if (monsterPools.ContainsKey(type.monsterPrefab))
                {
                    inPool = monsterPools[type.monsterPrefab].Count;
                }
                
                Debug.Log($"{type.monsterTypeName}: активных={type.activeCount}, в пуле={inPool}, вес={type.spawnWeight}");
            }
        }
    }
}

