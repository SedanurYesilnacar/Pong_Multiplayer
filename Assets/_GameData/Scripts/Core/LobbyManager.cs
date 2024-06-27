using System;
using System.Collections;
using System.Collections.Generic;
using _GameData.Scripts.UI;
using QFSW.QC;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace _GameData.Scripts.Core
{
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField] private CreateLobbyCanvas createLobbyCanvas;
        
        private WaitForSeconds _heartbeatTimer; // Active lifespan = 30s
        private Lobby _createdLobby;
        
        private void Awake()
        {
            _heartbeatTimer = new WaitForSeconds(25f);
            
            DontDestroyOnLoad(this);
        }

        private async void Start()
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += SignedInHandler;
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            
            InitSubCanvas();
        }

        private void SignedInHandler()
        {
            Debug.Log(AuthenticationService.Instance.PlayerId);
        }

        private void InitSubCanvas()
        {
            createLobbyCanvas.Init(CreateLobby);
        }

        [Command]
        private async void CreateLobby()
        {
            var userLobbyOptions = createLobbyCanvas.GetLobbyOptions();
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = userLobbyOptions.LobbyAccessibilityType == LobbyAccessibilityType.Private
            };
            
            try
            {
                _createdLobby = await LobbyService.Instance.CreateLobbyAsync(userLobbyOptions.LobbyName, 2, lobbyOptions);
                StartCoroutine(HeartbeatRoutine());
                Debug.Log(_createdLobby.Name);
                Debug.Log(_createdLobby.MaxPlayers);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }

        [Command]
        private async void ListLobbies()
        {
            QueryLobbiesOptions queryOption = new QueryLobbiesOptions()
            {
                Count = 25,
                Filters = new List<QueryFilter>() { new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT) },
                Order = new List<QueryOrder>() { new QueryOrder(false, QueryOrder.FieldOptions.Created) }
            };
            
            try
            {
                var queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryOption);
                Debug.Log("Found lobbies : " + queryResponse.Results.Count);
                for (int i = 0; i < queryResponse.Results.Count; i++)
                {
                    Debug.Log(queryResponse.Results[i].Name + " " + (queryResponse.Results[i].MaxPlayers - queryResponse.Results[i].AvailableSlots) + "/" + queryResponse.Results[i].MaxPlayers);
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }

        private IEnumerator HeartbeatRoutine()
        {
            yield return _heartbeatTimer;
            
            if (_createdLobby == null) yield break;
            
            yield return LobbyService.Instance.SendHeartbeatPingAsync(_createdLobby.Id);
            StartCoroutine(HeartbeatRoutine());
        }

        [Command]
        private async void JoinLobbyByCode(string lobbyCode)
        {
            try
            {
                await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
                
                Debug.Log("Joined lobby");
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }

        [Command]
        private async void QuickJoin()
        {
            try
            {
                await LobbyService.Instance.QuickJoinLobbyAsync();
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }

        [Command]
        private void PrintPlayers(Lobby targetLobby)
        {
            Debug.Log("Players in lobby : " + targetLobby.Name);
            for (int i = 0; i < targetLobby.Players.Count; i++)
            {
                Debug.Log(targetLobby.Players[i].Id);
            }
        }
    }
}