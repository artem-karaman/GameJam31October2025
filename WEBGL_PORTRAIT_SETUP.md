# Настройка вертикальной ориентации для WebGL

## Настройки в Unity Player Settings

Настройки уже применены в `ProjectSettings/ProjectSettings.asset`:

### Основные настройки:
- ✅ **defaultScreenOrientation**: `4` (Portrait)
- ✅ **defaultScreenWidthWeb**: `1080`
- ✅ **defaultScreenHeightWeb**: `1920`
- ✅ **allowedAutorotateToPortrait**: `true` (только портретная)
- ✅ **allowedAutorotateToLandscapeRight/Left**: `false` (запрещено)
- ✅ **allowedAutorotateToPortraitUpsideDown**: `false` (запрещено)
- ✅ **useOSAutorotation**: `false` (отключено)

## Дополнительные шаги

### 1. Добавить скрипт WebGLPortraitEnforcer
Скрипт `WebGLPortraitEnforcer.cs` уже создан. Добавьте его на любой GameObject в сцене (например, на GameManager или создайте отдельный объект).

### 2. Настройка через Unity Editor
В Unity Editor:
1. Откройте **Edit → Project Settings → Player**
2. В разделе **Resolution and Presentation**:
   - **Default Orientation**: выберите **Portrait**
   - **Allowed Orientations for Auto Rotation**: снимите все галочки кроме **Portrait**
3. В разделе **WebGL**:
   - Убедитесь что **Default Canvas Width**: `1080`
   - Убедитесь что **Default Canvas Height**: `1920`

### 3. После сборки WebGL билда
После сборки WebGL, в HTML файле (обычно `index.html`) добавьте или проверьте наличие следующих meta тегов в `<head>`:

```html
<meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no">
<meta name="screen-orientation" content="portrait">
<meta name="mobile-web-app-capable" content="yes">
<meta name="apple-mobile-web-app-capable" content="yes">
```

### 4. Если используется кастомный шаблон WebGL
Если вы используете кастомный WebGL шаблон:
1. Найдите шаблон в папке `Assets/WebGLTemplates/`
2. Отредактируйте HTML файл и добавьте meta теги выше

## Проверка работы

1. Соберите WebGL билд
2. Откройте в браузере на мобильном устройстве или в эмуляторе
3. Попробуйте повернуть устройство - игра должна остаться вертикальной

## Важные заметки

- На некоторых устройствах браузер может игнорировать настройки ориентации, если страница не открыта в полноэкранном режиме
- Для PWA (Progressive Web App) нужно также настроить `manifest.json` с ориентацией `portrait`
- Скрипт `WebGLPortraitEnforcer` обеспечивает дополнительную защиту от поворота экрана во время выполнения игры

