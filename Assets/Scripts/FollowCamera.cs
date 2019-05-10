using UnityEngine;

//[RequireComponent(typeof(Camera))]
public class FollowCamera : MonoBehaviour
{
    public Transform target;

    public bool showDeadzoneDebugRect = false;

    private readonly float _spring = 10f;
    private Vector2 _deadZoneRectSize = new Vector2(0.3f, 0.15f);

    private Camera _camera;
    private Rect _deadzoneRect;

    private void Start()
    {
        _camera = GetComponentInChildren<Camera>();// GetComponent<Camera>();
        if (target == null || target.Equals(null))
        {
            target = FindObjectOfType<Player>()?.transform;
        }
    }

    void Update()
    {
        _deadzoneRect.width = Vector3.Distance(_camera.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)), _camera.ScreenToWorldPoint(new Vector3(Screen.width, 0f, 0f)));
        _deadzoneRect.height = Vector3.Distance(_camera.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)), _camera.ScreenToWorldPoint(new Vector3(Screen.height, 0f, 0f)));
        _deadzoneRect.size *= _deadZoneRectSize;
        _deadzoneRect.center = transform.position;

        if (showDeadzoneDebugRect)
        {
            DebugDraw();
        }

        Vector3 newPosition = transform.position;

        if (!TargetInDeadzoneX())
        {
            float xDistance = target.position.x > _deadzoneRect.x ? Mathf.Abs(target.position.x - _deadzoneRect.max.x) : -Mathf.Abs(target.position.x - _deadzoneRect.min.x);
            newPosition.x += _spring * xDistance * Time.deltaTime;
        }

        if (!TargetInDeadzoneY())
        {
            float yDistance = target.position.y > _deadzoneRect.y ? Mathf.Abs(target.position.y - _deadzoneRect.max.y) : -Mathf.Abs(target.position.y - _deadzoneRect.min.y);
            newPosition.y += _spring * yDistance * Time.deltaTime;
        }

        newPosition.z = transform.position.z;
        transform.position = newPosition;
    }

    bool TargetInDeadzoneX()
    {
        return target.position.x >= _deadzoneRect.min.x && target.position.x <= _deadzoneRect.max.x;
    }

    bool TargetInDeadzoneY()
    {
        return target.position.y >= _deadzoneRect.min.y && target.position.y <= _deadzoneRect.max.y;
    }

    void DebugDraw()
    {
        Vector3 tr = new Vector3(_deadzoneRect.max.x, _deadzoneRect.max.y, 0f);
        Vector3 tl = new Vector3(_deadzoneRect.min.x, _deadzoneRect.max.y, 0f);
        Vector3 br = new Vector3(_deadzoneRect.max.x, _deadzoneRect.min.y, 0f);
        Vector3 bl = new Vector3(_deadzoneRect.min.x, _deadzoneRect.min.y, 0f);
        Debug.DrawLine(tl, tr, Color.green);
        Debug.DrawLine(tr, br, Color.green);
        Debug.DrawLine(br, bl, Color.green);
        Debug.DrawLine(bl, tl, Color.green);
    }
}
