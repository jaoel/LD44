using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class FollowCamera : MonoBehaviour {

    public Transform target;
    public float deadZoneRadius = 10f;
    public Vector3 velocity = Vector3.zero;

    private new Camera camera;

    private void Start() {
        camera = GetComponent<Camera>();
        if(target == null || target.Equals(null)) {
            target = FindObjectOfType<Player>()?.transform;
        }
    }

    void Update() {
        float screenWidthInWorldUnits = Vector3.Distance(camera.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)), camera.ScreenToWorldPoint(new Vector3(Screen.width, 0f, 0f)));
        deadZoneRadius = screenWidthInWorldUnits / 4f;
        //DebugDrawRadius();

        Vector3 toTarget = target.position - transform.position;
        //if (toTarget.magnitude > centerRadius) {
            Vector3 newPosition = transform.position + 0.05f * toTarget;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
        //}
    }

    void DebugDrawRadius() {
        Vector3 center = transform.position;
        center.z = -2f;
        for (int i = 0; i < 36; i++) {
            Vector3 start = center + Quaternion.Euler(0f, 0f, i * 10f) * Vector3.up * deadZoneRadius;
            Vector3 end = center + Quaternion.Euler(0f, 0f, (i + 1) * 10f) * Vector3.up * deadZoneRadius;
            Debug.DrawLine(start, end, Color.green);
        }
    }
}
