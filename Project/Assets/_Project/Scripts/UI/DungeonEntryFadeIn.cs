using UnityEngine;

// Lives in the UI scene next to the fade Image (authored fully opaque at rest, so a freshly loaded
// Dungeon stays hidden while it generates). Fades the screen in once DungeonReadySignal fires,
// rather than on scene Start, so the player never sees the level being built mid-fade.
[RequireComponent(typeof(ScreenFader))]
public class DungeonEntryFadeIn : MonoBehaviour
{
  [SerializeField, Min(0f)] float fadeInDuration = 1.5f;

  ScreenFader fader;

  void Awake()
  {
    fader = GetComponent<ScreenFader>();
  }

  void OnEnable()
  {
    DungeonReadySignal.Raised += HandleDungeonReady;
  }

  void OnDisable()
  {
    DungeonReadySignal.Raised -= HandleDungeonReady;
  }

  void HandleDungeonReady()
  {
    fader.FadeInAndStart(fadeInDuration);
  }
}
