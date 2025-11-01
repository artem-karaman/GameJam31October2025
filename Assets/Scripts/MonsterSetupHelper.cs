using UnityEngine;

/// <summary>
/// Вспомогательный скрипт для правильной настройки монстра
/// Добавьте этот скрипт на префаб монстра или используйте его как чек-лист
/// </summary>
[RequireComponent(typeof(MonsterController))]
[RequireComponent(typeof(SpriteRenderer))]
public class MonsterSetupHelper : MonoBehaviour
{
    [ContextMenu("Setup Monster Components")]
    public void SetupMonsterComponents()
    {
        // Убеждаемся, что есть все необходимые компоненты
        
        // 1. MonsterController (обязательный)
        MonsterController monsterController = GetComponent<MonsterController>();
        if (monsterController == null)
        {
            monsterController = gameObject.AddComponent<MonsterController>();
            Debug.Log("✓ Добавлен MonsterController");
        }
        else
        {
            Debug.Log("✓ MonsterController уже есть");
        }
        
        // 2. SpriteRenderer (обязательный)
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            Debug.Log("✓ Добавлен SpriteRenderer");
        }
        else
        {
            Debug.Log("✓ SpriteRenderer уже есть");
        }
        
        // 3. CircleCollider2D (обязательный для обнаружения попаданий)
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.4f;
            collider.isTrigger = true;
            Debug.Log("✓ Добавлен CircleCollider2D с триггером");
        }
        else
        {
            collider.isTrigger = true; // Убеждаемся, что это триггер
            Debug.Log("✓ CircleCollider2D настроен как триггер");
        }
        
        // 4. Animator (опциональный, но рекомендуется)
        Animator animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = gameObject.AddComponent<Animator>();
            Debug.Log("✓ Добавлен Animator");
        }
        else
        {
            Debug.Log("✓ Animator уже есть");
        }
        
        // 5. Настраиваем тег (если нужно)
        // В рантайме создание тегов не работает, но можно проверить
        if (!gameObject.CompareTag("Untagged") && !gameObject.CompareTag("Monster"))
        {
            Debug.LogWarning("Рекомендуется установить тег 'Monster' для объекта в редакторе Unity");
        }
        
        Debug.Log("=== Настройка монстра завершена ===");
        Debug.Log("ВАЖНО: Убедитесь, что на объекте есть скрипт MonsterController!");
    }
    
    void Start()
    {
        // Автоматически вызываем настройку при старте, если нужно
        // Раскомментируйте следующую строку для автоматической настройки:
        // SetupMonsterComponents();
    }
}

