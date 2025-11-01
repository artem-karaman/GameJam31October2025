using UnityEngine;

/// <summary>
/// Принудительно устанавливает вертикальную ориентацию для WebGL
/// Добавьте этот скрипт на любой GameObject в сцене
/// </summary>
public class WebGLPortraitEnforcer : MonoBehaviour
{
    void Start()
    {
        // Принудительно устанавливаем вертикальную ориентацию
        Screen.orientation = ScreenOrientation.Portrait;
        
        // Запрещаем автоматический поворот экрана
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = false;
        
        Debug.Log("Вертикальная ориентация установлена для WebGL");
    }
    
    void Update()
    {
        // Периодически проверяем и исправляем ориентацию (на случай если браузер попытается повернуть)
        if (Screen.orientation != ScreenOrientation.Portrait)
        {
            Screen.orientation = ScreenOrientation.Portrait;
        }
    }
}

