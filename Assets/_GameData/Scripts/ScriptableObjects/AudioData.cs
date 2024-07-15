using UnityEngine;

namespace _GameData.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "AudioData")]
    public class AudioData : ScriptableObject
    {
        public AudioClip audioClip;
        public float volume;
        public bool useRandomPitch;
        public float minPitch;
        public float maxPitch;
    }
}