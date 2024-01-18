using A01.Player;
using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace A01.Manager
{
    public class GameManager : GameFlowEventController
    {
        public static GameManager instance;

        [SerializeField] private PlayerController player;
        [SerializeField] private Fade fader;
        [SerializeField] private float loadWaitTime;

        private NavMeshSurface navMeshSurface;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            navMeshSurface = GetComponent<NavMeshSurface>();
        }

        private void Start()
        {
            AudioManager.instance.PlaySFX("Theme", 1, 1, true);
        }

        private void SetWinCondition(bool value)
        {
            AudioManager.instance.PlaySFX(value ? "Win" : "Lose");

            StartCoroutine(SceneLoadDelay());
        }

        //Will reload the main scene and begin the game again
        IEnumerator SceneLoadDelay()
        {
            fader.fadeOut = true;
            fader.gameObject.SetActive(true);

            yield return new WaitForSeconds(loadWaitTime);

            SceneManager.LoadScene(0);
            fader.fadeOut = false;
        }

        public override void TriggerGameFlowEvent(bool value)
        {
            SetWinCondition(value);
        }

        public void ReBuildNavMesh()
        {
            navMeshSurface.BuildNavMesh();
        }
    }
}

