using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace _GameData.Scripts.Core
{
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance { get; private set; }

        public Lobby JoinedLobby { get; set; }
        public Player Player { get; private set; }

        private const string PlayerNameKey = "PlayerName";
        private const string PlayerBaseName = "Player";

        private void Awake()
        {
            InitSingleton();
        }
        
        private void InitSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
            DontDestroyOnLoad(this);
        }

        private async void Start()
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += SignedInHandler;
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            
            CreatePlayer();
        }

        private void SignedInHandler()
        {
            Debug.Log(AuthenticationService.Instance.PlayerId);
        }

        private void CreatePlayer()
        {
            var playerName = PlayerBaseName + Random.Range(1, 101);
            
            Player = new Player()
            {
                Data = new Dictionary<string, PlayerDataObject>()
                {
                    { PlayerNameKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                }
            };
        }
    }
}