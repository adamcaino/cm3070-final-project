using UnityEngine;

/// <summary>
/// Plain background-music player - swaps the looping clip on a single AudioSource. No crossfade or
/// mixing; nothing in this project's audio does that today, and callers (e.g. BossRoomEncounter) only need
/// a hard cut between boss music and ambience. One instance is expected per gameplay scene.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
  AudioSource source;

  void Awake()
  {
    source = GetComponent<AudioSource>();
    source.loop = true;
    source.playOnAwake = false;
  }

  public void Play(AudioClip clip)
  {
    if (clip == null)
    {
      Stop();
      return;
    }

    if (source.clip == clip && source.isPlaying) return;

    source.clip = clip;
    source.Play();
  }

  public void Stop()
  {
    source.Stop();
    source.clip = null;
  }
}
