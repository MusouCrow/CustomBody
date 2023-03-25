using UnityEngine;

public class Tester : MonoBehaviour {
    public struct CastHit {
        public bool collided;
        public Vector3 point;
        public Vector3 normal;
        public Vector3 direction;
        public float distance;
        public GameObject gameObject;
    }

    public float radius = 1;
    public float height = 1.5f;
    public Vector3 velocity;

    private CastHit groundHit;
    private CastHit moveHit;

    public bool InGround {
        get {
            return this.groundHit.distance <= 0.1f;
        }
    }

    protected void Update() {        
        this.CheckGround();

        var direction = this.velocity.normalized;
        var distance = this.velocity.magnitude;

        if (this.velocity.y.Equal(0) && this.InGround) {
            this.moveHit = this.SimulateLand(this.transform.position, direction, distance);
        }
        else {
            this.moveHit = this.SimulateFree(this.transform.position, direction, distance);
        }
    }

    protected void OnDrawGizmos() {
        if (!Application.isPlaying) {
            return;
        }

        this.DrawCapsuleGizmos(this.transform.position, Color.yellow);
        this.DrawCapsuleGizmos(this.moveHit.point, Color.green);

        if (!this.InGround) {
            this.DrawCapsuleGizmos(this.groundHit.point, Color.black);
        }
        else {
            Gizmos.color = Color.red;
            var normal = Vector3.ProjectOnPlane(this.transform.forward, this.groundHit.normal);
            Gizmos.DrawRay(this.groundHit.point, normal);
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
        Gizmos.DrawLine(this.transform.position, this.moveHit.point);
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
        this.groundHit = this.CapsuleCast(this.transform.position, Vector3.down, 100);
    }

    private CastHit SimulateFree(Vector3 position, Vector3 direction, float distance) {
        var hit = this.CapsuleCast(position, direction, distance);
        
        return hit;
    }

    private CastHit SimulateLand(Vector3 position, Vector3 direction, float distance) {
        var hit = this.CapsuleCast(position, direction, distance);
        
        return hit;
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

        Vector3 point = position + direction * distance;

        return new CastHit() {
            collided = collided,
            point = point,
            normal = normal,
            direction = direction,
            distance = distance,
            gameObject = gameObject
        };
    }
}