using UnityEngine;

public class Trigger : MonoBehaviour {
    public enum EventType {DropKill}
    
    public EventType eventType;

    protected void OnTriggerEnter(Collider other) {
        if (this.eventType == EventType.DropKill) {
            var entity = other.GetComponent<Entity>();

            if (entity) {
                // entity.Body.SetPosition(entity.RebornPosition);
            }
        }
    }
}