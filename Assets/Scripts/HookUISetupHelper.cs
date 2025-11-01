using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Быстрая настройка префаба крюка для работы на Canvas
/// Добавьте на префаб крюка и используйте Context Menu для настройки
/// </summary>
public class HookUISetupHelper : MonoBehaviour
{
    [ContextMenu("Setup Hook Prefab")]
    public void SetupHookPrefab()
    {
        // Добавляем/находим компонент крюка
        HookUI hook = GetComponent<HookUI>();
        if (hook == null)
        {
            hook = gameObject.AddComponent<HookUI>();
        }
        
        // Настраиваем RectTransform
        RectTransform rect = GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = gameObject.AddComponent<RectTransform>();
        }
        
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(32, 32);
        
        // Настраиваем Image для крюка
        Image hookImage = GetComponent<Image>();
        if (hookImage == null)
        {
            hookImage = gameObject.AddComponent<Image>();
        }
        hook.hookImage = hookImage;
        
        // Если спрайт не назначен, скрипт создаст его автоматически
        if (hookImage.sprite == null)
        {
            Debug.Log("Спрайт крюка не назначен - будет создан автоматически при запуске");
        }
        
        // Настраиваем линию (будет создана автоматически в HookUI)
        // Можно настроить толщину линии в компоненте HookUI
        
        Debug.Log($"✓ Префаб крюка настроен: {gameObject.name}");
        Debug.Log("  Скрипт HookUI добавлен и настроен");
        Debug.Log("  Теперь можете:");
        Debug.Log("    - Назначить свой Sprite в компоненте Image");
        Debug.Log("    - Настроить толщину линии в HookUI.lineThickness");
        Debug.Log("    - Настроить скорость и параметры полета");
        
        // Удаляем helper после настройки (опционально)
        // DestroyImmediate(this);
    }
}

