using UnityEngine;

public class UnityBody : IMover, IBody {
    private Transform transform;
    private CharacterController controller;

    private bool positionTick;
    private Vector3 nextPosition;

    public bool InGround {
        get {
            return this.controller.isGrounded;
        }
    }

    public bool InLegalGround {
        get;
        private set;
    }

    public Vector3 Velocity {
        get;
        private set;
    }

    public Vector3 LegalGroundPosition {
        get;
        private set;
    }

    public Vector3 GroundNormal {
        get;
        private set;
    }

    public UnityBody(Transform transform, CharacterController controller) {
        this.transform = transform;
        this.controller = controller;
    }

    public void LateUpdate() {
        if (this.positionTick) {
            this.transform.position = this.nextPosition;
            this.positionTick = false;
            this.Velocity = Vector3.zero;
        }

        if (!this.Velocity.Equal(Vector3.zero)) {
            var velocity = this.Velocity;

            if (this.InGround) {
                velocity.y -= 0.1f;
            }

            this.controller.Move(velocity);
            this.Velocity = Vector3.zero;
        }
    }

    public void Move(Vector3 velocity) {
        if (this.InGround && velocity.y <= 0) {
            velocity = Vector3.ProjectOnPlane(velocity, this.GroundNormal);
        }

        this.Velocity += velocity;
    }

    public void SetPosition(Vector3 position) {
        this.positionTick = true;
        this.nextPosition = position;
        
        // this.transform.position = position;
        // this.Velocity = Vector3.zero;
    }

    public void OnControllerColliderHit(ControllerColliderHit hit) {
        this.InLegalGround = this.IsLegalSlope(hit.normal);
        this.GroundNormal = hit.normal;

        if (this.InLegalGround) {
            this.LegalGroundPosition = hit.point;
        }
    }

    private bool IsLegalSlope(Vector3 normal) {
        float angle = Vector3.Angle(Vector3.up, normal);

        return angle <= this.controller.slopeLimit;
    }
}