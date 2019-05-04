using UnityEngine;

public class TankArena : MonoBehaviour
{
    public GameObject rollerAgentPrefab;
    public GameObject targetPrefab;

    private GameObject rollerAgentObject;
    private GameObject targetObject;

    private RollerAgent rollerAgent;
    private void Start()
    {
        rollerAgentObject = Instantiate(rollerAgentPrefab,
            new Vector3(0, transform.position.y + 0.5f, 0),
            Quaternion.identity);
        rollerAgent = rollerAgentObject.GetComponent<RollerAgent>();
        rollerAgent.tankArena = this;

        targetObject = Instantiate(targetPrefab,
            new Vector3(
                Random.Range(-4, 4),
                transform.position.y + 0.5f,
                Random.Range(-4, 4)
            ),
            Quaternion.identity);
        rollerAgent.target = targetObject;
    }
}
