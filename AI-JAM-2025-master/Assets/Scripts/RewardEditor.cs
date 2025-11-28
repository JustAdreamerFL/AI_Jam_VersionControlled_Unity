using UnityEngine;

public class RewardEditor : MonoBehaviour
{
    [Header("Reward Factors")]
    [SerializeField, Range(0f, 1f)] public float movementRF = 0.8f;
    [SerializeField, Range(0f, 1f)] public float facingRF = 0.5f;
    [SerializeField, Range(0f, 1f)] public float armAngleRF = 0.5f;
    [SerializeField, Range(0f, 1f)] public float enemyDistanceRF = 0.2f;
    [SerializeField, Range(0f, 1f)] public float arenaPositionRF = 0.2f;
    [SerializeField, Range(0f, 1f)] public float collisionCombinationRF = 0.7f;
}
