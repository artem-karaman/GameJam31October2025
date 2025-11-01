using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Вспомогательный скрипт для автоматической настройки префабов игрока и монстров на Canvas
/// Добавьте на префаб и используйте Context Menu для настройки
/// </summary>
public class PrefabSetupHelper : MonoBehaviour
{
    [ContextMenu("Setup as Player Prefab")]
    public void SetupAsPlayer()
    {
        // Добавляем/находим компонент игрока
        CastlePlayerUI player = GetComponent<CastlePlayerUI>();
        if (player == null)
        {
            player = gameObject.AddComponent<CastlePlayerUI>();
        }
        
        // Настраиваем компоненты игрока
        player.SetupPlayerComponents();
        
        // Удаляем этот helper скрипт после настройки (опционально)
        // Можно оставить для повторной настройки
        // DestroyImmediate(this);
        
        Debug.Log($"✓ Префаб игрока настроен: {gameObject.name}");
        Debug.Log("  Скрипт CastlePlayerUI добавлен и настроен");
        Debug.Log("  Теперь можете назначить свой Sprite в компоненте Image");
    }
    
    [ContextMenu("Setup as Monster Prefab")]
    public void SetupAsMonster()
    {
        // Добавляем/находим компонент монстра
        MonsterUI monster = GetComponent<MonsterUI>();
        if (monster == null)
        {
            monster = gameObject.AddComponent<MonsterUI>();
        }
        
        // Настраиваем компоненты монстра
        monster.SetupMonsterComponents();
        
        // Удаляем этот helper скрипт после настройки (опционально)
        // Можно оставить для повторной настройки
        // DestroyImmediate(this);
        
        Debug.Log($"✓ Префаб монстра настроен: {gameObject.name}");
        Debug.Log("  Скрипт MonsterUI добавлен и настроен");
        Debug.Log("  Теперь можете назначить свой Sprite в компоненте Image");
        Debug.Log("  Можете добавить Animator и назначить AnimatorController для анимаций");
    }
    
    [ContextMenu("Setup Player - Keep Helper")]
    public void SetupPlayerKeepHelper()
    {
        CastlePlayerUI player = GetComponent<CastlePlayerUI>();
        if (player == null)
        {
            player = gameObject.AddComponent<CastlePlayerUI>();
        }
        player.SetupPlayerComponents();
        Debug.Log($"✓ Префаб игрока настроен (helper сохранен): {gameObject.name}");
    }
    
    [ContextMenu("Setup Monster - Keep Helper")]
    public void SetupMonsterKeepHelper()
    {
        MonsterUI monster = GetComponent<MonsterUI>();
        if (monster == null)
        {
            monster = gameObject.AddComponent<MonsterUI>();
        }
        monster.SetupMonsterComponents();
        Debug.Log($"✓ Префаб монстра настроен (helper сохранен): {gameObject.name}");
    }
}

