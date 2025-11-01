using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Контроллер игрока как UI элемента на Canvas
/// Автоматически настраивает все необходимые компоненты для работы на Canvas
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class CastlePlayerUI : MonoBehaviour
{
    [Header("Settings")]
    public float castleTopHeight = 3f;
    public Vector2 castlePosition = Vector2.zero;
    
    [Header("References")]
    [Tooltip("Картинка игрока (автоматически найдет если не назначена)")]
    public Image playerImage;
    
    [Header("Fishing Animation")]
    [Tooltip("Время анимации замаха (в секундах)")]
    [Range(0.1f, 2f)]
    public float windupDuration = 0.5f;
    [Tooltip("Угол наклона при замахе (в градусах)")]
    [Range(0f, 45f)]
    public float windupAngle = 25f;
    [Tooltip("Смещение при замахе назад (в пикселях)")]
    [Range(0f, 50f)]
    public float windupBackOffset = 20f;
    
    private RectTransform rectTransform;
    private Vector2 originalPosition;
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
    /// Автоматически настраивает все компоненты для работы на Canvas
    /// </summary>
    [ContextMenu("Setup Player Components")]
    public void SetupPlayerComponents()
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
        rectTransform.sizeDelta = new Vector2(80, 80);
        
        // Image (обязателен для отображения)
        if (playerImage == null)
        {
            playerImage = GetComponent<Image>();
        }
        
        if (playerImage == null)
        {
            playerImage = gameObject.AddComponent<Image>();
        }
        
        // Создаем спрайт только если он не назначен вручную
        // Если спрайт уже есть (из префаба или назначен вручную), используем его
        if (playerImage.sprite == null)
        {
            CreatePlayerSprite();
        }
    }
    
    void SetupPlayer()
    {
        // Позиционируем на вершине замка
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector2(castlePosition.x, 1100f); // На вершине замка (в пикселях от низа)
            originalPosition = rectTransform.anchoredPosition;
            originalRotation = rectTransform.localRotation;
        }
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
    /// Начинает анимацию замаха (как рыбак замахивается удочкой)
    /// </summary>
    public void StartWindup()
    {
        if (rectTransform == null) return;
        
        isWindingUp = true;
        originalPosition = rectTransform.anchoredPosition;
        originalRotation = rectTransform.localRotation;
    }
    
    /// <summary>
    /// Обновляет анимацию замаха
    /// </summary>
    void UpdateWindupAnimation()
    {
        if (rectTransform == null) return;
        
        windupProgress += Time.deltaTime / windupDuration;
        windupProgress = Mathf.Clamp01(windupProgress);
        
        ApplyWindupTransform(windupProgress);
    }
    
    /// <summary>
    /// Применяет трансформацию замаха
    /// </summary>
    void ApplyWindupTransform(float progress)
    {
        if (rectTransform == null) return;
        
        // Кривая анимации (ease-out для плавности)
        float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
        
        // Наклон назад и вверх (как рыбак замахивается)
        float angle = -windupAngle * easedProgress;
        rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
        
        // Небольшое смещение назад при замахе
        Vector2 offset = new Vector2(-windupBackOffset * easedProgress, windupBackOffset * 0.5f * easedProgress);
        rectTransform.anchoredPosition = originalPosition + offset;
    }
    
    /// <summary>
    /// Завершает замах и делает бросок
    /// </summary>
    public void Cast(float castPower = 1f)
    {
        if (rectTransform == null) return;
        
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
        Vector2 startPos = rectTransform.anchoredPosition;
        Quaternion startRot = rectTransform.localRotation;
        
        // Быстрое движение вперед и вниз (бросок)
        while (elapsed < castDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / castDuration;
            
            // Движение вперед при броске
            float forwardOffset = windupBackOffset * 1.5f * (1f - t) * power;
            float downOffset = windupBackOffset * 0.3f * (1f - t) * power;
            
            rectTransform.anchoredPosition = originalPosition + new Vector2(forwardOffset, -downOffset);
            
            // Возврат угла
            float angle = -windupAngle * (1f - t);
            rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
            
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
        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localRotation = originalRotation;
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
        playerImage.sprite = sprite;
        playerImage.color = Color.white;
    }
    
    public Vector2 Position
    {
        get { return rectTransform.anchoredPosition; }
        set { rectTransform.anchoredPosition = value; }
    }
}

