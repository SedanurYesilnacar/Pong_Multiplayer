using Unity.Netcode.Components;
using UnityEngine;

namespace _GameData.Scripts
{
    public class ClientNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}