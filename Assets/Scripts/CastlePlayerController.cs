using UnityEngine;

/// <summary>
/// Контроллер игрока на вершине замка
/// </summary>
public class CastlePlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float castleTopHeight = 3f;
    public Vector2 castlePosition = Vector2.zero;
    
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        SetupPlayer();
    }
    
    void SetupPlayer()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Устанавливаем позицию на вершине замка
        transform.position = castlePosition + Vector2.up * castleTopHeight;
        
        // Создаем простой спрайт игрока
        if (spriteRenderer.sprite == null)
        {
            CreatePlayerSprite();
        }
        
        spriteRenderer.sortingOrder = 5;
    }
    
    void CreatePlayerSprite()
    {
        Texture2D texture = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        
        Color playerColor = new Color(0.2f, 0.6f, 0.9f);
        
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float distFromCenter = Vector2.Distance(new Vector2(x, y), new Vector2(32, 32));
                
                if (distFromCenter < 25f)
                {
                    pixels[y * 64 + x] = playerColor;
                }
                else if (distFromCenter < 28f)
                {
                    pixels[y * 64 + x] = Color.black;
                }
                else
                {
                    pixels[y * 64 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = Color.white;
    }
}

