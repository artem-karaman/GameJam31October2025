using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Контроллер монстра как UI элемента на Canvas
/// Автоматически настраивает все необходимые компоненты для работы на Canvas
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class MonsterUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Картинка монстра (автоматически найдет если не назначена)")]
    public Image monsterImage;
    [Tooltip("Аниматор для монстра (опционально)")]
    public Animator animator;
    
    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public float patrolRadius = 5f;
    public Vector2 centerPosition = Vector2.zero;
    public bool enableScreenWrap = true;
    
    [Header("Animation")]
    public RuntimeAnimatorController[] animatorControllers;
    private int _currentAnimatorIndex = 0;
    
    private RectTransform rectTransform;
    private Vector2 currentTarget;
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
        if (currentTarget == Vector2.zero)
        {
            SetRandomTarget();
        }
    }
    
    /// <summary>
    /// Автоматически настраивает все компоненты для работы на Canvas
    /// </summary>
    [ContextMenu("Setup Monster Components")]
    public void SetupMonsterComponents()
    {
        // RectTransform (обязателен)
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }
        
        // Настраиваем якоря для Canvas
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        // Размер монстра
        rectTransform.sizeDelta = new Vector2(64, 64);
        
        // Image (обязателен для отображения)
        if (monsterImage == null)
        {
            monsterImage = GetComponent<Image>();
        }
        
        if (monsterImage == null)
        {
            monsterImage = gameObject.AddComponent<Image>();
        }
        
        // Animator (опционально, но создаем если нужен)
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            // Не создаем автоматически, только если уже есть
        }
        
        // Применяем аниматор контроллер
        UpdateAnimator();
        
        // Создаем спрайт только если он не назначен вручную
        // Если спрайт уже есть (из префаба или назначен вручную), используем его
        if (monsterImage.sprite == null)
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
        if (rectTransform == null) return;
        
        // Движение по Canvas в пикселях
        float pixelsPerSecond = moveSpeed * 100f; // 1 unit = 100px
        rectTransform.anchoredPosition = Vector2.MoveTowards(
            rectTransform.anchoredPosition, 
            currentTarget, 
            pixelsPerSecond * Time.deltaTime
        );
        
        // Поворачиваем спрайт в сторону движения
        float currentX = rectTransform.anchoredPosition.x;
        float targetX = currentTarget.x;
        
        if (currentX < targetX)
        {
            rectTransform.localScale = new Vector3(1, 1, 1); // Смотрим вправо
        }
        else if (currentX > targetX)
        {
            rectTransform.localScale = new Vector3(-1, 1, 1); // Смотрим влево
        }
        
        // Если достигли цели или очень близко, выбираем новую
        float distanceToTarget = Vector2.Distance(rectTransform.anchoredPosition, currentTarget);
        if (distanceToTarget < 20f) // Порог в пикселях
        {
            SetRandomTarget();
        }
    }
    
    void WrapAroundScreen()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        
        // Для ScreenSpaceOverlay используем размер reference resolution
        float screenWidth = 1080f; // Reference resolution width в пикселях
        float leftBound = -screenWidth / 2f;
        float rightBound = screenWidth / 2f;
        
        Vector2 pos = rectTransform.anchoredPosition;
        
        // Если монстр вышел за левый край - появляется справа
        if (pos.x < leftBound)
        {
            pos.x = rightBound - 50f; // Немного отступ от края
            rectTransform.anchoredPosition = pos;
            SetRandomTarget();
        }
        // Если монстр вышел за правый край - появляется слева
        else if (pos.x > rightBound)
        {
            pos.x = leftBound + 50f; // Немного отступ от края
            rectTransform.anchoredPosition = pos;
            SetRandomTarget();
        }
    }
    
    void SetRandomTarget()
    {
        // Выбираем случайную точку на окружности вокруг центра замка
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float groundLevel = 200f; // Уровень земли от низа Canvas (в пикселях)
        
        // centerPosition уже должно быть в пикселях (устанавливается в MonsterSpawnerUI)
        // Центр замка на Canvas: x = 0 (центр экрана)
        float centerX = centerPosition.x;
        
        // Вычисляем целевую позицию на окружности
        float radiusInPixels = patrolRadius * 100f; // Конвертируем радиус в пиксели
        currentTarget = new Vector2(
            centerX + Mathf.Cos(angle) * radiusInPixels, // X позиция на окружности
            groundLevel + Mathf.Sin(angle) * radiusInPixels * 0.3f // Небольшая вариация по Y
        );
        
        // Ограничиваем целевую позицию в разумных пределах экрана
        currentTarget.x = Mathf.Clamp(currentTarget.x, -500f, 500f);
        currentTarget.y = Mathf.Clamp(currentTarget.y, 150f, 400f);
        
        // Debug.Log($"Монстр {gameObject.name} выбрал новую цель: {currentTarget}"); // Можно включить для отладки
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
        if (animatorControllers != null && animatorControllers.Length > 0)
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
        Vector2 startScale = rectTransform.localScale;
        Color startColor = monsterImage.color;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            rectTransform.localScale = Vector2.Lerp(startScale, Vector2.zero, t);
            monsterImage.color = Color.Lerp(startColor, Color.clear, t);
            
            yield return null;
        }
        
        // Возвращаем в пул или уничтожаем
        if (MonsterSpawnerUI.Instance != null)
        {
            MonsterSpawnerUI.Instance.ReturnMonster(gameObject);
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
        monsterImage.sprite = sprite;
        monsterImage.color = Color.white;
    }
    
    public bool IsDead => isDead;
    public int currentAnimatorIndex => _currentAnimatorIndex;
    
    public Vector2 Position
    {
        get { return rectTransform.anchoredPosition; }
        set { rectTransform.anchoredPosition = value; }
    }
}

