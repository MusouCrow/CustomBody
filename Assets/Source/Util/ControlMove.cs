using UnityEngine;

public class ControlMove {
    public float speed;
    
    private IMover mover;

    public ControlMove(IMover mover, float speed) {
        this.mover = mover;
        this.speed = speed;
    }

    public void Update() {
        var direction = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) {
            direction.z = 1;
        }
        else if (Input.GetKey(KeyCode.S)) {
            direction.z = -1;
        }

        if (Input.GetKey(KeyCode.A)) {
            direction.x = -1;
        }
        else if (Input.GetKey(KeyCode.D)) {
            direction.x = 1;
        }

        if (!direction.Equal(Vector3.zero)) {
            this.mover.Move(direction.normalized * this.speed);
        }
    }
}