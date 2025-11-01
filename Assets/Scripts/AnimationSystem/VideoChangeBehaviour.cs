// csharp
using UnityEngine.Playables;

namespace AnimationSystem
{
    public class VideoChangeBehaviour : PlayableBehaviour
    {
        /// <summary>
        /// Исполняемая логика клипа. Когда плеер “входит” в клип, один раз вызывает у контроллера смену видео
        /// с указанными параметрами; при паузе/выходе сбрасывает флаг, чтобы в следующий раз сработать снова
        /// </summary>
        public VideoController Controller;
        public string VideoName;
        public bool PlayOneShot;

        private bool _triggered;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (_triggered) return;
            _triggered = true;
            Controller?.ChangeVideo(VideoName, PlayOneShot);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            // Сбрасываем, чтобы повторно срабатывать при следующем входе в клип
            _triggered = false;
        }
    }
}