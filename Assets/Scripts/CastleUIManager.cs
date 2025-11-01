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
    }
    
    
    public void ResetScore()
    {
        score = 0;
        monstersKilled = 0;
    }
}


