using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace _GameData.Scripts.UI
{
    public class LoadingCanvas : MonoBehaviour
    {
        [SerializeField] private Canvas loadingCanvas;
        
        private const float LoadingCompletionDelay = 0.1f;
        private WaitForSeconds _loadingCompletionDelay;
        private Coroutine _waitLoadingRoutine;
        
        public static LoadingCanvas Instance { get; private set; }

        private void Awake()
        {
            _loadingCompletionDelay = new WaitForSeconds(LoadingCompletionDelay);
            InitSingleton();
        }
        
        private void InitSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
            DontDestroyOnLoad(this);
        }

        public void Init(Task loadingTask)
        {
            if (_waitLoadingRoutine != null) return;
            _waitLoadingRoutine = StartCoroutine(WaitUntilTaskCompleted(loadingTask));
        }

        public void Init()
        {
            if (_waitLoadingRoutine != null) return;
            SetLoadingCanvasVisibility(true);
        }

        public void Hide()
        {
            SetLoadingCanvasVisibility(false);
        }

        private IEnumerator WaitUntilTaskCompleted(Task loadingTask)
        {
            SetLoadingCanvasVisibility(true);
            
            yield return new WaitUntil(() => loadingTask.IsCompleted);
            yield return _loadingCompletionDelay;

            SetLoadingCanvasVisibility(false);
            _waitLoadingRoutine = null;
        }

        private void SetLoadingCanvasVisibility(bool isVisible)
        {
            loadingCanvas.enabled = isVisible;
        }
    }
}