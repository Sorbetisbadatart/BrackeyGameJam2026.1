using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class DebugSceneControls : MonoBehaviour
    {
        [SerializeField] KeyCode reloadKey = KeyCode.R;
        [SerializeField] bool requireModifier = false;
        [SerializeField] KeyCode modifierKey = KeyCode.LeftShift;
        [SerializeField] bool resetTimeScale = true;
        [SerializeField] bool logMessage = true;

        void Update()
        {
            if (!Input.GetKeyDown(reloadKey)) return;
            if (requireModifier && !Input.GetKey(modifierKey)) return;
            var scene = SceneManager.GetActiveScene();
            if (logMessage) Debug.Log("DebugSceneControls: reloading scene '" + scene.name + "'", this);
            if (resetTimeScale) Time.timeScale = 1f;
            SceneManager.LoadScene(scene.buildIndex);
        }
    }
}
