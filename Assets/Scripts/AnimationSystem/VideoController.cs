using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    [SerializeField] private string folderName;
    [SerializeField] private string newVideoName; // Название видео без расширения
    private VideoPlayer videoPlayer;
    private string currentVideoName;

    [SerializeField] private bool isOnceAnim; // Флаг: следующее видео проигрывать один раз

    // Путь к папке с видео внутри StreamingAssets
    private string videosFolderPath;

    // Служебные поля для разовой анимации
    private string previousVideoName;  // Какое видео нужно вернуть после разового
    private bool isPlayingOneShot;     // Сейчас проигрывается разовая анимация?

    void Start()
    {
        currentVideoName = newVideoName; // Всегда со старта задаем дефолтное имя
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer == null)
        {
            Debug.LogError("На объекте отсутствует компонент VideoPlayer.");
            return;
        }

        // Подписываемся на окончание клипа (в т.ч. для незацикленного)
        videoPlayer.loopPointReached += OnVideoLoopPointReached;

        // Все видосы лежат по пути Assets/StreamingAssets/Videos
        videosFolderPath = System.IO.Path.Combine(Application.streamingAssetsPath, "Videos");

        PlayVideo(false);
    }

    public void ChangeVideo(string updateVideoName, bool isOneTimeAnim)
    {
        newVideoName = updateVideoName;
        isOnceAnim = isOneTimeAnim; // Это указание для следующего переключения
    }

    void Update()
    {
        if (string.IsNullOrEmpty(newVideoName)) return;

        if (newVideoName != currentVideoName)
        {
            // Запоминаем предыдущее только если запускаем разовый клип
            string prev = currentVideoName;
            bool playOnce = isOnceAnim;

            // Переключаем текущее имя
            currentVideoName = newVideoName;

            if (playOnce)
            {
                // Если до этого не было разового — зафиксируем "базовое" для возврата
                if (!isPlayingOneShot)
                    previousVideoName = prev;
                // Иначе уже идёт разовый — previousVideoName оставляем как было
            }

            PlayVideo(playOnce);
        }
    }

    private void PlayVideo(bool playOnce)
    {
        if (videoPlayer == null) return;
        
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "Videos", folderName, currentVideoName + ".mov");

        videoPlayer.url = path;
        videoPlayer.isLooping = !playOnce;
        isPlayingOneShot = playOnce;

        videoPlayer.Play();
    }

    private void OnVideoLoopPointReached(VideoPlayer vp)
    {
        // Срабатывает в конце клипа (и каждый раз на цикле). Нас интересует только разовый.
        if (!isPlayingOneShot) return;

        // Завершили разовое — возвращаемся к предыдущему в бесконечный цикл
        isPlayingOneShot = false;
        isOnceAnim = false;

        if (!string.IsNullOrEmpty(previousVideoName) && previousVideoName != currentVideoName)
        {
            currentVideoName = previousVideoName;
            newVideoName = previousVideoName; // держим значения в синхронизации
            PlayVideo(false);
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoLoopPointReached;
    }
}