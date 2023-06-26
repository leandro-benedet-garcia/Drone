using UnityEngine;
using UnityEngine.InputSystem;

using TMPro;

namespace DroneGame
{
  class Selection : MonoBehaviour
  {
    public Transform selected;
    TextMeshProUGUI _informationText;

    void Awake()
    {
      _informationText = GameObject.Find("Canvas/SelectionInformation")
                                   .GetComponent<TextMeshProUGUI>()
                                   ?? throw new("Component Not found");
    }

    void OnMouseClick(InputValue value)
    {
      var mousePosition = Mouse.current
                               .position
                               .ReadValue();
      var ray = Camera.main
                      .ScreenPointToRay(mousePosition);

      if (Physics.Raycast(ray, out RaycastHit hit))
      {
        selected = hit.collider
                      .transform
                      .parent;
        var connector = selected.GetComponent<Connector>();
        _informationText.text = $@"From {connector.from.letterCoordinate}
To {connector.to.letterCoordinate}
Distance {connector.distance}";
      }
    }
  }
}
