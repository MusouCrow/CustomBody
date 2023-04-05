using UnityEngine;

public class Entity : MonoBehaviour {
    public SolidBody Body {
        get;
        private set;
    }

    private ControlMove controlMove;
    private EaseMove flightMove;

    private void Awake() {
        var controller = this.GetComponent<CharacterController>();
        this.Body = new SolidBody(this.transform, controller);

        this.controlMove = new ControlMove(this.Body, 0.1f);
        this.flightMove = new EaseMove(this.Body);
    }

    private void Start() {
        this.Body.Start();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space) && this.Body.InGround) {
            this.Flight(0.45f, 0.035f);
        }
    }

    private void FixedUpdate() {
        this.controlMove.Update();
        this.flightMove.Update();
        this.Body.Update(Time.fixedDeltaTime);
    }

    private void OnFlightEnd() {
        this.Body.gravityRate = 1;
    }

    public void Flight(float power, float speed) {
        this.flightMove.Enter(power, speed, Vector3.up, false, this.OnFlightEnd);
        this.Body.gravityRate = 0;
    }
}