using System;
using System.Collections.Generic;
using _GameData.Scripts.Core;
using UnityEngine;

namespace _GameData.Scripts.UI.MenuUI
{
    public class MenuTransitionManager : MonoBehaviour
    {
        [SerializeField] private List<MenuStateCanvas> menuStateCanvas;
        [SerializeField] private NotificationCanvas notificationCanvas;

        private Canvas _currentCanvas;
        private const string KickMessage = "You've been kicked from the lobby";

        private void OnEnable()
        {
            SubscribeEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            LobbyManager.Instance.OnMenuStateChangeRequested += OnMenuStateChangeRequestedHandler;
            LobbyManager.Instance.OnNotificationPopupRequested += OnNotificationPopupRequestedHandler;
            LobbyManager.Instance.OnPlayerKicked += OnPlayerKickedHandler;
        }

        private void UnsubscribeEvents()
        {
            LobbyManager.Instance.OnMenuStateChangeRequested -= OnMenuStateChangeRequestedHandler;
            LobbyManager.Instance.OnNotificationPopupRequested -= OnNotificationPopupRequestedHandler;
            LobbyManager.Instance.OnPlayerKicked -= OnPlayerKickedHandler;
        }

        private void Start()
        {
            LoadInitialState();
        }

        private void LoadInitialState()
        {
            ChangeState(LobbyManager.Instance.JoinedLobby != null ? MenuStates.Lobby : MenuStates.MainMenu);
        }

        public void ChangeState(MenuStates targetState)
        {
            if (_currentCanvas) _currentCanvas.enabled = false;
            if (targetState == MenuStates.None)
            {
                _currentCanvas = null;
                return;
            }
            
            var targetCanvas = menuStateCanvas.Find(i => i.state == targetState).canvas;
            _currentCanvas = targetCanvas;
            _currentCanvas.enabled = true;
            if (_currentCanvas.gameObject.TryGetComponent(out IInitializableCanvas initializableCanvas)) initializableCanvas.Init();
        }

        public void ShowNotification(string message)
        {
            notificationCanvas.Show(message);
        }

        private void OnMenuStateChangeRequestedHandler(MenuStates requestedMenuState)
        {
            ChangeState(requestedMenuState);
        }

        private void OnNotificationPopupRequestedHandler(string notificationMessage)
        {
            ShowNotification(notificationMessage);
        }

        private void OnPlayerKickedHandler()
        {
            ShowNotification(KickMessage);
            ChangeState(MenuStates.MainMenu);
        }
    }

    [Serializable]
    public struct MenuStateCanvas
    {
        public MenuStates state;
        public Canvas canvas;
    }

    public enum MenuStates
    {
        None,
        MainMenu,
        CreateLobby,
        LobbyBrowser,
        Settings,
        Lobby,
    }
}