using System;
using UnityEngine;

namespace _GameData.Scripts
{
    public class InputManager : MonoBehaviour
    {
        public float VerticalValue { get; private set; }

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Update()
        {
            VerticalValue = Input.GetKey(KeyCode.S) ? -1f : Input.GetKey(KeyCode.W) ? 1f : 0f;
        }
    }
}