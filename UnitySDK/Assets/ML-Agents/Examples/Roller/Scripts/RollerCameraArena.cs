using UnityEngine;

public class RollerCameraArena : MonoBehaviour
{
    public GameObject rollerAgentObject = null;
    private RollerAgent rollerAgent;
    public GameObject target1Object = null;
    public GameObject target2Object = null;
    public GameObject target3Object = null;
    // Start is called before the first frame update
    void Start()
    {
        rollerAgent = rollerAgentObject.GetComponent<RollerAgent>();
        rollerAgent.arenaPosition = transform.position;
        rollerAgent.target1 = target1Object;
        rollerAgent.target2 = target2Object;
        rollerAgent.target3 = target3Object;
    }
}
