using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace _GameData.Scripts.UI
{
    public class MenuTransitionManager : MonoBehaviour
    {
        [SerializeField] private List<MenuStateCanvas> menuStateCanvas;

        private Canvas _currentCanvas;
        
        public Lobby CurrentLobby { get; set; }

        private void Start()
        {
            ChangeState(MenuStates.MainMenu);
        }

        public void ChangeState(MenuStates targetState)
        {
            if (_currentCanvas) _currentCanvas.enabled = false;
            var targetCanvas = menuStateCanvas.Find(i => i.state == targetState).canvas;
            _currentCanvas = targetCanvas;
            _currentCanvas.enabled = true;
            if (_currentCanvas.gameObject.TryGetComponent(out IInitializableCanvas initializableCanvas)) initializableCanvas.Init();
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