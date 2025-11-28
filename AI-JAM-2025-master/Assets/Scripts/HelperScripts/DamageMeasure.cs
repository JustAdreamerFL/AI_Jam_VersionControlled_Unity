using UnityEngine;

// Dočasný skript na meranie sily nárazu
public class DamageMeasure : MonoBehaviour
{
    BoxCollider boxCollider;

    // TODO: vymazat tento skript po dokončení projektu

    private void Start() {
        boxCollider = gameObject.GetComponent<BoxCollider>();
    }

    void Update()
    {
        
    }

    // Hodnota impulzu/sily nárazu bez iných úprav
    private void OnCollisionEnter(Collision collision) {
        float collisionForce = collision.impulse.magnitude / Time.fixedDeltaTime;// / 100000f;
        //Debug.Log("Collision force: " + collisionForce);
    }
}
