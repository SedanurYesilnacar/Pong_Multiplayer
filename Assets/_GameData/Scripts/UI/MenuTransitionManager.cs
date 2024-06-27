using UnityEngine;

namespace _GameData.Scripts.UI
{
    public class MenuTransitionManager : MonoBehaviour
    {
        [SerializeField] private Canvas mainMenuCanvas;
        [SerializeField] private Canvas notificationCanvas;
    }

    public enum MenuStates
    {
        MainMenu,
        
    }
}