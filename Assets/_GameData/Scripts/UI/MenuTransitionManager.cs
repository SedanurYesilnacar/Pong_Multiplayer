using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace _GameData.Scripts.UI
{
    public class MenuTransitionManager : MonoBehaviour
    {
        [SerializeField] private List<MenuStateCanvas> menuStateCanvas;
        [SerializeField] private NotificationCanvas notificationCanvas;

        private Canvas _currentCanvas;
        
        public Lobby CurrentLobby { get; set; }

        private void Start()
        {
            ChangeState(MenuStates.MainMenu);
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.T)) Debug.Log(CurrentLobby.Id);
        }

        public void ChangeState(MenuStates targetState)
        {
            if (_currentCanvas) _currentCanvas.enabled = false;
            var targetCanvas = menuStateCanvas.Find(i => i.state == targetState).canvas;
            _currentCanvas = targetCanvas;
            _currentCanvas.enabled = true;
            if (_currentCanvas.gameObject.TryGetComponent(out IInitializableCanvas initializableCanvas)) initializableCanvas.Init();
        }

        public void ShowNotification(string message)
        {
            notificationCanvas.Show(message);
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
        MainMenu,
        CreateLobby,
        LobbyBrowser,
        Settings,
        Loading,
        Lobby,
        
    }
}