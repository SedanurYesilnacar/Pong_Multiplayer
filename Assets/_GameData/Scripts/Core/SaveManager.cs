using UnityEngine;

namespace _GameData.Scripts.Core
{
    public static class SaveManager
    {
        public static void SaveValue(SaveKeys prefKey, float saveValue)
        {
            PlayerPrefs.SetFloat(prefKey.ToString(), saveValue);
        }
        
        public static float GetValue(SaveKeys prefKey, float defaultValue = 0f)
        {
            return PlayerPrefs.GetFloat(prefKey.ToString(), defaultValue);
        }
    }

    public enum SaveKeys
    {
        MusicVolume
    }
}