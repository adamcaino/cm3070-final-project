using UnityEngine;
using UnityEngine.UI;

// Shows the static keyboard & mouse control bindings grid. No tabs/sub-diagrams - just a Back button
// back to the main panel.
// Displays the controls panel and returns to the main menu when requested.
public class ControlsPanelController : MonoBehaviour
{
  [SerializeField] MainMenuController mainMenu;
  [SerializeField] PauseMenuController pauseMenu;
  [SerializeField] Button backButton;
  [SerializeField] Slider mouseSensitivitySlider;

  // Registers the Back button callback.
  void Awake()
  {
    backButton.onClick.AddListener(OnBack);

    if (mouseSensitivitySlider == null)
    {
      mouseSensitivitySlider = GetComponentInChildren<Slider>(true);
    }

    if (mouseSensitivitySlider != null)
    {
      mouseSensitivitySlider.minValue = PlayerCameraOrbit.MinSensitivity;
      mouseSensitivitySlider.maxValue = PlayerCameraOrbit.MaxSensitivity;
      mouseSensitivitySlider.SetValueWithoutNotify(PlayerCameraOrbit.GetSensitivity());
      mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
    }
  }

  // Refreshes slider display when this panel opens.
  void OnEnable()
  {
    if (mouseSensitivitySlider != null)
    {
      mouseSensitivitySlider.SetValueWithoutNotify(PlayerCameraOrbit.GetSensitivity());
    }
  }

  // Removes listeners to avoid duplicate subscriptions after domain reloads.
  void OnDestroy()
  {
    backButton.onClick.RemoveListener(OnBack);

    if (mouseSensitivitySlider != null)
    {
      mouseSensitivitySlider.onValueChanged.RemoveListener(OnMouseSensitivityChanged);
    }
  }

  // Saves sensitivity changes so gameplay camera picks them up in Update.
  void OnMouseSensitivityChanged(float value)
  {
    PlayerCameraOrbit.SetSensitivity(value);
  }

  // Returns to the main menu panel.
  void OnBack()
  {
    if (pauseMenu != null)
    {
      pauseMenu.CloseControls();
      return;
    }

    mainMenu.ShowMain();
  }
}
