using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Автоматически создает и настраивает всю сцену для игры про рыбалку
/// Добавьте этот скрипт на любой GameObject и нажмите кнопку Setup в Inspector
/// </summary>
public class SceneAutoSetup : MonoBehaviour
{
    [ContextMenu("Setup Scene")]
    public void SetupScene()
    {
        Debug.Log("=== Начинаю автоматическую настройку сцены ===");
        
        // 1. Создаем EventSystem (нужен для UI)
        SetupEventSystem();
        
        // 2. Создаем камеру и настраиваем её
        SetupCamera();
        
        // 3. Создаем GameManager
        GameManager gameManager = SetupGameManager();
        
        // 4. Создаем Player
        PlayerController player = SetupPlayer();
        
        // 5. Создаем FishingRod
        FishingRodController fishingRod = SetupFishingRod(player);
        
        // 6. Создаем Canvas и UI
        Canvas canvas = SetupCanvas();
        
        // 7. Создаем кнопку для броска удочки
        Button castButton = SetupCastButton(canvas);
        
        // 8. Создаем TouchController
        TouchController touchController = SetupTouchController(castButton, canvas);
        
        // 9. Создаем FishPool
        FishPool fishPool = SetupFishPool();
        
        // 10. Создаем воду (опционально)
        SetupWater();
        
        // 11. Связываем все компоненты
        LinkComponents(gameManager, player, fishingRod, touchController, fishPool);
        
        Debug.Log("=== Настройка сцены завершена! ===");
    }
    
