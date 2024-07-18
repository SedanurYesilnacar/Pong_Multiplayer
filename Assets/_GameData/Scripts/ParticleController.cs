using UnityEngine;

namespace _GameData.Scripts
{
    public class ParticleController : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particle;

        public void Play(Vector3 targetPosition)
        {
            particle.transform.position = targetPosition;
            particle.Play();
        }
    }
}