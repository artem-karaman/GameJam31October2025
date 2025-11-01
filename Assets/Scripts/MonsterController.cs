using UnityEngine;

/// <summary>
/// Контроллер монстра со SpriteRenderer
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class MonsterController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public float patrolRadius = 5f;
    public Vector2 centerPosition = Vector2.zero;
    public bool enableScreenWrap = true;
    
    [Header("Animation")]
    public RuntimeAnimatorController[] animatorControllers;
    private int _currentAnimatorIndex = 0;
    
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Vector3 currentTarget;
    private bool isDead = false;
    private Camera mainCamera;
    
    void Awake()
    {
        SetupMonsterComponents();
    }
    
    void Start()
    {
        mainCamera = Camera.main;
        
        // Если цель не установлена, устанавливаем её
        if (currentTarget == Vector3.zero)
        {
            SetRandomTarget();
        }
    }
    
    /// <summary>
    /// Автоматически настраивает компоненты монстра
    /// </summary>
    [ContextMenu("Setup Monster Components")]
    public void SetupMonsterComponents()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        spriteRenderer.sortingOrder = 4;
        
        // Animator (опционально)
        animator = GetComponent<Animator>();
        // Не создаем автоматически, только если уже есть
        
        // Применяем аниматор контроллер
        UpdateAnimator();
        
        // Создаем спрайт только если он не назначен вручную
        if (spriteRenderer.sprite == null)
        {
            CreateDefaultSprite();
        }
    }
    
    void Update()
    {
        if (isDead) return;
        
        MoveTowardsTarget();
        
        // Применяем обтекание экрана
        if (enableScreenWrap)
        {
            WrapAroundScreen();
        }
    }
    
    void MoveTowardsTarget()
    {
        if (spriteRenderer == null) return;
        
        // Движение в мировых координатах
        transform.position = Vector3.MoveTowards(
            transform.position, 
            currentTarget, 
            moveSpeed * Time.deltaTime
        );
        
        // Поворачиваем спрайт в сторону движения
        float currentX = transform.position.x;
        float targetX = currentTarget.x;
        
        if (currentX < targetX)
        {
            transform.localScale = new Vector3(1, 1, 1); // Смотрим вправо
        }
        else if (currentX > targetX)
        {
            transform.localScale = new Vector3(-1, 1, 1); // Смотрим влево
        }
        
        // Если достигли цели или очень близко, выбираем новую
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget);
        if (distanceToTarget < 0.2f)
        {
            SetRandomTarget();
        }
    }
    
    void WrapAroundScreen()
    {
        if (mainCamera == null) return;
        
        // Получаем границы экрана в мировых координатах
        float screenHeight = 2f * mainCamera.orthographicSize;
        float screenWidth = screenHeight * mainCamera.aspect;
        
        float leftBound = mainCamera.transform.position.x - screenWidth / 2f;
        float rightBound = mainCamera.transform.position.x + screenWidth / 2f;
        
        Vector3 pos = transform.position;
        
        // Если монстр вышел за левый край - появляется справа
        if (pos.x < leftBound)
        {
            pos.x = rightBound - 0.5f; // Немного отступ от края
            transform.position = pos;
            SetRandomTarget();
        }
        // Если монстр вышел за правый край - появляется слева
        else if (pos.x > rightBound)
        {
            pos.x = leftBound + 0.5f; // Немного отступ от края
            transform.position = pos;
            SetRandomTarget();
        }
    }
    
    void SetRandomTarget()
    {
        // Выбираем случайную точку на окружности вокруг центра замка
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float groundLevel = centerPosition.y; // Уровень земли
        
        // Вычисляем целевую позицию на окружности
        float radiusInWorld = patrolRadius;
        currentTarget = new Vector3(
            centerPosition.x + Mathf.Cos(angle) * radiusInWorld,
            groundLevel + Mathf.Sin(angle) * radiusInWorld * 0.3f,
            0
        );
        
        // Ограничиваем целевую позицию в разумных пределах экрана
        if (mainCamera != null)
        {
            float screenHeight = 2f * mainCamera.orthographicSize;
            float screenWidth = screenHeight * mainCamera.aspect;
            float leftBound = mainCamera.transform.position.x - screenWidth / 2f;
            float rightBound = mainCamera.transform.position.x + screenWidth / 2f;
            
            currentTarget.x = Mathf.Clamp(currentTarget.x, leftBound + 1f, rightBound - 1f);
            currentTarget.y = Mathf.Clamp(currentTarget.y, groundLevel - 1f, groundLevel + 3f);
        }
    }
    
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
        if (animator != null && animatorControllers != null && animatorControllers.Length > 0)
        {
            int indexToUse = Mathf.Clamp(_currentAnimatorIndex, 0, animatorControllers.Length - 1);
            if (animatorControllers[indexToUse] != null)
            {
                animator.runtimeAnimatorController = animatorControllers[indexToUse];
            }
        }
    }
    
    public void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log($"Монстр {gameObject.name} умирает от попадания крюка");
        
        // Уведомляем UI Manager о смерти монстра
        if (CastleUIManager.Instance != null)
        {
            CastleUIManager.Instance.OnMonsterKilled();
        }
        
        StartCoroutine(DeathAnimation());
    }
    
    System.Collections.IEnumerator DeathAnimation()
    {
        float duration = 0.3f;
        Vector3 startScale = transform.localScale;
        Color startColor = spriteRenderer.color;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
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
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 100f);
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = Color.white;
    }
    
    public bool IsDead => isDead;
    public int currentAnimatorIndex => _currentAnimatorIndex;
    
    public Vector3 Position
    {
        get { return transform.position; }
        set { transform.position = value; }
    }
}
