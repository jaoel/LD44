using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    private float maxRange = 4f;

    [SerializeField]
    private Camera _camera;

    private void Start()
    {
        if (target == null || target.Equals(null))
        {
            target = FindObjectOfType<Player>()?.transform;
        }
    }

    void Update()
    {
        Vector2 mousePosNormalized = new Vector2(2f * Input.mousePosition.x / Screen.width - 1f, 2f * Input.mousePosition.y / Screen.height - 1f);
        mousePosNormalized = mousePosNormalized.normalized * Mathf.SmoothStep(0f, 1f, Vector3.ClampMagnitude(mousePosNormalized, 1f).magnitude);

        Vector2 targetPosition = target.position.ToVector2();
        Vector2 newPosition = targetPosition + mousePosNormalized * maxRange;

        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
    }
}
