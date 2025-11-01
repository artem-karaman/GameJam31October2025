using UnityEngine;

/// <summary>
/// Контроллер тапов для игры со SpriteRenderer (работа с камерой)
/// </summary>
public class CastleGameTouchController : MonoBehaviour
{
    public static CastleGameTouchController Instance { get; private set; }
    
    [Header("References")]
    public HookController hookController;
    public Transform playerTransform;
    public CastlePlayer playerController;
    
    [Header("Touch Settings")]
    [Tooltip("Разрешить новый бросок даже если крюк активен (отменит текущий)")]
    public bool allowInterruptHook = false;
    
    private Camera mainCamera;
    private bool isHoldingTouch = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeReferences();
    }
    
    /// <summary>
    /// Инициализирует все ссылки, вызывается при старте и может быть вызвана повторно
    /// </summary>
    void InitializeReferences()
    {
        // Автоматически находим камеру
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        // Автоматически находим компоненты если не назначены
        if (hookController == null)
        {
            hookController = FindObjectOfType<HookController>();
        }
        
        if (playerTransform == null || playerController == null)
        {
            CastlePlayer player = FindObjectOfType<CastlePlayer>();
            if (player != null)
            {
                playerTransform = player.transform;
                playerController = player;
            }
        }
        
        Debug.Log($"TouchController инициализирован:");
        Debug.Log($"  - camera: {mainCamera != null} {(mainCamera != null ? $"({mainCamera.name})" : "")}");
        Debug.Log($"  - hookController: {hookController != null}");
        Debug.Log($"  - playerTransform: {playerTransform != null}");
        Debug.Log($"  - playerController: {playerController != null}");
    }
    
    void Update()
    {
        HandleInput();
    }
    
    void HandleInput()
    {
        // Проверяем что все компоненты на месте
        if (hookController == null || playerTransform == null || mainCamera == null)
        {
            // Пытаемся переинициализировать если что-то пропало
            if (mainCamera == null || hookController == null || playerTransform == null)
            {
                InitializeReferences();
            }
            return;
        }
        
        // Обработка мыши
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"Mouse Down: {Input.mousePosition}");
            OnTouchStart(Input.mousePosition);
        }
        
        if (Input.GetMouseButton(0) && isHoldingTouch)
        {
            OnTouchHold(Input.mousePosition);
        }
        
        if (Input.GetMouseButtonUp(0) && isHoldingTouch)
        {
            Debug.Log($"Mouse Up: {Input.mousePosition}");
            OnTouchEnd(Input.mousePosition);
        }
        
        // Обработка тача
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                Debug.Log($"Touch Began: {touch.position}");
                OnTouchStart(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                if (isHoldingTouch)
                {
                    OnTouchHold(touch.position);
                }
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                if (isHoldingTouch)
                {
                    Debug.Log($"Touch Ended: {touch.position}");
                    OnTouchEnd(touch.position);
                }
            }
        }
    }
    
    void OnTouchStart(Vector2 screenPosition)
    {
        Debug.Log($"OnTouchStart вызван, screenPosition={screenPosition}");
        
        if (hookController == null)
        {
            Debug.LogWarning("hookController == null, пытаюсь найти...");
            hookController = FindObjectOfType<HookController>();
            if (hookController == null)
            {
                Debug.LogError("hookController не найден!");
                return;
            }
        }
        
        if (hookController.IsActive)
        {
            if (allowInterruptHook)
            {
                // Если разрешено прерывание, останавливаем текущий крюк
                Debug.Log("Прерываем текущий бросок крюка");
                // Крюк сам сбросится при новом CastHook
            }
            else
            {
                // Крюк еще в полете или возвращается - игнорируем новый тап
                // (защита от повторных тапов, чтобы не запускать несколько крюков одновременно)
                return;
            }
        }
        
        isHoldingTouch = true;
        Debug.Log($"isHoldingTouch = true, playerController={playerController != null}");
        
        // Начинаем замах (как рыбак замахивается удочкой)
        if (playerController != null)
        {
            playerController.StartWindup();
            Debug.Log("Замах игрока начат");
        }
        else
        {
            Debug.LogWarning("playerController == null!");
        }
    }
    
    void OnTouchHold(Vector2 screenPosition)
    {
        if (!isHoldingTouch) return;
        // Можно добавить визуализацию направления здесь
    }
    
    void OnTouchEnd(Vector2 screenPosition)
    {
        Debug.Log($"OnTouchEnd вызван, screenPosition={screenPosition}, isHoldingTouch={isHoldingTouch}");
        
        if (!isHoldingTouch)
        {
            Debug.LogWarning("OnTouchEnd вызван, но isHoldingTouch = false");
            return;
        }
        
        if (hookController == null)
        {
            Debug.LogError("hookController == null в OnTouchEnd!");
            isHoldingTouch = false;
            return;
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("mainCamera == null в OnTouchEnd!");
            isHoldingTouch = false;
            return;
        }
        
        isHoldingTouch = false;
        
        // Конвертируем позицию экрана в мировые координаты
        Vector3 worldPos = ScreenToWorldPosition(screenPosition);
        Debug.Log($"Конвертированная позиция: screen={screenPosition} -> world={worldPos}");
        
        // Делаем бросок (анимация игрока)
        float castPower = 1f;
        
        // Вычисляем "силу" броска на основе расстояния
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(playerTransform.position, worldPos);
            castPower = Mathf.Clamp01(distance / 5f); // Нормализуем по максимальному расстоянию
            Debug.Log($"Расстояние до цели: {distance}, сила броска: {castPower}");
        }
        
        if (playerController != null)
        {
            playerController.Cast(castPower);
            Debug.Log("Анимация броска запущена");
        }
        else
        {
            Debug.LogWarning("playerController == null, анимация броска не запущена");
        }
        
        // Небольшая задержка перед вылетом крюка для синхронизации с анимацией
        StartCoroutine(CastHookDelayed(worldPos, 0.1f));
    }
    
    /// <summary>
    /// Запускает крюк с небольшой задержкой после анимации броска
    /// </summary>
    System.Collections.IEnumerator CastHookDelayed(Vector3 targetWorldPos, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (hookController == null)
        {
            Debug.LogError("hookController == null! Не могу бросить крюк");
            yield break;
        }
        
        hookController.CastHook(targetWorldPos);
    }
    
    /// <summary>
    /// Конвертирует позицию экрана в мировые координаты
    /// </summary>
    Vector3 ScreenToWorldPosition(Vector2 screenPos)
    {
        if (mainCamera == null)
        {
            Debug.LogError("mainCamera == null в ScreenToWorldPosition!");
            return Vector3.zero;
        }
        
        // Для ортогональной камеры используем правильную дистанцию
        // Расстояние от камеры до плоскости игры (обычно Z камеры - это -10, плоскость игры на Z=0, значит дистанция = 10)
        float distanceToPlane = Mathf.Abs(mainCamera.transform.position.z);
        
        // Конвертируем экранную позицию в мировые координаты
        Vector3 screenPos3D = new Vector3(screenPos.x, screenPos.y, distanceToPlane);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos3D);
        worldPos.z = 0; // Устанавливаем Z = 0 для 2D игры
        
        Debug.Log($"ScreenToWorld: screen={screenPos}, distanceToPlane={distanceToPlane}, world={worldPos}");
        
        return worldPos;
    }
}
