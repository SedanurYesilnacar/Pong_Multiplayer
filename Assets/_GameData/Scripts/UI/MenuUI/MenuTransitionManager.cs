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
        
        private void Start()
        {
            LoadInitialState();

            LobbyManager.Instance.OnPlayerKicked += OnPlayerKickedHandler;
        }

        private void LoadInitialState()
        {
            if (LobbyManager.Instance.JoinedLobby != null) ChangeState(MenuStates.Lobby);
            else ChangeState(MenuStates.MainMenu);
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

        private void OnPlayerKickedHandler()
        {
            ShowNotification(KickMessage);
            ChangeState(MenuStates.MainMenu);
        }

        private void OnDisable()
        {
            LobbyManager.Instance.OnPlayerKicked -= OnPlayerKickedHandler;
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