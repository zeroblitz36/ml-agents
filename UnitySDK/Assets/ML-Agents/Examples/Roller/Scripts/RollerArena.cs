using UnityEngine;

public class RollerArena : MonoBehaviour
{
    public GameObject rollerAgentPrefab;
    public GameObject target1Prefab;
    public GameObject target2Prefab;
    public GameObject target3Prefab;

    private GameObject rollerAgentObject;
    private GameObject target1Object;
    private GameObject target2Object;
    private GameObject target3Object;

    private RollerAgent rollerAgent;
    private void Start()
    {
        rollerAgentObject = Instantiate(rollerAgentPrefab,
            new Vector3(0, transform.position.y + 0.5f, 0),
            Quaternion.identity);
        rollerAgent = rollerAgentObject.GetComponent<RollerAgent>();
        rollerAgent.tankArena = this;

        target1Object = Instantiate(target1Prefab);
        target2Object = Instantiate(target2Prefab);
        target3Object = Instantiate(target3Prefab);
        rollerAgent.target1 = target1Object;
        rollerAgent.target2 = target2Object;
        rollerAgent.target3 = target3Object;
    }
}
