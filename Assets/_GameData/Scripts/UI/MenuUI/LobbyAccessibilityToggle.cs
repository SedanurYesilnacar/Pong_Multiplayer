using TMPro;
using UnityEngine;

namespace _GameData.Scripts.UI.MenuUI
{
    public class LobbyAccessibilityToggle : MonoBehaviour
    {
        [SerializeField] private TMP_Text lobbyAccessibilityText;

        public LobbyAccessibilityType CurrentAccessibilityType { get; private set; } =  LobbyAccessibilityType.Public;

        public void ToggleAccessibility()
        {
            CurrentAccessibilityType = CurrentAccessibilityType == LobbyAccessibilityType.Public
                ? LobbyAccessibilityType.Private
                : LobbyAccessibilityType.Public;

            lobbyAccessibilityText.text = CurrentAccessibilityType.ToString();
        } 
    }
    
    public enum LobbyAccessibilityType { Public, Private }
}