using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GameData.Scripts.UI
{
    public class NotificationCanvas : MonoBehaviour
    {
        [SerializeField] private Canvas notificationCanvas;
        [SerializeField] private TMP_Text notificationMessageText;
        [SerializeField] private Button okButton;

        private void Start()
        {
            okButton.onClick.AddListener(OkClickHandler);
        }

        public void Show(string message)
        {
            notificationMessageText.text = message;
            notificationCanvas.enabled = true;
        }

        private void Hide()
        {
            notificationCanvas.enabled = false;
        }

        private void OkClickHandler()
        {
            Hide();
        }
    }
}