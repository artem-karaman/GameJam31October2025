using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Управление UI игры: фон, HUD, счет
/// </summary>
public class CastleUIManager : MonoBehaviour
{
    public static CastleUIManager Instance { get; private set; }
    
    [Header("UI References")]
    public Canvas canvas;
    public Image backgroundImage;
    public Text scoreText;
    public Text monstersKilledText;
    
    [Header("Settings")]
    public Color backgroundColor = new Color(0.2f, 0.4f, 0.6f);
    
    private int score = 0;
    private int monstersKilled = 0;
    
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
    
    /// <summary>
    /// Обновляет счет убитых монстров
    /// </summary>
    public void OnMonsterKilled()
    {
        monstersKilled++;
        score += 10; // 10 очков за монстра
        
        UpdateUI();
    }
    
    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
        
        if (monstersKilledText != null)
        {
            monstersKilledText.text = $"Monsters: {monstersKilled}";
        }
    }
    
    public void ResetScore()
    {
        score = 0;
        monstersKilled = 0;
        UpdateUI();
    }
}


