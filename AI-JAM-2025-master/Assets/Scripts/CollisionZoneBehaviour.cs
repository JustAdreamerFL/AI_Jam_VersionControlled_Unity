using System;
using Unity.VisualScripting;
using UnityEngine;

public class CollisionZoneBehaviour : MonoBehaviour {

    private float totalHealth = 1f;
    [SerializeField] public float CurrentHealth = 1f;
    public float RewardOfCollision { get; private set; }

    // Percentual value of how much damage is taken per hit based on collision force
    // [SerializeField, Range(1f, 20f)]
    [HideInInspector] public float resistanceOfZone = 1f;
    // Multiplier for damage to enemy based on collision force
    //[SerializeField, Range(1f, 20f)]
    [HideInInspector] public float damageOfZone = 1f;

    private string zoneType = "default";

    public event EventHandler<CollisionEventArgs> OnCollision;
    public class CollisionEventArgs : EventArgs {
        public float Damage { get; private set; }

        public string colliderHitName { get; private set; }

        public CollisionEventArgs(float damage, string colliderHitName) {
            this.Damage = damage;
            this.colliderHitName = colliderHitName;
        }
    }

    public event EventHandler<DamageDealtEventArgs> OnDamageDealt;
    public class DamageDealtEventArgs : EventArgs
    {
        public float DamageDealt { get; private set; }
        public DamageDealtEventArgs(float damageDealt)
        {
            DamageDealt = damageDealt;
        }
    }

    private void Start() {
        CurrentHealth = totalHealth;
    }

    private void Update() {
        RewardOfCollision = 0f;
    }
    
    internal void OnCollisionHit(Collision collision, CollisionZoneBehaviour enemyCollisionZone) {
        // normalised collision force
        float collisionForce = collision.impulse.magnitude / Time.fixedDeltaTime / 100000f;
        //Debug.Log($"Collision detected with force: {collisionForce}", this);

        float damageTaken = 0f;
        if (enemyCollisionZone == null) {
            damageTaken = 1f / (resistanceOfZone);
        }
        else {
            damageTaken = (enemyCollisionZone.damageOfZone) / (resistanceOfZone);
        }

        var damage = damageTaken * Math.Max(collisionForce, 0.0001f);

        CurrentHealth = Math.Max(CurrentHealth - damage, 0f);

        //Debug.Log($"{this.gameObject.name} -> {enemyCollisionZone?.gameObject.name}");
        if (enemyCollisionZone != null)
        {
            // Calculate damage dealt to the enemy zone, normalize it by logarithm with base 20,
            // because our damage values range from 1 to 20
            float damageDealt = damageOfZone / enemyCollisionZone.resistanceOfZone;
            float damageDealtNormalized = Mathf.Clamp(Mathf.Log(damageDealt, 20), -1, 1);

            RewardOfCollision = damageDealtNormalized;
            if (CurrentHealth > 0)
                OnDamageDealt?.Invoke(this, new DamageDealtEventArgs(damageDealt));
            //Debug.Log(gameObject.name + " (" + enemyCollisionZone.damageOfZone + ";" + resistanceOfZone + ") got reward: " + RewardOfCollision);
        } 

        OnCollision?.Invoke(this, new CollisionEventArgs(damage, collision.gameObject.tag));
    }

    public void ResetHealth() {
        CurrentHealth = totalHealth;
    }

    public void ChangeHealth(float value) {
        CurrentHealth = Math.Clamp(CurrentHealth + value, 0f, totalHealth);
    }

    internal void SetZoneType(string type) {
        zoneType = type;
    }

    public string GetZoneType() {
        return zoneType;
    }
}
