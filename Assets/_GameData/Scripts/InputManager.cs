using UnityEngine;

namespace _GameData.Scripts
{
    public class InputManager : MonoBehaviour
    {
        private PlayerControls _playerControls;
        public float VerticalValue { get; private set; }

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            SetupInputSystem();
        }

        private void SetupInputSystem()
        {
            _playerControls = new PlayerControls();
            _playerControls.Movement.Enable();
        }

        private void Update()
        {
            VerticalValue = _playerControls.Movement.Move.ReadValue<float>();
        }
    }
}