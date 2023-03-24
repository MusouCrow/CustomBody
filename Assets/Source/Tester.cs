using UnityEngine;

public class Tester : MonoBehaviour {
    public float radius = 1;
    public float height = 1.5f;
    public Vector3 velocity;

    private Vector3 target;
    private Vector3 normal;
    private GameObject aim;

    protected void Awake() {
        this.aim = new GameObject("Aim");
        var cc = this.aim.AddComponent<CharacterController>();
        cc.radius = radius;
        cc.height = height;
        cc.enabled = false;
    }

    protected void Update() {
        RaycastHit hit;

        var direction = this.velocity.normalized;
        var distance = this.velocity.magnitude;
        
        var row = new Vector3(this.radius, this.height * 0.5f, this.radius);
        row = row.Mul(direction);

        var position = this.transform.position - row;
        var height = Vector3.up * this.height;
        var p1 = position + height * 0.25f;
        var p2 = position - height * 0.25f;

        bool ok = Physics.CapsuleCast(p1, p2, this.radius, direction, out hit, distance);

        if (ok) {
            this.target = position + direction * hit.distance;
            this.normal = hit.normal;
        }
        else {
            this.target = position + direction * distance;
            this.normal = Vector3.zero;
        }

        this.aim.transform.position = this.target;
    }

    protected void OnDrawGizmos() {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(this.transform.position, this.target);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(this.target, this.normal);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(this.target, 0.1f);
    }
}