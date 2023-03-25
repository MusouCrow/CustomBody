using UnityEngine;

public class Tester : MonoBehaviour {
    public struct CastHit {
        public bool collided;
        public Vector3 position;
        public Vector3 normal;
        public Vector3 direction;
        public float distance;
        public GameObject gameObject;
    }

    public float radius = 1;
    public float height = 1.5f;
    public float slopeLimit = 45;
    public Vector3 velocity;

    private CastHit groundHit;
    private CastHit moveHit;

    public bool InGround {
        get {
            return this.groundHit.distance <= 0.1f;
        }
    }

    public Vector3 Position {
        get {
            return this.transform.position + Vector3.up * 0.001f;
        }
    }

    protected void Update() {        
        this.CheckGround();

        var direction = this.velocity.normalized;
        var distance = this.velocity.magnitude;
        var normal = Vector3.up;
        int count = 0;

        if (this.velocity.y.Equal(0) && this.InGround && this.IsLegalSlope(this.groundHit.normal)) {
            normal = this.groundHit.normal;
        }

        this.moveHit = this.Simulate(this.Position, direction, distance, normal, ref count);
    }

    protected void OnDrawGizmos() {
        if (!Application.isPlaying) {
            return;
        }

        this.DrawCapsuleGizmos(this.Position, Color.yellow);
        this.DrawCapsuleGizmos(this.moveHit.position, Color.green);

        if (!this.InGround) {
            this.DrawCapsuleGizmos(this.groundHit.position, Color.black);
        }
        else {
            Gizmos.color = Color.blue;

            var normal = Vector3.ProjectOnPlane(this.transform.forward, this.groundHit.normal);
            Gizmos.DrawRay(this.groundHit.position, normal);

            normal = Vector3.ProjectOnPlane(this.moveHit.direction, this.moveHit.normal);
            Gizmos.DrawRay(this.moveHit.position, normal);
        }

        if (this.groundHit.gameObject) {
            var go = this.groundHit.gameObject;
            var t = go.transform;
            var filter = go.GetComponent<MeshFilter>();

            if (filter) {
                Gizmos.color = new Color(1, 0, 0, 0.5f);
                Gizmos.DrawMesh(filter.sharedMesh, 0, t.position, t.rotation, t.lossyScale);
            }
        }

        Gizmos.color = Color.white;
        Gizmos.DrawLine(this.Position, this.Position + this.velocity);
    }

    private void DrawCapsuleGizmos(Vector3 position, Color color) {
        var radius = this.radius * Vector3.up;
        var height = this.height * Vector3.up;

        Gizmos.color = color;
        Gizmos.DrawSphere(position, 0.1f);

        Gizmos.DrawWireSphere(position + radius, this.radius);
        Gizmos.DrawWireSphere(position + height + radius, this.radius);
    }

    private void CheckGround() {
        this.groundHit = this.CapsuleCast(this.Position, Vector3.down, 100);
    }

    private CastHit Simulate(Vector3 position, Vector3 direction, float distance, Vector3 normal, ref int count) {
        var planeDir = Vector3.ProjectOnPlane(direction, normal);
        var hit = this.CapsuleCast(position, planeDir, distance);
        count++;

        if (hit.collided && distance - hit.distance > 0.1f && this.IsLegalSlope(hit.normal)) {
            return this.Simulate(hit.position, direction, distance - hit.distance, hit.normal, ref count);
        }
        
        return hit;
    }

    private bool IsLegalSlope(Vector3 normal) {
        float angle = Vector3.Angle(Vector3.up, normal);

        return angle <= this.slopeLimit;
    }

    private CastHit CapsuleCast(Vector3 position, Vector3 direction, float distance) {
        var shift = new Vector3(this.radius, this.height, this.radius);
        shift = shift.Mul(direction);
        
        var height = Vector3.up * this.height;
        var radius = Vector3.up * this.radius;

        var p1 = position + radius;
        var p2 = position + radius + height;
        RaycastHit hit;
        bool collided = Physics.CapsuleCast(p1, p2, this.radius, direction, out hit, distance);
        
        Vector3 normal = Vector3.zero;
        GameObject gameObject = null;

        if (collided) {
            distance = hit.distance;
            normal = hit.normal;
            gameObject = hit.collider.gameObject;
        }

        return new CastHit() {
            collided = collided,
            position = position + direction * distance,
            normal = normal,
            direction = direction,
            distance = distance,
            gameObject = gameObject
        };
    }
}