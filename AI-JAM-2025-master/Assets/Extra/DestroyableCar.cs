using UnityEngine;

public class DestroyableCar : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        RobotAgent robot = other.gameObject.GetComponentInParent<RobotAgent>();
        if (robot == null)
            return;
        else
        {
            Destroy(this.gameObject);
        }
    }
}
