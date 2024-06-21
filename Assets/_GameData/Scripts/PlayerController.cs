using Unity.Netcode;
using UnityEngine;

namespace _GameData.Scripts
{
    public class PlayerController : NetworkBehaviour
    {
        private const float EdgeOffset = 8f;
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            SetInitialPosition();
        }

        private void SetInitialPosition()
        {
            float edgeMultiplier;
            if (IsHost)
            {
                if (IsOwner) edgeMultiplier = -1f;
                else edgeMultiplier = 1f;
            }
            else
            {
                if (IsOwner) edgeMultiplier = 1f;
                else edgeMultiplier = -1f;
            }
            
            transform.position = Vector3.right * EdgeOffset * edgeMultiplier;
        }
    }
}
