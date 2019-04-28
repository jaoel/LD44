using UnityEngine;

//[RequireComponent(typeof(Camera))]
public class FollowCamera : MonoBehaviour {
    public Transform target;

    public bool showDeadzoneDebugRect = false;

    private float spring = 10f;
    private Vector2 deadZoneRectSize = new Vector2(0.3f, 0.15f);

    private new Camera camera;
    private Rect deadzoneRect;

    private void Start() {
        camera = GetComponentInChildren<Camera>();// GetComponent<Camera>();
        if(target == null || target.Equals(null)) {
            target = FindObjectOfType<Player>()?.transform;
        }
    }

    void Update() {
        deadzoneRect.width = Vector3.Distance(camera.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)), camera.ScreenToWorldPoint(new Vector3(Screen.width, 0f, 0f)));
        deadzoneRect.height = Vector3.Distance(camera.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)), camera.ScreenToWorldPoint(new Vector3(Screen.height, 0f, 0f)));
        deadzoneRect.size *= deadZoneRectSize;
        deadzoneRect.center = transform.position;

        if (showDeadzoneDebugRect) {
            DebugDraw();
        }

        Vector3 newPosition = transform.position;

        if (!TargetInDeadzoneX()) {
            float xDistance = target.position.x > deadzoneRect.x ? Mathf.Abs(target.position.x - deadzoneRect.max.x) : -Mathf.Abs(target.position.x - deadzoneRect.min.x);
            newPosition.x += spring * xDistance * Time.deltaTime;
        }

        if (!TargetInDeadzoneY()) {
            float yDistance = target.position.y > deadzoneRect.y ? Mathf.Abs(target.position.y - deadzoneRect.max.y) : -Mathf.Abs(target.position.y - deadzoneRect.min.y);
            newPosition.y += spring * yDistance * Time.deltaTime;
        }

        newPosition.z = transform.position.z;
        transform.position = newPosition;
    }

    bool TargetInDeadzoneX() {
        return target.position.x >= deadzoneRect.min.x
            && target.position.x <= deadzoneRect.max.x;
    }

    bool TargetInDeadzoneY() {
        return target.position.y >= deadzoneRect.min.y
            && target.position.y <= deadzoneRect.max.y;
    }

    void DebugDraw() {
        Vector3 tr = new Vector3(deadzoneRect.max.x, deadzoneRect.max.y, 0f);
        Vector3 tl = new Vector3(deadzoneRect.min.x, deadzoneRect.max.y, 0f);
        Vector3 br = new Vector3(deadzoneRect.max.x, deadzoneRect.min.y, 0f);
        Vector3 bl = new Vector3(deadzoneRect.min.x, deadzoneRect.min.y, 0f);
        Debug.DrawLine(tl, tr, Color.green);
        Debug.DrawLine(tr, br, Color.green);
        Debug.DrawLine(br, bl, Color.green);
        Debug.DrawLine(bl, tl, Color.green);
    }
}
