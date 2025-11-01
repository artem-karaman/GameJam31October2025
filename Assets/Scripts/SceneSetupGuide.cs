using UnityEngine;

/// <summary>
/// Гайд по настройке сцены - показывает в Inspector подсказки
/// </summary>
[CreateAssetMenu(fileName = "SceneSetupGuide", menuName = "Castle Game/Scene Setup Guide", order = 1)]
public class SceneSetupGuide : ScriptableObject
{
    [TextArea(10, 20)]
    public string guideText = @"
=== БЫСТРЫЙ ГАЙД ПО НАСТРОЙКЕ СЦЕНЫ ===

1. ПОДГОТОВКА ПРЕФАБОВ:
   
   а) Префаб игрока:
      - Создайте GameObject
      - Добавьте компонент PrefabSetupHelper
      - Правый клик → 'Setup as Player Prefab'
      - Назначьте свой Sprite в Image (опционально)
      - Сохраните как префаб
   
   б) Префаб крюка:
      - Создайте GameObject
      - Добавьте компонент HookUISetupHelper
      - Правый клик → 'Setup Hook Prefab'
      - Назначьте свой Sprite в Image (опционально)
      - Настройте толщину линии в HookUI.lineThickness
      - Сохраните как префаб
   
   в) Префабы монстров:
      - Создайте GameObject для каждого типа монстра
      - Добавьте компонент PrefabSetupHelper
      - Правый клик → 'Setup as Monster Prefab'
      - Назначьте свой Sprite в Image
      - Добавьте Animator и AnimatorController (опционально)
      - Сохраните как префаб
      - Повторите для всех типов монстров

2. НАСТРОЙКА СЦЕНЫ:
   
   а) Добавьте пустой GameObject в сцену
   б) Добавьте компонент CastleSceneSetupUI
   в) В Inspector назначьте префабы:
      - playerPrefab (опционально)
      - hookPrefab (опционально)
      - monsterPrefab (для старого способа, опционально)
   
   г) Правый клик по CastleSceneSetupUI → 'Setup Castle Scene UI'
      Это создаст все необходимые объекты на Canvas

3. НАСТРОЙКА ПУЛА МОНСТРОВ:
   
   СПОСОБ 1 - Автоматический (рекомендуется):
   
   а) Найдите объект MonsterSpawner на Canvas
   б) Откройте компонент MonsterPoolSetupHelper
   в) Перетащите все префабы монстров в массив monsterPrefabs
   г) Настройте параметры:
      - useSameWeight: true (одинаковый вес для всех)
      - defaultWeight: вес спавна (1-10)
      - defaultPoolSize: минимальный размер пула
   
   д) Правый клик → 'Автоматическая настройка из префабов'
      Это создаст массив типов монстров автоматически
   
   СПОСОБ 2 - Ручной:
   
   а) Найдите объект MonsterSpawner
   б) Откройте компонент MonsterSpawnerUI
   в) В 'Monster Pool System' установите размер массива monsterTypes
   г) Для каждого элемента:
      - Назначьте monsterPrefab
      - Укажите monsterTypeName
      - Настройте spawnWeight (1-10)
      - Установите poolMinSize (0-20)

4. ПРОВЕРКА НАСТРОЙКИ:
   
   - Правый клик по MonsterPoolSetupHelper → 'Проверить настройку пулов'
   - Правый клик по MonsterSpawnerUI → 'Print Pool Status' (в Play Mode)

5. ЗАПУСК ИГРЫ:
   
   - Нажмите Play
   - Все должно работать автоматически!

=== ПОДСКАЗКИ ===

- Веса спавна: тип с весом 5 появляется в 5 раз чаще чем с весом 1
- Размер пула: больше = меньше созданий во время игры, но больше памяти
- Можно использовать mix: некоторые типы в массиве, один в monsterPrefab
";

    [ContextMenu("Показать гайд в консоли")]
    public void ShowGuide()
    {
        Debug.Log(guideText);
    }
}

