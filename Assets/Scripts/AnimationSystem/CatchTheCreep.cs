using UnityEngine;
using System.Collections;

public class CatchTheCreep : MonoBehaviour
{
    public GameObject firstObject;   // Крип убит, отключаем его атаку
    public GameObject secondObject;  // Крип умирает, включаем анимацию смэрти

    [Header("Задержка перед выключением второго (в секундах)")]
    public float delaySeconds = 1.5f;     // Время ожидания

    void Start()
    {
        StartCoroutine(SwitchObjects());
    }

    private IEnumerator SwitchObjects()
    {
        // 1️⃣ Сразу отключаем первый объект
        if (firstObject != null)
            firstObject.SetActive(false);

        // 2️⃣ Включаем второй
        if (secondObject != null)
            secondObject.SetActive(true);

        // 3️⃣ Ждём delaySeconds секунд
        yield return new WaitForSeconds(delaySeconds);

        // 4️⃣ Выключаем второй объект
        if (secondObject != null)
            secondObject.SetActive(false);
    }
}