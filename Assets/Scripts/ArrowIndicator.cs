using UnityEngine;

/// <summary>
/// Визуализация стрелки от крюка до точки касания при удержании тапа
/// </summary>
public class ArrowIndicator : MonoBehaviour
{
    [Header("References")]
    public LineRenderer arrowLine;
    public SpriteRenderer arrowHead;
    
    [Header("Settings")]
    public float arrowWidth = 0.1f;
    public float headSize = 0.5f;
    public Color arrowColor = Color.red;
    
    private bool isVisible = false;
    private Vector2 hookPosition;
    private Vector2 targetPosition;
    
    void Start()
    {
        SetupArrow();
    }
    
    void SetupArrow()
    {
        // Создаем линию стрелки
        if (arrowLine == null)
        {
            arrowLine = gameObject.AddComponent<LineRenderer>();
            arrowLine.material = new Material(Shader.Find("Sprites/Default"));
            arrowLine.startColor = arrowColor;
            arrowLine.endColor = arrowColor;
            arrowLine.startWidth = arrowWidth;
            arrowLine.endWidth = arrowWidth;
            arrowLine.positionCount = 2;
            arrowLine.sortingOrder = 10;
        }
        
        // Создаем наконечник стрелки
        if (arrowHead == null)
        {
            GameObject headObj = new GameObject("ArrowHead");
            headObj.transform.SetParent(transform);
            arrowHead = headObj.AddComponent<SpriteRenderer>();
            CreateArrowHeadSprite();
            arrowHead.sortingOrder = 11;
            arrowHead.color = arrowColor;
        }
        
        Hide();
    }
    
    /// <summary>
    /// Показывает стрелку от крюка до цели
    /// </summary>
    public void Show(Vector2 hookPos, Vector2 targetPos)
    {
        isVisible = true;
        hookPosition = hookPos;
        targetPosition = targetPos;
        
        arrowLine.enabled = true;
        arrowHead.gameObject.SetActive(true);
        
        UpdateArrow();
    }
    
    /// <summary>
    /// Скрывает стрелку
    /// </summary>
    public void Hide()
    {
        isVisible = false;
        arrowLine.enabled = false;
        if (arrowHead != null)
        {
            arrowHead.gameObject.SetActive(false);
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
        // Обновляем линию
        arrowLine.SetPosition(0, hookPosition);
        arrowLine.SetPosition(1, targetPosition);
        
        // Обновляем позицию и поворот наконечника
        if (arrowHead != null)
        {
            Vector2 direction = (targetPosition - hookPosition).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            arrowHead.transform.position = targetPosition;
            arrowHead.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
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
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        arrowHead.sprite = sprite;
    }
}

