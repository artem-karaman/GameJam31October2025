using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Спавнер монстров вокруг замка
/// </summary>
public class MonsterSpawner : MonoBehaviour
{
    public static MonsterSpawner Instance { get; private set; }
    
    [Header("Settings")]
    public GameObject monsterPrefab;
    public int monsterCount = 5;
    public float spawnRadius = 6f;
    public Vector2 castleCenter = Vector2.zero;
    
    [Header("Monster Pools")]
    public RuntimeAnimatorController[] availableAnimators;
    
    private List<GameObject> activeMonsters = new List<GameObject>();
    private Queue<GameObject> monsterPool = new Queue<GameObject>();
    
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
        SpawnMonsters();
    }
    
    void SpawnMonsters()
    {
        for (int i = 0; i < monsterCount; i++)
        {
            SpawnMonster();
        }
    }
    
    GameObject SpawnMonster()
    {
        GameObject monster;
        
        if (monsterPool.Count > 0)
        {
            monster = monsterPool.Dequeue();
            monster.SetActive(true);
        }
        else
        {
            monster = CreateMonster();
        }
        
        // Устанавливаем позицию на окружности вокруг замка
        // Монстры ходят на уровне земли (y = -8 или около того)
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float groundLevel = -8f; // Уровень земли
        Vector2 position = new Vector2(
            castleCenter.x + Mathf.Cos(angle) * spawnRadius,
            groundLevel + Mathf.Sin(angle) * spawnRadius * 0.3f // Немного варьируем Y для естественности
        );
        
        monster.transform.position = position;
        
        // Настраиваем монстра
        MonsterController controller = monster.GetComponent<MonsterController>();
        if (controller != null)
        {
            controller.centerPosition = castleCenter;
            controller.patrolRadius = spawnRadius;
            
            // Случайно выбираем аниматор
            if (availableAnimators != null && availableAnimators.Length > 0)
            {
                int animIndex = Random.Range(0, availableAnimators.Length);
                controller.SetAnimatorController(animIndex);
            }
        }
        
        activeMonsters.Add(monster);
        return monster;
    }
    
    GameObject CreateMonster()
    {
        GameObject monster;
        
        if (monsterPrefab != null)
        {
            monster = Instantiate(monsterPrefab);
        }
        else
        {
            // Создаем монстра программно
            monster = new GameObject("Monster");
            monster.AddComponent<MonsterController>();
        }
        
        return monster;
    }
    
    /// <summary>
    /// Возвращает монстра в пул
    /// </summary>
    public void ReturnMonster(GameObject monster)
    {
        if (monster == null) return;
        
        activeMonsters.Remove(monster);
        monsterPool.Enqueue(monster);
        monster.SetActive(false);
        
        // Спавним нового монстра
        StartCoroutine(SpawnMonsterDelayed());
    }
    
    System.Collections.IEnumerator SpawnMonsterDelayed()
    {
        yield return new WaitForSeconds(1f);
        SpawnMonster();
    }
    
    /// <summary>
    /// Для тестирования в редакторе - меняет анимации всех монстров
    /// Можно вызывать во время Play Mode для проверки разных анимаций
    /// </summary>
    [ContextMenu("Cycle Monster Animations")]
    public void CycleMonsterAnimations()
    {
        if (availableAnimators == null || availableAnimators.Length <= 1) return;
        
        foreach (var monster in activeMonsters)
        {
            if (monster == null || !monster.activeSelf) continue;
            
            MonsterController controller = monster.GetComponent<MonsterController>();
            if (controller != null && !controller.IsDead)
            {
                int newIndex = (controller.currentAnimatorIndex + 1) % availableAnimators.Length;
                controller.SetAnimatorController(newIndex);
            }
        }
    }
    
    /// <summary>
    /// Устанавливает конкретный индекс аниматора для всех монстров
    /// </summary>
    public void SetAllMonstersAnimator(int animatorIndex)
    {
        if (availableAnimators == null || animatorIndex < 0 || animatorIndex >= availableAnimators.Length) return;
        
        foreach (var monster in activeMonsters)
        {
            if (monster == null || !monster.activeSelf) continue;
            
            MonsterController controller = monster.GetComponent<MonsterController>();
            if (controller != null && !controller.IsDead)
            {
                controller.SetAnimatorController(animatorIndex);
            }
        }
    }
}

