using Unity.MLAgents;
using UnityEngine;

public class MetamfetamineController : MonoBehaviour
{
    private RobotAgent kamosTy;
    private RobotAgent dealer;
    [SerializeField] private float blueCrystalValue = 1000f;


    private void Start()
    {
        kamosTy = GetComponent<RobotAgent>();
        dealer = kamosTy.enemyRobot;
        if (dealer  != null )
            return;
        dealer.OnRobotDie += Dealer_OnRobotDie;
    }

    private void Dealer_OnRobotDie(object sender, System.EventArgs e)
    {
        kamosTy.AddReward(blueCrystalValue);
        Debug.LogError("virus added");
    }

    private void OnDestroy()
    {
        if (dealer != null)
        {
            dealer.OnRobotDie -= Dealer_OnRobotDie;
        }
    }
}
