using UnityEngine;
using UnityEngine.UI;

// Shows the static keyboard & mouse control bindings grid. No tabs/sub-diagrams - just a Back button
// back to the main panel.
public class ControlsPanelController : MonoBehaviour
{
  [SerializeField] MainMenuController mainMenu;
  [SerializeField] Button backButton;

  void Awake()
  {
    backButton.onClick.AddListener(OnBack);
  }

  void OnBack()
  {
    mainMenu.ShowMain();
  }
}
