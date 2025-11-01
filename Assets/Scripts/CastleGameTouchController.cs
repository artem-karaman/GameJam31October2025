using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Контроллер тапов для игры с замком и крюком
/// </summary>
public class CastleGameTouchController : MonoBehaviour
{
    public static CastleGameTouchController Instance { get; private set; }
    
    [Header("References")]
    public HookController hookController;
    public ArrowIndicator arrowIndicator;
    public Transform playerTransform;
    
    private bool isHoldingTouch = false;
    private Vector2 touchStartPosition;
    private Vector2 currentTouchPosition;
    
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
    
    void Update()
    {
        HandleInput();
    }
    
    void HandleInput()
    {
        // Обработка мыши (для редактора)
        if (Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                OnTouchStart(Input.mousePosition);
            }
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
                if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
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
        // Не запускаем новый бросок, если крюк уже летит
        if (hookController != null && hookController.IsActive)
        {
            return;
        }
        
        isHoldingTouch = true;
        touchStartPosition = screenPosition;
        currentTouchPosition = screenPosition;
        
        // Показываем стрелку
        if (arrowIndicator != null && playerTransform != null)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPosition);
            arrowIndicator.Show(playerTransform.position, worldPos);
        }
    }
    
    void OnTouchHold(Vector2 screenPosition)
    {
        if (!isHoldingTouch) return;
        
        currentTouchPosition = screenPosition;
        
        // Обновляем стрелку
        if (arrowIndicator != null && playerTransform != null)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPosition);
            arrowIndicator.Show(playerTransform.position, worldPos);
        }
    }
    
    void OnTouchEnd(Vector2 screenPosition)
    {
        if (!isHoldingTouch) return;
        
        isHoldingTouch = false;
        
        // Скрываем стрелку
        if (arrowIndicator != null)
        {
            arrowIndicator.Hide();
        }
        
        // Бросаем крюк
        if (hookController != null && playerTransform != null)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPosition);
            hookController.CastHook(worldPos);
        }
    }
}

