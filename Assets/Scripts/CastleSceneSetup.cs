using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Автоматическая настройка сцены для игры с замком и крюком
/// Добавьте этот скрипт на любой GameObject и нажмите Setup в Inspector
/// </summary>
public class CastleSceneSetup : MonoBehaviour
{
    [ContextMenu("Setup Castle Scene")]
    public void SetupScene()
    {
        Debug.Log("=== Начинаю автоматическую настройку сцены ===");
        
        // 1. Настраиваем камеру
        SetupCamera();
        
        // 2. Создаем замок (визуально)
        GameObject castle = SetupCastle();
        
        // 3. Создаем игрока на вершине замка
        CastlePlayerController player = SetupPlayer(castle);
        
        // 4. Создаем крюк
        HookController hook = SetupHook(player);
        
        // 5. Создаем индикатор стрелки
        ArrowIndicator arrow = SetupArrow();
        
        // 6. Создаем контроллер тапа
        CastleGameTouchController touchController = SetupTouchController(hook, arrow, player);
        
        // 7. Создаем спавнер монстров
        MonsterSpawner spawner = SetupMonsterSpawner(castle);
        
        // 8. Создаем GameManager
        CastleGameManager gameManager = SetupGameManager(player, hook, touchController, spawner, arrow);
        
        Debug.Log("=== Настройка сцены завершена! ===");
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
        
        // Настраиваем камеру для вертикального разрешения
        cam.orthographic = true;
        cam.orthographicSize = 10f;
        cam.backgroundColor = new Color(0.3f, 0.5f, 0.7f);
        cam.transform.position = new Vector3(0, 0, -10);
        
        Debug.Log("✓ Камера настроена");
        return cam;
    }
    
    GameObject SetupCastle()
    {
        GameObject castle = GameObject.Find("Castle");
        
        if (castle == null)
        {
            castle = new GameObject("Castle");
            
            SpriteRenderer renderer = castle.AddComponent<SpriteRenderer>();
            CreateCastleSprite(renderer);
            renderer.sortingOrder = 1;
        }
        
        // Замок стоит на полу (нижняя точка на y = -8)
        // Верх замка будет на y = 3 (где стоит игрок)
        castle.transform.position = new Vector3(0, -8f, 0); // Низ замка на полу
        
        Debug.Log("✓ Замок создан");
        return castle;
    }
    
    void CreateCastleSprite(SpriteRenderer renderer)
    {
        // Создаем высокий замок - от пола до вершины где стоит игрок
        float playerHeight = 3f; // Высота вершины замка (где стоит игрок)
        float castleBottom = -8f; // Низ экрана (пол) - позиция замка по Y
        float castleHeight = playerHeight - castleBottom; // Общая высота замка = 11 единиц
        
        int width = 200; // Ширина замка
        int height = Mathf.RoundToInt(castleHeight * 100f); // Высота в пикселях (масштаб 100 pixels per unit)
        
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        
        Color castleColor = new Color(0.5f, 0.4f, 0.3f);
        Color darkColor = new Color(0.3f, 0.2f, 0.1f);
        Color lightColor = new Color(0.6f, 0.5f, 0.4f);
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float centerX = width / 2f;
                
                // Расстояние от центра по X
                float distFromCenterX = Mathf.Abs(x - centerX);
                
                // Рисуем замок - прямоугольник с закругленными краями
                if (distFromCenterX < 80f) // Ширина замка
                {
                    // Основание замка (низ) - шире
                    if (y < height * 0.1f)
                    {
                        if (distFromCenterX < 90f)
                        {
                            pixels[y * width + x] = darkColor;
                        }
                        else
                        {
                            pixels[y * width + x] = Color.clear;
                        }
                    }
                    // Основная часть замка
                    else if (y < height * 0.95f)
                    {
                        // Рисуем кирпичи
                        bool isBrick = ((x / 10) + (y / 10)) % 2 == 0;
                        pixels[y * width + x] = isBrick ? castleColor : lightColor;
                        
                        // Края замка - темнее
                        if (distFromCenterX > 75f)
                        {
                            pixels[y * width + x] = darkColor;
                        }
                    }
                    // Верхняя часть - башня
                    else
                    {
                        // Узкая башня наверху
                        if (distFromCenterX < 40f)
                        {
                            pixels[y * width + x] = castleColor;
                        }
                        else
                        {
                            pixels[y * width + x] = Color.clear;
                        }
                    }
                }
                else
                {
                    pixels[y * width + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        // Якорь спрайта внизу по центру (pivot внизу)
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0f), 100f);
        renderer.sprite = sprite;
    }
    
