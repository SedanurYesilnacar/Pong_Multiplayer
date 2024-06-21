using UnityEngine;

namespace _GameData.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayerVisualData")]
    public class PlayerVisualData : ScriptableObject
    {
        public Sprite hostPlayerSprite;
        public Sprite clientPlayerSprite;
    }
}