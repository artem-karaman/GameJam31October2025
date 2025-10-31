using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("References")]
    public PlayerController player;
    public FishingRodController fishingRod;
    public FishPool fishPool;
    
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
        // Создаем пул рыб, если его нет
        if (fishPool == null)
        {
            GameObject poolObj = new GameObject("FishPool");
            fishPool = poolObj.AddComponent<FishPool>();
        }
        
        // Связываем компоненты
        if (fishingRod != null && player != null)
        {
            fishingRod.playerController = player;
        }
    }
    
    public void CastFishingRod()
    {
        if (fishingRod != null)
        {
            fishingRod.CastRod();
        }
    }
}

