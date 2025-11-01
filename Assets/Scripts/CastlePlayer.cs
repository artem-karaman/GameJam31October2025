using UnityEngine;

/// <summary>
/// Контроллер игрока на вершине замка (SpriteRenderer версия)
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class CastlePlayer : MonoBehaviour
{
    [Header("Settings")]
    public float castleTopHeight = 3f;
    public Vector2 castlePosition = Vector2.zero;
    
    [Header("Fishing Animation")]
    [Tooltip("Время анимации замаха (в секундах)")]
    [Range(0.1f, 2f)]
    public float windupDuration = 0.5f;
    [Tooltip("Угол наклона при замахе (в градусах)")]
    [Range(0f, 45f)]
    public float windupAngle = 25f;
    [Tooltip("Смещение при замахе назад (в мировых единицах)")]
    [Range(0f, 0.5f)]
    public float windupBackOffset = 0.2f;
    
    private SpriteRenderer spriteRenderer;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isWindingUp = false;
    private float windupProgress = 0f;
    
    void Awake()
    {
        SetupPlayerComponents();
    }
    
    void Start()
    {
        SetupPlayer();
    }
    
    /// <summary>
    /// Автоматически настраивает компоненты игрока
    /// </summary>
    [ContextMenu("Setup Player Components")]
    public void SetupPlayerComponents()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        spriteRenderer.sortingOrder = 5;
        
        if (spriteRenderer.sprite == null)
        {
            CreatePlayerSprite();
        }
    }
    
    void SetupPlayer()
    {
        // Позиционируем на вершине замка
        transform.position = new Vector3(castlePosition.x, castlePosition.y + castleTopHeight, 0);
        originalPosition = transform.position;
        originalRotation = transform.localRotation;
    }
    
    void Update()
    {
        if (isWindingUp)
        {
            UpdateWindupAnimation();
        }
        else if (windupProgress > 0f)
        {
            // Возвращаемся в исходное положение
            windupProgress = Mathf.Max(0f, windupProgress - Time.deltaTime * 2f);
            ApplyWindupTransform(windupProgress);
        }
    }
    
    /// <summary>
    /// Начинает анимацию замаха
    /// </summary>
    public void StartWindup()
    {
        if (spriteRenderer == null) return;
        
        isWindingUp = true;
        originalPosition = transform.position;
        originalRotation = transform.localRotation;
    }
    
    /// <summary>
    /// Обновляет анимацию замаха
    /// </summary>
    void UpdateWindupAnimation()
    {
        if (spriteRenderer == null) return;
        
        windupProgress += Time.deltaTime / windupDuration;
        windupProgress = Mathf.Clamp01(windupProgress);
        
        ApplyWindupTransform(windupProgress);
    }
    
    /// <summary>
    /// Применяет трансформацию замаха
    /// </summary>
    void ApplyWindupTransform(float progress)
    {
        if (spriteRenderer == null) return;
        
        // Кривая анимации (ease-out для плавности)
        float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
        
        // Наклон назад и вверх
        float angle = -windupAngle * easedProgress;
        transform.localRotation = Quaternion.Euler(0, 0, angle);
        
        // Небольшое смещение назад при замахе
        Vector3 offset = new Vector3(-windupBackOffset * easedProgress, windupBackOffset * 0.5f * easedProgress, 0);
        transform.position = originalPosition + offset;
    }
    
    /// <summary>
    /// Завершает замах и делает бросок
    /// </summary>
    public void Cast(float castPower = 1f)
    {
        if (spriteRenderer == null) return;
        
        isWindingUp = false;
        
        // Анимация броска - быстрое движение вперед
        StartCoroutine(CastAnimation(castPower));
    }
    
    /// <summary>
    /// Анимация броска крюка
    /// </summary>
    System.Collections.IEnumerator CastAnimation(float power)
    {
        float castDuration = 0.2f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.localRotation;
        
        // Быстрое движение вперед и вниз (бросок)
        while (elapsed < castDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / castDuration;
            
            // Движение вперед при броске
            float forwardOffset = windupBackOffset * 1.5f * (1f - t) * power;
            float downOffset = windupBackOffset * 0.3f * (1f - t) * power;
            
            transform.position = originalPosition + new Vector3(forwardOffset, -downOffset, 0);
            
            // Возврат угла
            float angle = -windupAngle * (1f - t);
            transform.localRotation = Quaternion.Euler(0, 0, angle);
            
            yield return null;
        }
        
        // Возвращаемся в исходное положение
        while (windupProgress > 0f)
        {
            windupProgress = Mathf.Max(0f, windupProgress - Time.deltaTime * 3f);
            ApplyWindupTransform(windupProgress);
            yield return null;
        }
        
        // Финальная позиция
        transform.position = originalPosition;
        transform.localRotation = originalRotation;
    }
    
    /// <summary>
    /// Прерывает замах без броска
    /// </summary>
    public void CancelWindup()
    {
        isWindingUp = false;
        // windupProgress будет постепенно уменьшаться в Update
    }
    
    void CreatePlayerSprite()
    {
        Texture2D texture = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        
        Color playerColor = new Color(0.2f, 0.6f, 0.9f);
        
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float distFromCenter = Vector2.Distance(new Vector2(x, y), new Vector2(32, 32));
                
                if (distFromCenter < 25f)
                {
                    pixels[y * 64 + x] = playerColor;
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
    
    public Vector2 Position
    {
        get { return transform.position; }
        set { transform.position = value; }
    }
}

