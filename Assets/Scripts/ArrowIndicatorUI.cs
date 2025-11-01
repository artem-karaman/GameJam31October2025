using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Визуализация стрелки/линии направления на Canvas при свайпе
/// </summary>
public class ArrowIndicatorUI : MonoBehaviour
{
    [Header("References")]
    public Image arrowLineImage;
    public Image arrowHeadImage;
    
    [Header("Settings")]
    public float arrowWidth = 20f; // Толщина линии в пикселях
    public float headSize = 40f; // Размер наконечника
    public Color arrowColor = new Color(1f, 0.3f, 0.3f, 0.8f); // Красный с прозрачностью
    
    private RectTransform lineRect;
    private RectTransform headRect;
    private Canvas parentCanvas;
    private bool isVisible = false;
    private Vector2 hookPosition;
    private Vector2 targetPosition;
    
    void Start()
    {
        SetupArrow();
    }
    
    void SetupArrow()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            parentCanvas = FindObjectOfType<Canvas>();
        }
        
        // Создаем линию стрелки
        if (arrowLineImage == null)
        {
            GameObject lineObj = new GameObject("ArrowLine");
            if (parentCanvas != null)
            {
                lineObj.transform.SetParent(parentCanvas.transform);
            }
            else
            {
                lineObj.transform.SetParent(transform);
            }
            
            lineRect = lineObj.AddComponent<RectTransform>();
            lineRect.sizeDelta = new Vector2(arrowWidth, 100);
            lineRect.pivot = new Vector2(0.5f, 0f);
            
            arrowLineImage = lineObj.AddComponent<Image>();
            arrowLineImage.color = arrowColor;
            
            // Создаем спрайт для линии
            CreateLineSprite();
        }
        else
        {
            lineRect = arrowLineImage.GetComponent<RectTransform>();
        }
        
        // Создаем наконечник стрелки
        if (arrowHeadImage == null)
        {
            GameObject headObj = new GameObject("ArrowHead");
            if (parentCanvas != null)
            {
                headObj.transform.SetParent(parentCanvas.transform);
            }
            else
            {
                headObj.transform.SetParent(transform);
            }
            
            headRect = headObj.AddComponent<RectTransform>();
            headRect.sizeDelta = new Vector2(headSize, headSize);
            headRect.pivot = new Vector2(0.5f, 0.5f);
            
            arrowHeadImage = headObj.AddComponent<Image>();
            arrowHeadImage.color = arrowColor;
            
            // Создаем спрайт наконечника
            CreateArrowHeadSprite();
        }
        else
        {
            headRect = arrowHeadImage.GetComponent<RectTransform>();
        }
        
        Hide();
    }
    
    /// <summary>
    /// Показывает стрелку от игрока до цели (в координатах Canvas)
    /// </summary>
    public void Show(Vector2 hookPos, Vector2 targetPos)
    {
        isVisible = true;
        hookPosition = hookPos;
        targetPosition = targetPos;
        
        if (arrowLineImage != null)
        {
            arrowLineImage.enabled = true;
        }
        if (arrowHeadImage != null)
        {
            arrowHeadImage.enabled = true;
        }
        
        UpdateArrow();
    }
    
    /// <summary>
    /// Скрывает стрелку
    /// </summary>
    public void Hide()
    {
        isVisible = false;
        if (arrowLineImage != null)
        {
            arrowLineImage.enabled = false;
        }
        if (arrowHeadImage != null)
        {
            arrowHeadImage.enabled = false;
        }
    }
    
    void Update()
    {
        if (isVisible)
        {
            UpdateArrow();
        }
    }
    
    void UpdateArrow()
    {
        if (lineRect == null || headRect == null) return;
        
        Vector2 direction = targetPosition - hookPosition;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Обновляем линию
        lineRect.sizeDelta = new Vector2(arrowWidth, distance);
        lineRect.anchoredPosition = (hookPosition + targetPosition) / 2f;
        lineRect.rotation = Quaternion.Euler(0, 0, angle - 90f);
        
        // Обновляем наконечник
        headRect.anchoredPosition = targetPosition;
        headRect.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }
    
    void CreateLineSprite()
    {
        int width = 20;
        int height = 100;
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Простая линия с закругленными краями
                float distFromCenter = Mathf.Abs(x - width / 2f);
                if (distFromCenter < width / 2f)
                {
                    float alpha = 1f - (distFromCenter / (width / 2f));
                    pixels[y * width + x] = new Color(1f, 1f, 1f, alpha);
                }
                else
                {
                    pixels[y * width + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0f), 100f);
        arrowLineImage.sprite = sprite;
    }
    
    void CreateArrowHeadSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                Vector2 dir = (pos - center);
                float dist = dir.magnitude;
                float angle = Vector2.SignedAngle(Vector2.up, dir);
                
                // Рисуем треугольник (наконечник стрелки)
                if (dist < size * 0.4f && angle > -60 && angle < 60)
                {
                    float alpha = 1f - (dist / (size * 0.4f));
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        arrowHeadImage.sprite = sprite;
    }
}


