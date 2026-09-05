using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

// Master / Music / SFX volume sliders bound to the AudioMixer's exposed parameters, persisted via
// AudioMixerVolume/PlayerPrefs so they carry over between sessions.
public class SettingsPanelController : MonoBehaviour
{
  [SerializeField] MainMenuController mainMenu;
  [SerializeField] AudioMixer mixer;
  [SerializeField] Slider masterSlider;
  [SerializeField] Slider musicSlider;
  [SerializeField] Slider sfxSlider;
  [SerializeField] Button backButton;

  void Awake()
  {
    backButton.onClick.AddListener(OnBack);

    masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
    musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
    sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

    LoadSavedValues();
  }

  void OnEnable()
  {
    LoadSavedValues();
  }

  void LoadSavedValues()
  {
    masterSlider.SetValueWithoutNotify(AudioMixerVolume.GetSaved(AudioMixerVolume.MasterParam));
    musicSlider.SetValueWithoutNotify(AudioMixerVolume.GetSaved(AudioMixerVolume.MusicParam));
    sfxSlider.SetValueWithoutNotify(AudioMixerVolume.GetSaved(AudioMixerVolume.SFXParam));
  }

  void OnMasterVolumeChanged(float value)
  {
    AudioMixerVolume.Set(mixer, AudioMixerVolume.MasterParam, value);
  }

  void OnMusicVolumeChanged(float value)
  {
    AudioMixerVolume.Set(mixer, AudioMixerVolume.MusicParam, value);
  }

  void OnSFXVolumeChanged(float value)
  {
    AudioMixerVolume.Set(mixer, AudioMixerVolume.SFXParam, value);
  }

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
