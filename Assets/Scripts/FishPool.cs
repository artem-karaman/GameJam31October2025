using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FishData
{
    public string name;
    public Color color;
    public float rarity = 1f;
}

public class FishPool : MonoBehaviour
{
    public static FishPool Instance { get; private set; }
    
    [Header("Fish Settings")]
    public List<FishData> fishTypes = new List<FishData>();
    
    [Header("Pool Settings")]
    public int poolSize = 20;
    
    private Queue<GameObject> availableFish = new Queue<GameObject>();
    private List<GameObject> allFish = new List<GameObject>();
    
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
        InitializePool();
        
        if (fishTypes.Count == 0)
        {
            CreateDefaultFishTypes();
        }
    }
    
    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject fish = CreateFish();
            fish.SetActive(false);
            availableFish.Enqueue(fish);
            allFish.Add(fish);
        }
    }
    
    GameObject CreateFish()
    {
        GameObject fish = new GameObject("Fish");
        SpriteRenderer renderer = fish.AddComponent<SpriteRenderer>();
        
        Texture2D texture = new Texture2D(48, 48);
        Color[] pixels = new Color[48 * 48];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 48, 48), new Vector2(0.5f, 0.5f), 48);
        renderer.sprite = sprite;
        
        fish.transform.SetParent(transform);
        return fish;
    }
    
    void CreateDefaultFishTypes()
    {
        fishTypes.Add(new FishData { name = "Обычная рыба", color = new Color(0.5f, 0.5f, 0.8f), rarity = 5f });
        fishTypes.Add(new FishData { name = "Золотая рыба", color = new Color(1f, 0.8f, 0.2f), rarity = 2f });
        fishTypes.Add(new FishData { name = "Красная рыба", color = new Color(0.9f, 0.3f, 0.3f), rarity = 3f });
        fishTypes.Add(new FishData { name = "Редкая рыба", color = new Color(0.8f, 0.2f, 0.9f), rarity = 1f });
    }
    
    public GameObject GetRandomFish()
    {
        // Выбираем случайную рыбу на основе весов
        float totalWeight = 0f;
        foreach (var fish in fishTypes)
        {
            totalWeight += fish.rarity;
        }
        
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        FishData selectedFishData = fishTypes[0];
        foreach (var fish in fishTypes)
        {
            currentWeight += fish.rarity;
            if (randomValue <= currentWeight)
            {
                selectedFishData = fish;
                break;
            }
        }
        
        // Получаем рыбу из пула
        GameObject fishObj;
        if (availableFish.Count > 0)
        {
            fishObj = availableFish.Dequeue();
        }
        else
        {
            fishObj = CreateFish();
            allFish.Add(fishObj);
        }
        
        // Настраиваем рыбу
        fishObj.SetActive(true);
        SpriteRenderer renderer = fishObj.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = selectedFishData.color;
        }
        
        return fishObj;
    }
    
    public void ReturnFish(GameObject fish)
    {
        if (fish != null && allFish.Contains(fish))
        {
            fish.SetActive(false);
            availableFish.Enqueue(fish);
        }
    }
}

