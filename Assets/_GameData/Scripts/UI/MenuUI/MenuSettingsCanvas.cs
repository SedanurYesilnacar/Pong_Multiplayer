using _GameData.Scripts.Core;

namespace _GameData.Scripts.UI.MenuUI
{
    public class MenuSettingsCanvas : BaseSettingsCanvas
    {
        protected override void BackClickHandler()
        {
            LobbyManager.Instance.OnMenuStateChangeRequested?.Invoke(MenuStates.MainMenu);
        }
    }
}