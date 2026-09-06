using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

// Master / Music / SFX volume sliders bound to the AudioMixer's exposed parameters, persisted via
// AudioMixerVolume/PlayerPrefs so they carry over between sessions.
// Binds audio settings sliders to persisted AudioMixer volume values.
public class SettingsPanelController : MonoBehaviour
{
  [SerializeField] MainMenuController mainMenu;
  [SerializeField] AudioMixer mixer;
  [SerializeField] Slider masterSlider;
  [SerializeField] Slider musicSlider;
  [SerializeField] Slider sfxSlider;
  [SerializeField] Button backButton;

  // Registers button and slider callbacks, then loads saved values.
  void Awake()
  {
    backButton.onClick.AddListener(OnBack);

    masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
    musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
    sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

    LoadSavedValues();
  }

  // Reloads saved slider values when the settings panel becomes visible.
  void OnEnable()
  {
    LoadSavedValues();
  }

  // Updates sliders without triggering new persistence writes.
  void LoadSavedValues()
  {
    masterSlider.SetValueWithoutNotify(AudioMixerVolume.GetSaved(AudioMixerVolume.MasterParam));
    musicSlider.SetValueWithoutNotify(AudioMixerVolume.GetSaved(AudioMixerVolume.MusicParam));
    sfxSlider.SetValueWithoutNotify(AudioMixerVolume.GetSaved(AudioMixerVolume.SFXParam));
  }

  // Persists and applies the master volume value.
  void OnMasterVolumeChanged(float value)
  {
    AudioMixerVolume.Set(mixer, AudioMixerVolume.MasterParam, value);
  }

  // Persists and applies the music volume value.
  void OnMusicVolumeChanged(float value)
  {
    AudioMixerVolume.Set(mixer, AudioMixerVolume.MusicParam, value);
  }

  // Persists and applies the sound-effect volume value.
  void OnSFXVolumeChanged(float value)
  {
    AudioMixerVolume.Set(mixer, AudioMixerVolume.SFXParam, value);
  }

  // Returns to the pause menu, main menu, or hides the panel when no owner is found.
  void OnBack()
  {
    PauseMenuController pauseMenu = FindFirstObjectByType<PauseMenuController>();
    if (pauseMenu != null)
    {
      pauseMenu.CloseOptions();
      return;
    }

    MainMenuController menu = mainMenu != null
      ? mainMenu
      : FindFirstObjectByType<MainMenuController>();

    if (menu != null)
    {
      menu.ShowMain();
      return;
    }

    gameObject.SetActive(false);
  }
}
