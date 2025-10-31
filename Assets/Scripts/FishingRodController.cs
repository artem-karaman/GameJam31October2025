using UnityEngine;

public class FishingRodController : MonoBehaviour
{
    [Header("References")]
    public LineRenderer fishingLine;
    public Transform hookTransform;
    public PlayerController playerController;
    
    [Header("Settings")]
    public float rodLength = 3f;
    public float castSpeed = 10f;
    public float retractSpeed = 8f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;
    
    private bool isCasting = false;
    private bool isWaiting = false;
    private bool isRodShaking = false;
    private bool canPullOut = false;
    
    private Vector2 castTarget;
    private Vector2 castStartPos;
    private float castProgress = 0f;
    private Vector3 hookStartPos;
    
    private float waitTimer = 0f;
    private float waitDuration = 0f;
    
    private GameObject caughtFish = null;
    
    void Start()
    {
        SetupFishingRod();
    }
    
    void SetupFishingRod()
    {
        // Создаем LineRenderer
        if (fishingLine == null)
        {
            fishingLine = gameObject.AddComponent<LineRenderer>();
            fishingLine.material = new Material(Shader.Find("Sprites/Default"));
            fishingLine.startColor = Color.blue;
            fishingLine.endColor = Color.blue;
            fishingLine.startWidth = 0.1f;
            fishingLine.endWidth = 0.05f;
            fishingLine.positionCount = 2;
        }
        
        // Создаем крючок
        if (hookTransform == null)
        {
            GameObject hook = new GameObject("Hook");
            hook.transform.SetParent(transform);
            hookTransform = hook.transform;
            
            SpriteRenderer hookSprite = hook.AddComponent<SpriteRenderer>();
            CreateHookSprite(hookSprite);
        }
        
        hookStartPos = hookTransform.localPosition;
        fishingLine.enabled = false;
        hookTransform.gameObject.SetActive(false);
    }
    
    public void CastRod()
    {
        if (isCasting || isWaiting) return;
        
        isCasting = true;
        isWaiting = false;
        isRodShaking = false;
        canPullOut = false;
        
        castStartPos = transform.position;
        castTarget = castStartPos + Vector2.down * rodLength;
        castProgress = 0f;
        
        fishingLine.enabled = true;
        hookTransform.gameObject.SetActive(true);
    }
    
    void UpdateCast()
    {
        castProgress += Time.deltaTime * castSpeed;
        
        if (castProgress >= 1f)
        {
            hookTransform.position = castTarget;
            StartWaiting();
        }
        else
        {
            Vector2 currentPos = Vector2.Lerp(castStartPos, castTarget, castProgress);
            float arcHeight = Mathf.Sin(castProgress * Mathf.PI) * 0.5f;
            currentPos.y += arcHeight;
            hookTransform.position = currentPos;
        }
    }
    
    void StartWaiting()
    {
        isCasting = false;
        isWaiting = true;
        canPullOut = false;
        
        waitDuration = Random.Range(minWaitTime, maxWaitTime);
        waitTimer = 0f;
    }
    
    void UpdateWaiting()
    {
        waitTimer += Time.deltaTime;
        
        if (waitTimer >= waitDuration)
        {
            StartRodShaking();
        }
    }
    
    void StartRodShaking()
    {
        isWaiting = false;
        isRodShaking = true;
        canPullOut = true;
        
        // Получаем рыбу
        if (FishPool.Instance != null)
        {
            caughtFish = FishPool.Instance.GetRandomFish();
            if (caughtFish != null)
            {
                caughtFish.transform.position = hookTransform.position;
                caughtFish.transform.SetParent(hookTransform);
                
                SpriteRenderer fishRenderer = caughtFish.GetComponent<SpriteRenderer>();
                if (fishRenderer != null)
                {
                    fishRenderer.sortingOrder = 1;
                }
            }
        }
    }
    
    void UpdateRodShaking()
    {
        float shakeAmount = 0.15f;
        float shakeSpeed = 15f;
        Vector2 shakeOffset = new Vector2(
            Mathf.Sin(Time.time * shakeSpeed) * shakeAmount,
            Mathf.Cos(Time.time * shakeSpeed * 0.7f) * shakeAmount * 0.5f
        );
        
        hookTransform.position = castTarget + shakeOffset;
        
        // Проверяем клики для вытаскивания удочки
        CheckForPullOut();
    }
    
    void CheckForPullOut()
    {
        if (!canPullOut || !isRodShaking) return;
        
        // Проверяем клик мыши или тач
        if (Input.GetMouseButtonDown(0))
        {
            // Игнорируем клики по UI
            if (UnityEngine.EventSystems.EventSystem.current != null && 
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            
            PullOutRod();
        }
        
        // Проверяем тач
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (UnityEngine.EventSystems.EventSystem.current != null && 
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    return;
                }
                
                PullOutRod();
            }
        }
    }
    
    void PullOutRod()
    {
        if (!canPullOut) return;
        
        isRodShaking = false;
        canPullOut = false;
        
        StartCoroutine(RetractRod());
    }
    
    System.Collections.IEnumerator RetractRod()
    {
        // Возвращаем крючок к игроку
        while (Vector2.Distance(hookTransform.position, transform.position) > 0.1f)
        {
            hookTransform.position = Vector2.MoveTowards(
                hookTransform.position, 
                transform.position, 
                retractSpeed * Time.deltaTime
            );
            yield return null;
        }
        
        // Анимация рыбы
        if (caughtFish != null)
        {
            Vector2 fishStartPos = caughtFish.transform.position;
            Vector2 fishEndPos = transform.position + Vector3.up * 2f;
            float fishMoveProgress = 0f;
            
            while (fishMoveProgress < 1f)
            {
                fishMoveProgress += Time.deltaTime * 3f;
                caughtFish.transform.position = Vector2.Lerp(fishStartPos, fishEndPos, fishMoveProgress);
                yield return null;
            }
            
            yield return new WaitForSeconds(1f);
            
            if (FishPool.Instance != null && caughtFish != null)
            {
                FishPool.Instance.ReturnFish(caughtFish);
            }
            
            caughtFish = null;
        }
        
        // Завершаем
        isCasting = false;
        fishingLine.enabled = false;
        hookTransform.gameObject.SetActive(false);
        hookTransform.localPosition = hookStartPos;
        
        // Разрешаем новый бросок
        if (TouchController.Instance != null)
        {
            TouchController.Instance.SetCanCast(true);
        }
    }
    
    void UpdateLineRenderer()
    {
        if (fishingLine == null || hookTransform == null) return;
        
        fishingLine.SetPosition(0, transform.position);
        fishingLine.SetPosition(1, hookTransform.position);
    }
    
    void CreateHookSprite(SpriteRenderer spriteRenderer)
    {
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                if ((x < 4 || x > 28) || (y < 4 || y > 28))
                {
                    pixels[y * 32 + x] = Color.gray;
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
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = Color.gray;
    }
    
    void Update()
    {
        if (isCasting)
        {
            UpdateCast();
        }
        
        if (isWaiting)
        {
            UpdateWaiting();
        }
        
        if (isRodShaking)
        {
            UpdateRodShaking();
        }
        
        if (fishingLine.enabled)
        {
            UpdateLineRenderer();
        }
    }
}

