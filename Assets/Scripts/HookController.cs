using UnityEngine;
using System.Collections;

/// <summary>
/// Контроллер крюка, который летит по дуге к месту тапа
/// </summary>
public class HookController : MonoBehaviour
{
    [Header("References")]
    public LineRenderer hookLine;
    public SpriteRenderer hookSprite;
    public Transform playerTransform;
    
    [Header("Settings")]
    public float hookSpeed = 10f;
    public float arcHeight = 2f;
    public float retractSpeed = 15f;
    public float hookRadius = 0.3f; // Радиус для попадания в монстров
    
    private bool isFlying = false;
    private bool isRetracting = false;
    private Vector2 startPosition;
    private Vector2 targetPosition;
    private float flightProgress = 0f;
    private Vector2 currentPosition;
    
    void Start()
    {
        SetupHook();
    }
    
    void SetupHook()
    {
        // Создаем LineRenderer для веревки
        if (hookLine == null)
        {
            hookLine = gameObject.AddComponent<LineRenderer>();
            hookLine.material = new Material(Shader.Find("Sprites/Default"));
            hookLine.startColor = Color.gray;
            hookLine.endColor = Color.gray;
            hookLine.startWidth = 0.05f;
            hookLine.endWidth = 0.05f;
            hookLine.positionCount = 2;
            hookLine.sortingOrder = 1;
        }
        
        // Создаем спрайт крюка
        if (hookSprite == null)
        {
            hookSprite = gameObject.AddComponent<SpriteRenderer>();
            CreateHookSprite();
            hookSprite.sortingOrder = 2;
        }
        
        // Добавляем коллайдер для крюка (необязательно, но может помочь в отладке)
        CircleCollider2D hookCollider = GetComponent<CircleCollider2D>();
        if (hookCollider == null)
        {
            hookCollider = gameObject.AddComponent<CircleCollider2D>();
            hookCollider.radius = hookRadius;
            hookCollider.isTrigger = true; // Делаем триггер, чтобы не мешать движению
        }
        
        hookLine.enabled = false;
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Запускает полет крюка к указанной позиции
    /// </summary>
    public void CastHook(Vector2 targetPos)
    {
        if (isFlying || isRetracting) return;
        
        if (playerTransform == null)
        {
            Debug.LogError("PlayerTransform не назначен!");
            return;
        }
        
        startPosition = playerTransform.position;
        targetPosition = targetPos;
        flightProgress = 0f;
        isFlying = true;
        isRetracting = false;
        
        gameObject.SetActive(true);
        transform.position = startPosition;
        hookLine.enabled = true;
    }
    
    void Update()
    {
        if (isFlying)
        {
            UpdateFlight();
        }
        else if (isRetracting)
        {
            UpdateRetract();
        }
        
        if (hookLine.enabled && playerTransform != null)
        {
            UpdateLineRenderer();
        }
    }
    
    void UpdateFlight()
    {
        flightProgress += Time.deltaTime * hookSpeed / Vector2.Distance(startPosition, targetPosition);
        
        if (flightProgress >= 1f)
        {
            // Крюк достиг цели
            transform.position = targetPosition;
            
            // Проверяем попадание в монстров в финальной точке
            CheckMonsterHit();
            
            // Если не попали ни в кого, завершаем полет и возвращаем
            if (isFlying) // Если полет еще продолжается (не прервался от попадания)
            {
                isFlying = false;
                StartRetract();
            }
        }
        else
        {
            // Вычисляем позицию по дуге
            Vector2 currentPos = Vector2.Lerp(startPosition, targetPosition, flightProgress);
            float arcProgress = Mathf.Sin(flightProgress * Mathf.PI);
            currentPos.y += arcHeight * arcProgress;
            
            transform.position = currentPos;
            currentPosition = currentPos;
            
            // Проверяем попадание в монстров во время полета
            // Монстр удалится только если крюк действительно попал
            CheckMonsterHit();
        }
    }
    
    void StartRetract()
    {
        isRetracting = true;
        currentPosition = transform.position;
    }
    
    void UpdateRetract()
    {
        Vector2 targetPos = playerTransform.position;
        transform.position = Vector2.MoveTowards(transform.position, targetPos, retractSpeed * Time.deltaTime);
        currentPosition = transform.position;
        
        if (Vector2.Distance(transform.position, targetPos) < 0.1f)
        {
            // Крюк вернулся
            isRetracting = false;
            hookLine.enabled = false;
            gameObject.SetActive(false);
        }
    }
    
    void UpdateLineRenderer()
    {
        hookLine.SetPosition(0, playerTransform.position);
        hookLine.SetPosition(1, transform.position);
    }
    
    void CheckMonsterHit()
    {
        // Проверяем столкновение с монстрами через OverlapCircle
        // Используем только во время полета крюка
        if (!isFlying) return;
        
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, hookRadius);
        
        foreach (var hit in hits)
        {
            // Пропускаем коллайдер самого крюка, если он есть
            if (hit.gameObject == gameObject) continue;
            
            // Ищем компонент MonsterController
            MonsterController monster = hit.GetComponent<MonsterController>();
            if (monster != null && !monster.IsDead)
            {
                // Проверяем, что монстр действительно находится в радиусе попадания
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                if (distance <= hookRadius)
                {
                    Debug.Log($"Крюк попал в монстра: {hit.gameObject.name}");
                    monster.Die();
                    
                    // Прерываем полет и сразу возвращаемся
                    isFlying = false;
                    StartRetract();
                    break; // Убиваем только одного монстра за раз
                }
            }
        }
    }
    
    void CreateHookSprite()
    {
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        
        // Рисуем простой крюк
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float distFromCenter = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                
                if (distFromCenter < 3f)
                {
                    pixels[y * 32 + x] = Color.gray;
                }
                else if (distFromCenter < 4f)
                {
                    pixels[y * 32 + x] = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }
                else
                {
                    pixels[y * 32 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        hookSprite.sprite = sprite;
        hookSprite.color = Color.white;
    }
    
    public bool IsActive => isFlying || isRetracting;
    
    void OnDrawGizmos()
    {
        // Рисуем радиус попадания в редакторе
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hookRadius);
    }
}

