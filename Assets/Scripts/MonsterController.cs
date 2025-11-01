using UnityEngine;
using System.Collections;

/// <summary>
/// Контроллер монстра, который ходит вокруг замка
/// </summary>
public class MonsterController : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    
    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public float patrolRadius = 5f;
    public Vector2 centerPosition = Vector2.zero;
    public bool enableScreenWrap = true; // Включает обтекание экрана
    
    [Header("Animation")]
    public RuntimeAnimatorController[] animatorControllers;
    private int _currentAnimatorIndex = 0;
    
    private Vector2 currentTarget;
    private bool isDead = false;
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        SetupMonster();
        SetRandomTarget();
    }
    
    void SetupMonster()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                CreateDefaultSprite();
            }
        }
        
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = gameObject.AddComponent<Animator>();
            }
        }
        
        // Применяем аниматор контроллер
        UpdateAnimator();
        
        // Добавляем коллайдер для обнаружения попаданий
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.4f;
            // Делаем триггер, чтобы не было физического столкновения, только обнаружение
            collider.isTrigger = true;
        }
        
        // Убеждаемся, что у объекта есть тег "Monster" для более точного обнаружения
        if (!gameObject.CompareTag("Monster"))
        {
            // Если тег не существует, создаем его (в рантайме это не работает, но для документации)
            // Лучше установить тег в редакторе или через префаб
        }
    }
    
    void Update()
    {
        if (isDead) return;
        
        MoveTowardsTarget();
        
        // Применяем обтекание экрана (wrap-around)
        if (enableScreenWrap)
        {
            WrapAroundScreen();
        }
    }
    
    void MoveTowardsTarget()
    {
        transform.position = Vector2.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);
        
        // Поворачиваем спрайт в сторону движения
        if (transform.position.x < currentTarget.x)
        {
            spriteRenderer.flipX = false;
        }
        else if (transform.position.x > currentTarget.x)
        {
            spriteRenderer.flipX = true;
        }
        
        // Если достигли цели, выбираем новую
        if (Vector2.Distance(transform.position, currentTarget) < 0.1f)
        {
            SetRandomTarget();
        }
    }
    
    /// <summary>
    /// Обтекание экрана - монстр появляется с противоположной стороны
    /// </summary>
    void WrapAroundScreen()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }
        
        // Получаем границы видимой области камеры
        float screenHeight = mainCamera.orthographicSize * 2f;
        float screenWidth = screenHeight * mainCamera.aspect;
        
        float leftBound = mainCamera.transform.position.x - screenWidth / 2f;
        float rightBound = mainCamera.transform.position.x + screenWidth / 2f;
        
        Vector3 pos = transform.position;
        
        // Если монстр вышел за левый край - появляется справа
        if (pos.x < leftBound)
        {
            pos.x = rightBound;
            transform.position = pos;
            // Обновляем цель, чтобы монстр продолжил движение
            SetRandomTarget();
        }
        // Если монстр вышел за правый край - появляется слева
        else if (pos.x > rightBound)
        {
            pos.x = leftBound;
            transform.position = pos;
            // Обновляем цель, чтобы монстр продолжил движение
            SetRandomTarget();
        }
    }
    
    void SetRandomTarget()
    {
        // Выбираем случайную точку на окружности вокруг замка
        // Монстры ходят на уровне земли (y около -8)
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float groundLevel = -8f; // Уровень земли
        currentTarget = new Vector2(
            centerPosition.x + Mathf.Cos(angle) * patrolRadius,
            groundLevel + Mathf.Sin(angle) * patrolRadius * 0.3f // Небольшое изменение Y
        );
    }
    
    /// <summary>
    /// Устанавливает аниматор контроллер по индексу
    /// </summary>
    public void SetAnimatorController(int index)
    {
        if (animatorControllers != null && index >= 0 && index < animatorControllers.Length)
        {
            _currentAnimatorIndex = index;
            UpdateAnimator();
        }
    }
    
    void UpdateAnimator()
    {
        if (animatorControllers != null && animatorControllers.Length > 0)
        {
            int indexToUse = Mathf.Clamp(_currentAnimatorIndex, 0, animatorControllers.Length - 1);
            if (animatorControllers[indexToUse] != null)
            {
                animator.runtimeAnimatorController = animatorControllers[indexToUse];
            }
        }
    }
    
    /// <summary>
    /// Убивает монстра (вызывается только при попадании крюка)
    /// </summary>
    public void Die()
    {
        if (isDead) return; // Уже мертв, не убиваем дважды
        
        isDead = true;
        Debug.Log($"Монстр {gameObject.name} умирает от попадания крюка");
        
        // Уведомляем UI Manager о смерти монстра
        if (CastleUIManager.Instance != null)
        {
            CastleUIManager.Instance.OnMonsterKilled();
        }
        
        StartCoroutine(DeathAnimation());
    }
    
    IEnumerator DeathAnimation()
    {
        // Простая анимация смерти - уменьшение и исчезновение
        float duration = 0.3f;
        Vector2 startScale = transform.localScale;
        Color startColor = spriteRenderer.color;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            transform.localScale = Vector2.Lerp(startScale, Vector2.zero, t);
            spriteRenderer.color = Color.Lerp(startColor, Color.clear, t);
            
            yield return null;
        }
        
        // Возвращаем в пул или уничтожаем
        if (MonsterSpawner.Instance != null)
        {
            MonsterSpawner.Instance.ReturnMonster(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void CreateDefaultSprite()
    {
        Texture2D texture = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        
        Color monsterColor = new Color(0.8f, 0.2f, 0.2f);
        
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float distFromCenter = Vector2.Distance(new Vector2(x, y), new Vector2(32, 32));
                
                if (distFromCenter < 25f)
                {
                    pixels[y * 64 + x] = monsterColor;
                }
                else if (distFromCenter < 28f)
                {
                    pixels[y * 64 + x] = Color.black;
                }
                else
                {
                    pixels[y * 64 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = Color.white;
    }
    
    public bool IsDead => isDead;
    
    public int currentAnimatorIndex => _currentAnimatorIndex;
    
    public void ResetMonster(Vector2 position, Vector2 center)
    {
        isDead = false;
        centerPosition = center;
        transform.position = position;
        transform.localScale = Vector2.one;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
        SetRandomTarget();
    }
}

