using UnityEngine;
using UnityEngine.UI;

// Shows the static keyboard & mouse control bindings grid. No tabs/sub-diagrams - just a Back button
// back to the main panel.
// Displays the controls panel and returns to the main menu when requested.
public class ControlsPanelController : MonoBehaviour
{
  [SerializeField] MainMenuController mainMenu;
  [SerializeField] Button backButton;

  // Registers the Back button callback.
  void Awake()
  {
    backButton.onClick.AddListener(OnBack);
  }

  // Returns to the main menu panel.
  void OnBack()
  {
    mainMenu.ShowMain();
  }
}
