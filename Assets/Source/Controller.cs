using UnityEngine;

public class Controller : MonoBehaviour {
    public float speed;

    private Body body;
    private Vector3 laterDirection;
    private EaseMove dashMove;
    private EaseMove jumpMove;

    private Vector3 velocity;

    protected void Awake() {
        this.body = this.GetComponent<Body>();
        this.laterDirection = Vector3.left;

        this.dashMove = new EaseMove(this.body);
        this.jumpMove = new EaseMove(this.body);
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
            this.dashMove.Enter(0.8f, 0.05f, this.laterDirection);
        }
        else if (Input.GetKeyDown(KeyCode.Space)) {
            this.jumpMove.Enter(0.7f, 0.02f, Vector3.up);
        }

        if (dir != Vector3.zero) {
            this.velocity = dir.normalized * this.speed;
            this.laterDirection = dir.normalized;
        }
    }

    protected void FixedUpdate() {
        this.dashMove.Update();
        this.jumpMove.Update();

        if (this.velocity != Vector3.zero) {
            this.body.Move(this.velocity);
            this.velocity = Vector3.zero;
        }

        if (this.transform.position.y < -1000) {
            this.body.SetPosition(this.body.LegalPosition, true);
        }
    }
}