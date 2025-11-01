using UnityEngine;

/// <summary>
/// Данные о типе монстра для пула
/// </summary>
[System.Serializable]
public class MonsterPoolData
{
    [Tooltip("Префаб монстра этого типа")]
    public GameObject monsterPrefab;
    
    [Tooltip("Имя типа монстра (для отладки)")]
    public string monsterTypeName = "Monster";
    
    [Tooltip("Вес спавна (чем больше, тем чаще появляется)")]
    [Range(1, 10)]
    public int spawnWeight = 1;
    
    [Tooltip("Минимальное количество в пуле (создается заранее)")]
    [Range(0, 20)]
    public int poolMinSize = 2;
    
    /// <summary>
    /// Текущее количество активных монстров этого типа
    /// </summary>
    [HideInInspector]
    public int activeCount = 0;
}