    EventSystem SetupEventSystem()
    {
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystem = eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            Debug.Log("✓ EventSystem создан");
        }
        else
        {
            Debug.Log("✓ EventSystem уже существует");
        }
        return eventSystem;
    }
    
    Camera SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = FindObjectOfType<Camera>();
        }
        
        if (cam == null)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            cam = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
            cameraObj.AddComponent<AudioListener>();
        }
        
        // Настраиваем камеру для 2D
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor = new Color(0.5f, 0.7f, 0.9f);
        cam.transform.position = new Vector3(0, 0, -10);
        
        Debug.Log("✓ Камера настроена");
        return cam;
    }
    
    GameManager SetupGameManager()
    {
        GameObject gmObj = GameObject.Find("GameManager");
        GameManager gm;
        
        if (gmObj == null)
        {
            gmObj = new GameObject("GameManager");
            gm = gmObj.AddComponent<GameManager>();
        }
        else
        {
            gm = gmObj.GetComponent<GameManager>();
            if (gm == null)
            {
                gm = gmObj.AddComponent<GameManager>();
            }
        }
        
        Debug.Log("✓ GameManager создан");
        return gm;
    }
    
    PlayerController SetupPlayer()
    {
        GameObject playerObj = GameObject.Find("Player");
        PlayerController player;
        
        if (playerObj == null)
        {
            playerObj = new GameObject("Player");
            player = playerObj.AddComponent<PlayerController>();
            playerObj.transform.position = new Vector3(0, -1, 0);
        }
        else
        {
            player = playerObj.GetComponent<PlayerController>();
            if (player == null)
            {
                player = playerObj.AddComponent<PlayerController>();
            }
        }
        
        Debug.Log("✓ Player создан");
        return player;
    }
    
    FishingRodController SetupFishingRod(PlayerController player)
    {
        if (player == null) return null;
        
        Transform rodTransform = player.transform.Find("FishingRod");
        GameObject rodObj;
        FishingRodController rod;
        
        if (rodTransform == null)
        {
            rodObj = new GameObject("FishingRod");
            rodObj.transform.SetParent(player.transform);
            rodObj.transform.localPosition = new Vector3(0, 0.5f, 0);
            rod = rodObj.AddComponent<FishingRodController>();
        }
        else
        {
            rodObj = rodTransform.gameObject;
            rod = rodObj.GetComponent<FishingRodController>();
            if (rod == null)
            {
                rod = rodObj.AddComponent<FishingRodController>();
            }
        }
        
        Debug.Log("✓ FishingRod создан");
        return rod;
    }
    
    Canvas SetupCanvas()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }
        
        Debug.Log("✓ Canvas создан");
        return canvas;
    }
    
    Button SetupCastButton(Canvas canvas)
    {
        if (canvas == null) return null;
        
        Transform buttonTransform = canvas.transform.Find("CastButton");
        GameObject buttonObj;
        Button button;
        
        if (buttonTransform == null)
        {
            buttonObj = new GameObject("CastButton");
            buttonObj.transform.SetParent(canvas.transform);
            
            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(200, 200);
            rect.anchoredPosition = Vector2.zero;
            
            Image img = buttonObj.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.5f);
            
            // Создаем спрайт для кнопки (круг)
            img.sprite = CreateCircleSprite();
            
            button = buttonObj.AddComponent<Button>();
            
            // Настраиваем цвета кнопки
            var colors = button.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0.5f);
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.7f);
            colors.pressedColor = new Color(1f, 1f, 1f, 0.9f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            button.colors = colors;
        }
        else
        {
            buttonObj = buttonTransform.gameObject;
            button = buttonObj.GetComponent<Button>();
            if (button == null)
            {
                button = buttonObj.AddComponent<Button>();
            }
        }
        
        Debug.Log("✓ CastButton создан");
        return button;
    }
    
    Sprite CreateCircleSprite()
    {
        int size = 200;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance < size / 2f - 5 && distance > size / 2f - 25)
                {
                    pixels[y * size + x] = Color.white;
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    TouchController SetupTouchController(Button castButton, Canvas canvas)
    {
        if (castButton == null) return null;
        
        GameObject tcObj = GameObject.Find("TouchController");
        TouchController tc;
        
        if (tcObj == null)
        {
            tcObj = new GameObject("TouchController");
            tc = tcObj.AddComponent<TouchController>();
        }
        else
        {
            tc = tcObj.GetComponent<TouchController>();
            if (tc == null)
            {
                tc = tcObj.AddComponent<TouchController>();
            }
        }
        
        tc.castButton = castButton;
        
        Debug.Log("✓ TouchController создан");
        return tc;
    }
    
    FishPool SetupFishPool()
    {
        GameObject poolObj = GameObject.Find("FishPool");
        FishPool pool;
        
        if (poolObj == null)
        {
            poolObj = new GameObject("FishPool");
            pool = poolObj.AddComponent<FishPool>();
        }
        else
        {
            pool = poolObj.GetComponent<FishPool>();
            if (pool == null)
            {
                pool = poolObj.AddComponent<FishPool>();
            }
        }
        
        Debug.Log("✓ FishPool создан");
        return pool;
    }
    
    void SetupWater()
    {
        GameObject waterObj = GameObject.Find("Water");
        
        if (waterObj == null)
        {
            waterObj = new GameObject("Water");
            SpriteRenderer waterRenderer = waterObj.AddComponent<SpriteRenderer>();
            
            // Создаем спрайт воды
            Texture2D waterTexture = new Texture2D(1024, 256);
            Color[] pixels = new Color[1024 * 256];
            Color waterColor = new Color(0.2f, 0.4f, 0.8f, 0.7f);
            
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = waterColor;
            }
            
            waterTexture.SetPixels(pixels);
            waterTexture.Apply();
            
            Sprite waterSprite = Sprite.Create(waterTexture, new Rect(0, 0, 1024, 256), new Vector2(0.5f, 0.5f), 100);
            waterRenderer.sprite = waterSprite;
            waterRenderer.sortingOrder = -1;
            
            waterObj.transform.position = new Vector3(0, -3f, 0);
            waterObj.transform.localScale = new Vector3(10f, 1f, 1f);
            
            Debug.Log("✓ Water создана");
        }
    }
    
    void LinkComponents(GameManager gm, PlayerController player, FishingRodController rod, 
                        TouchController tc, FishPool pool)
    {
        if (gm != null)
        {
            gm.player = player;
            gm.fishingRod = rod;
            gm.fishPool = pool;
        }
        
        if (rod != null && player != null)
        {
            rod.playerController = player;
        }
        
        Debug.Log("✓ Все компоненты связаны");
    }
}

