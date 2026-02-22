using UnityEngine;

namespace Game
{
    public class Rotator : MonoBehaviour
    {
        [SerializeField] Vector3 eulerDegreesPerSecond = new Vector3(0f, 90f, 0f);
        [SerializeField] Space rotateSpace = Space.Self;
        [SerializeField] bool useUnscaledTime = false;
        [SerializeField] bool randomizeStartRotation = false;
        [SerializeField] Vector3 randomStartEulerMin = Vector3.zero;
        [SerializeField] Vector3 randomStartEulerMax = Vector3.zero;

        void OnEnable()
        {
            if (randomizeStartRotation)
            {
                float rx = Random.Range(randomStartEulerMin.x, randomStartEulerMax.x);
                float ry = Random.Range(randomStartEulerMin.y, randomStartEulerMax.y);
                float rz = Random.Range(randomStartEulerMin.z, randomStartEulerMax.z);
                transform.Rotate(new Vector3(rx, ry, rz), rotateSpace);
            }
        }

        void Update()
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            transform.Rotate(eulerDegreesPerSecond * dt, rotateSpace);
        }
    }
}