    CastlePlayerController SetupPlayer(GameObject castle)
    {
        GameObject playerObj = GameObject.Find("Player");
        CastlePlayerController player;
        
        if (playerObj == null)
        {
            playerObj = new GameObject("Player");
            player = playerObj.AddComponent<CastlePlayerController>();
        }
        else
        {
            player = playerObj.GetComponent<CastlePlayerController>();
            if (player == null)
            {
                player = playerObj.AddComponent<CastlePlayerController>();
            }
        }
        
        player.castlePosition = castle.transform.position;
        // Игрок стоит на вершине замка
        float playerY = castle.transform.position.y + player.castleTopHeight;
        playerObj.transform.position = new Vector3(castle.transform.position.x, playerY, 0);
        
        Debug.Log("✓ Игрок создан");
        return player;
    }
    
    HookController SetupHook(CastlePlayerController player)
    {
        GameObject hookObj = GameObject.Find("Hook");
        HookController hook;
        
        if (hookObj == null)
        {
            hookObj = new GameObject("Hook");
            hook = hookObj.AddComponent<HookController>();
        }
        else
        {
            hook = hookObj.GetComponent<HookController>();
            if (hook == null)
            {
                hook = hookObj.AddComponent<HookController>();
            }
        }
        
        hook.playerTransform = player.transform;
        
        Debug.Log("✓ Крюк создан");
        return hook;
    }
    
    ArrowIndicator SetupArrow()
    {
        GameObject arrowObj = GameObject.Find("ArrowIndicator");
        ArrowIndicator arrow;
        
        if (arrowObj == null)
        {
            arrowObj = new GameObject("ArrowIndicator");
            arrow = arrowObj.AddComponent<ArrowIndicator>();
        }
        else
        {
            arrow = arrowObj.GetComponent<ArrowIndicator>();
            if (arrow == null)
            {
                arrow = arrowObj.AddComponent<ArrowIndicator>();
            }
        }
        
        Debug.Log("✓ Индикатор стрелки создан");
        return arrow;
    }
    
    CastleGameTouchController SetupTouchController(HookController hook, ArrowIndicator arrow, CastlePlayerController player)
    {
        GameObject touchObj = GameObject.Find("TouchController");
        CastleGameTouchController touchController;
        
        if (touchObj == null)
        {
            touchObj = new GameObject("TouchController");
            touchController = touchObj.AddComponent<CastleGameTouchController>();
        }
        else
        {
            touchController = touchObj.GetComponent<CastleGameTouchController>();
            if (touchController == null)
            {
                touchController = touchObj.AddComponent<CastleGameTouchController>();
            }
        }
        
        touchController.hookController = hook;
        touchController.arrowIndicator = arrow;
        touchController.playerTransform = player.transform;
        
        Debug.Log("✓ Контроллер тапа создан");
        return touchController;
    }
    
    MonsterSpawner SetupMonsterSpawner(GameObject castle)
    {
        GameObject spawnerObj = GameObject.Find("MonsterSpawner");
        MonsterSpawner spawner;
        
        if (spawnerObj == null)
        {
            spawnerObj = new GameObject("MonsterSpawner");
            spawner = spawnerObj.AddComponent<MonsterSpawner>();
        }
        else
        {
            spawner = spawnerObj.GetComponent<MonsterSpawner>();
            if (spawner == null)
            {
                spawner = spawnerObj.AddComponent<MonsterSpawner>();
            }
        }
        
        spawner.castleCenter = castle.transform.position;
        spawner.monsterCount = 5;
        spawner.spawnRadius = 6f;
        
        Debug.Log("✓ Спавнер монстров создан");
        return spawner;
    }
    
    CastleGameManager SetupGameManager(CastlePlayerController player, HookController hook, 
        CastleGameTouchController touchController, MonsterSpawner spawner, ArrowIndicator arrow)
    {
        GameObject gmObj = GameObject.Find("GameManager");
        CastleGameManager gm;
        
        if (gmObj == null)
        {
            gmObj = new GameObject("GameManager");
            gm = gmObj.AddComponent<CastleGameManager>();
        }
        else
        {
            gm = gmObj.GetComponent<CastleGameManager>();
            if (gm == null)
            {
                gm = gmObj.AddComponent<CastleGameManager>();
            }
        }
        
        gm.player = player;
        gm.hook = hook;
        gm.touchController = touchController;
        gm.monsterSpawner = spawner;
        gm.arrowIndicator = arrow;
        gm.castlePosition = Vector2.zero;
        
        Debug.Log("✓ GameManager создан");
        return gm;
    }
}

