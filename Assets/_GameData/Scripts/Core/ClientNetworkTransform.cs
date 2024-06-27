using Unity.Netcode.Components;

namespace _GameData.Scripts.Core
{
    public class ClientNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}