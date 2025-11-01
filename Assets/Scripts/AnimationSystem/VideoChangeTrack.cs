using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using AnimationSystem;

[TrackBindingType(typeof(VideoController))]
[TrackClipType(typeof(VideoChangeClip))]
public class VideoChangeTrack : TrackAsset
{
    /// <summary>
    /// Пользовательский трек для Timeline.
    /// Определяет, что на этом треке могут лежать только VideoChangeClip и что трек биндится
    /// к объекту VideoController на сцене. При создании графа пробрасывает связанный контроллер во все клипы,
    /// выступает контейнером и “проводником” биндинга.
    /// </summary>
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        // Нам не нужен особый ScriptPlayable — достаточно базового Playable с нужным числом входов
        var playable = Playable.Create(graph, inputCount);

        // Пробрасываем биндинг трека (VideoController на объекте сцены) в каждый клип
        var director = go.GetComponent<PlayableDirector>();
        var bound = director != null ? director.GetGenericBinding(this) as VideoController : null;

        foreach (var clip in GetClips())
        {
            if (clip.asset is VideoChangeClip vc)
                vc.boundController = bound;
        }

        return playable;
    }
}