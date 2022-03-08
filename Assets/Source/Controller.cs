using UnityEngine;

public class Controller : MonoBehaviour {
    public float speed;

    private Body body;
    private Vector3 laterDirection;

    protected void Awake() {
        this.body = this.GetComponent<Body>();
        this.laterDirection = Vector3.left;
    }

    protected void Update() {
        var dir = Vector3.zero;

        if (Input.GetKey(KeyCode.A)) {
            dir.x = -1;
        }
        else if (Input.GetKey(KeyCode.D)) {
            dir.x = 1;
        }

        if (Input.GetKey(KeyCode.W)) {
            dir.z = 1;
        }
        else if (Input.GetKey(KeyCode.S)) {
            dir.z = -1;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift)) {
            this.body.Move(this.laterDirection * 5);
        }

        if (dir != Vector3.zero) {
            this.body.Move(dir.normalized * this.speed);
            this.laterDirection = dir.normalized;
        }

        if (this.transform.position.y < -1000) {
            this.body.SetPosition(this.body.LegalPosition, true);
        }
    }
}