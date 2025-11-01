using UnityEngine;

/// <summary>
/// Главный менеджер игры с замком
/// </summary>
public class CastleGameManager : MonoBehaviour
{
    public static CastleGameManager Instance { get; private set; }
    
    [Header("References")]
    public CastlePlayerController player;
    public HookController hook;
    public CastleGameTouchController touchController;
    public MonsterSpawner monsterSpawner;
    public ArrowIndicator arrowIndicator;
    
    [Header("Castle Settings")]
    public Vector2 castlePosition = Vector2.zero;
    
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
        SetupGame();
    }
    
    void Update()
    {
        // Для тестирования - можно менять анимации монстров во время игры
        if (Input.GetKeyDown(KeyCode.C) && monsterSpawner != null)
        {
            monsterSpawner.CycleMonsterAnimations();
        }
    }
    
    void SetupGame()
    {
        // Настраиваем камеру для вертикального разрешения
        SetupCamera();
        
        // Связываем компоненты
        LinkComponents();
    }
    
    void SetupCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            mainCam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();
        }
        
        mainCam.orthographic = true;
        mainCam.orthographicSize = 10f; // Для вертикального разрешения
        mainCam.backgroundColor = new Color(0.3f, 0.5f, 0.7f);
        mainCam.transform.position = new Vector3(castlePosition.x, castlePosition.y, -10);
    }
    
    void LinkComponents()
    {
        // Связываем крюк с игроком
        if (hook != null && player != null)
        {
            hook.playerTransform = player.transform;
        }
        
        // Связываем контроллер тапа
        if (touchController != null)
        {
            touchController.hookController = hook;
            touchController.playerTransform = player != null ? player.transform : null;
            touchController.arrowIndicator = arrowIndicator;
        }
        
        // Настраиваем спавнер монстров
        if (monsterSpawner != null)
        {
            monsterSpawner.castleCenter = castlePosition;
        }
    }
}

