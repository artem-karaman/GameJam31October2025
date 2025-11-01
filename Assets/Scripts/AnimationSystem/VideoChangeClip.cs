// csharp
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace AnimationSystem
{
    [System.Serializable]
    public class VideoChangeClip : PlayableAsset, ITimelineClipAsset
    {
        /// <summary>
        /// Описание одного клипа на таймлайне.
        /// Хранит параметры (имя видео, play one shot) и ссылку/ExposedReference на контроллер.
        /// При сборке графа создает ScriptPlayable с VideoChangeBehaviour и передает ему нужные значения.
        /// </summary>
        public ExposedReference<VideoController> controller;

        public string videoName;
        public bool playOneShot;

        [System.NonSerialized] public VideoController boundController;

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<VideoChangeBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();

            var resolved = controller.Resolve(graph.GetResolver());
            behaviour.Controller = resolved != null ? resolved : boundController;
            behaviour.VideoName = videoName;
            behaviour.PlayOneShot = playOneShot;

            return playable;
        }
    }
}