using UnityEngine;
using UnityEngine.UI;

public class TouchController : MonoBehaviour
{
    public static TouchController Instance { get; private set; }
    
    [Header("References")]
    public Button castButton;
    
    [Header("Settings")]
    public Color normalColor = new Color(1f, 1f, 1f, 0.5f);
    public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
    
    private bool canCast = true;
    
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
        if (castButton != null)
        {
            castButton.onClick.AddListener(OnCastButtonClicked);
            UpdateButtonColor();
        }
    }
    
    public void OnCastButtonClicked()
    {
        if (!canCast) return;
        
        SetCanCast(false);
        GameManager.Instance?.CastFishingRod();
    }
    
    public void SetCanCast(bool value)
    {
        canCast = value;
        UpdateButtonColor();
    }
    
    void UpdateButtonColor()
    {
        if (castButton != null)
        {
            var colors = castButton.colors;
            colors.normalColor = canCast ? normalColor : disabledColor;
            castButton.colors = colors;
        }
    }
}

