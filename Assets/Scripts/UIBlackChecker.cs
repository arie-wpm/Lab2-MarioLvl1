using UnityEngine;

public class UIBlackChecker : MonoBehaviour
{
    [SerializeField] private GameObject _blackPanel;
    [SerializeField] private GameObject _normalCoin;
    [SerializeField] private GameObject _darkCoin;

    void Update() {
        if (_blackPanel.activeSelf) {
            _darkCoin.SetActive(true);
            _normalCoin.SetActive(false);
        } else {
            _darkCoin.SetActive(false);
            _normalCoin.SetActive(true);
        }
    }
}
