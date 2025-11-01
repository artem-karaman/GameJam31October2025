using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Контроллер тапов для игры с UI элементами на Canvas
/// </summary>
public class CastleGameTouchControllerUI : MonoBehaviour
{
    public static CastleGameTouchControllerUI Instance { get; private set; }
    
    [Header("References")]
    public HookUI hookController;
    public ArrowIndicatorUI arrowIndicatorUI;
    public RectTransform playerRectTransform;
    public Canvas canvas;
    public CastlePlayerUI playerController;
    
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
        // Автоматически находим Canvas если не назначен
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindObjectOfType<Canvas>();
            }
        }
        
        // Автоматически находим компоненты если не назначены
        if (hookController == null)
        {
            hookController = FindObjectOfType<HookUI>();
        }
        
        if (playerRectTransform == null)
        {
            CastlePlayerUI player = FindObjectOfType<CastlePlayerUI>();
            if (player != null)
            {
                playerRectTransform = player.GetComponent<RectTransform>();
            }
        }
        
        if (playerController == null)
        {
            playerController = FindObjectOfType<CastlePlayerUI>();
        }
        
        if (arrowIndicatorUI == null)
        {
            arrowIndicatorUI = FindObjectOfType<ArrowIndicatorUI>();
        }
        
        Debug.Log($"TouchController инициализирован:");
        Debug.Log($"  - canvas: {canvas != null}");
        Debug.Log($"  - hookController: {hookController != null}");
        Debug.Log($"  - playerRectTransform: {playerRectTransform != null}");
        Debug.Log($"  - playerController: {playerController != null}");
        Debug.Log($"  - arrowIndicatorUI: {arrowIndicatorUI != null}");
    }
    
    void Update()
    {
        HandleInput();
    }
    
    void HandleInput()
    {
        // Обработка мыши
        if (Input.GetMouseButtonDown(0))
        {
            // Проверяем, не кликнули ли по UI тексту (HUD элементам)
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem != null)
            {
                // Игнорируем клики только по конкретным UI элементам (кнопки, интерактивные)
                // Но разрешаем клики по фону и игровым объектам
                GameObject selected = eventSystem.currentSelectedGameObject;
                if (selected != null && (selected.CompareTag("Button") || selected.GetComponent<Button>() != null))
                {
                    return; // Игнорируем клики по кнопкам
                }
            }
            
            OnTouchStart(Input.mousePosition);
        }
        
        if (Input.GetMouseButton(0) && isHoldingTouch)
        {
            OnTouchHold(Input.mousePosition);
        }
        
        if (Input.GetMouseButtonUp(0) && isHoldingTouch)
        {
            OnTouchEnd(Input.mousePosition);
        }
        
        // Обработка тача
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                // Для тача проверяем только интерактивные элементы
                EventSystem eventSystem = EventSystem.current;
                if (eventSystem != null)
                {
                    PointerEventData pointerData = new PointerEventData(eventSystem);
                    pointerData.position = touch.position;
                    
                    var results = new System.Collections.Generic.List<RaycastResult>();
                    eventSystem.RaycastAll(pointerData, results);
                    
                    // Пропускаем клик только если попали в Button или другой интерактивный элемент
                    bool hasInteractiveUI = false;
                    foreach (var result in results)
                    {
                        if (result.gameObject.GetComponent<Button>() != null || 
                            result.gameObject.GetComponent<InputField>() != null ||
                            result.gameObject.CompareTag("Button"))
                        {
                            hasInteractiveUI = true;
                            break;
                        }
                    }
                    
                    if (!hasInteractiveUI)
                    {
                        OnTouchStart(touch.position);
                    }
                }
                else
                {
                    OnTouchStart(touch.position);
                }
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
                    OnTouchEnd(touch.position);
                }
            }
        }
    }
    
    void OnTouchStart(Vector2 screenPosition)
    {
        Debug.Log($"OnTouchStart: screenPosition={screenPosition}, hookController.IsActive={hookController?.IsActive ?? false}");
        
        if (hookController != null && hookController.IsActive)
        {
            Debug.Log("Крюк уже активен, игнорируем новый тап");
            return;
        }
        
        isHoldingTouch = true;
        Debug.Log("isHoldingTouch = true");
        
        // Начинаем замах (как рыбак замахивается удочкой)
        if (playerController != null)
        {
            playerController.StartWindup();
            Debug.Log("Замах игрока начат");
        }
        else
        {
            Debug.LogWarning("playerController == null! Замах не начат");
        }
        
        if (arrowIndicatorUI != null && playerRectTransform != null && canvas != null)
        {
            Vector2 canvasTargetPos = ScreenToCanvasPosition(screenPosition);
            arrowIndicatorUI.Show(playerRectTransform.anchoredPosition, canvasTargetPos);
        }
    }
    
    void OnTouchHold(Vector2 screenPosition)
    {
        if (!isHoldingTouch) return;
        
        if (arrowIndicatorUI != null && playerRectTransform != null && canvas != null)
        {
            Vector2 canvasTargetPos = ScreenToCanvasPosition(screenPosition);
            arrowIndicatorUI.Show(playerRectTransform.anchoredPosition, canvasTargetPos);
        }
    }
    
    void OnTouchEnd(Vector2 screenPosition)
    {
        if (!isHoldingTouch)
        {
            Debug.Log("OnTouchEnd: isHoldingTouch = false, выходим");
            return;
        }
        
        Debug.Log($"OnTouchEnd: screenPosition={screenPosition}, hookController={hookController != null}, canvas={canvas != null}");
        
        isHoldingTouch = false;
        
        if (arrowIndicatorUI != null)
        {
            arrowIndicatorUI.Hide();
        }
        
        // Делаем бросок (анимация игрока)
        Vector2 canvasPos = ScreenToCanvasPosition(screenPosition);
        Debug.Log($"Конвертированная позиция в Canvas: {canvasPos}");
        
        float castPower = 1f;
        
        // Вычисляем "силу" броска на основе времени удержания или расстояния
        if (playerRectTransform != null)
        {
            float distance = Vector2.Distance(playerRectTransform.anchoredPosition, canvasPos);
            castPower = Mathf.Clamp01(distance / 500f); // Нормализуем по максимальному расстоянию
            Debug.Log($"Расстояние до цели: {distance}, сила броска: {castPower}");
        }
        
        if (playerController != null)
        {
            playerController.Cast(castPower);
        }
        else
        {
            Debug.LogWarning("playerController == null! Анимация броска не будет выполнена");
        }
        
        // Небольшая задержка перед вылетом крюка для синхронизации с анимацией
        StartCoroutine(CastHookDelayed(canvasPos, 0.1f));
    }
    
    /// <summary>
    /// Запускает крюк с небольшой задержкой после анимации броска
    /// </summary>
    System.Collections.IEnumerator CastHookDelayed(Vector2 targetPos, float delay)
    {
        Debug.Log($"CastHookDelayed: ждем {delay} секунд, targetPos={targetPos}");
        yield return new WaitForSeconds(delay);
        
        Debug.Log($"CastHookDelayed: задержка прошла, hookController={hookController != null}, canvas={canvas != null}");
        
        if (hookController == null)
        {
            Debug.LogError("hookController == null! Не могу бросить крюк");
            yield break;
        }
        
        if (canvas == null)
        {
            Debug.LogWarning("canvas == null, но продолжаю...");
        }
        
        Debug.Log($"Вызываю hookController.CastHook({targetPos})");
        hookController.CastHook(targetPos);
        Debug.Log($"CastHook вызван, isActive={hookController.IsActive}");
    }
    
    Vector2 ScreenToCanvasPosition(Vector2 screenPos)
    {
        if (canvas == null) return screenPos;
        
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect == null) return screenPos;
        
        // Для ScreenSpaceOverlay камера не нужна
        Camera cam = canvas.worldCamera;
        if (cam == null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            cam = null; // Для overlay камера не используется
        }
        
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            cam,
            out Vector2 localPoint))
        {
            return localPoint;
        }
        
        // Fallback: простая конвертация для ScreenSpaceOverlay
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Vector2 canvasSize = canvasRect.sizeDelta;
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
            
            float x = (screenPos.x / screenSize.x - 0.5f) * canvasSize.x;
            float y = (screenPos.y / screenSize.y - 0.5f) * canvasSize.y;
            
            return new Vector2(x, y);
        }
        
        return screenPos;
    }
    
    Vector2 CanvasToScreenPosition(Vector2 canvasPos)
    {
        if (canvas == null) return canvasPos;
        
        return RectTransformUtility.WorldToScreenPoint(
            canvas.worldCamera ?? Camera.main,
            canvas.GetComponent<RectTransform>().TransformPoint(canvasPos));
    }
}

