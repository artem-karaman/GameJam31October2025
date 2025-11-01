using UnityEngine;
using System.Collections;

public class CleaverAnimChanger : MonoBehaviour
{
    public GameObject firstObject;
    public GameObject secondObject;  

    [Header("Задержка перед выключением второго (в секундах)")]
    public float delaySeconds = 1.5f;     // Время ожидания

    void Start()
    {
        StartCoroutine(SwitchObjects());
    }

    private IEnumerator SwitchObjects()
    {
        if (firstObject != null)
            firstObject.SetActive(false);
        
        if (secondObject != null)
            secondObject.SetActive(true);
        
        yield return new WaitForSeconds(delaySeconds);
        
        if (secondObject != null)
            secondObject.SetActive(false);
        
        if (firstObject != null)
            firstObject.SetActive(true);
    }
}