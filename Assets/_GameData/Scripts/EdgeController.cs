using UnityEngine;

namespace _GameData.Scripts
{
    public class EdgeController : MonoBehaviour
    {
        [field: SerializeField] public bool IsHostSide { get; private set; }
    }
}