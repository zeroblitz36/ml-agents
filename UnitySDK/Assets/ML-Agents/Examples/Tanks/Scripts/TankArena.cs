using UnityEngine;

public class TankArena : MonoBehaviour
{
    public GameObject m_PointSpherePrefab;
    public Material m_RedMaterial;

    private GameObject currentPointSphere = null;

    public GameObject m_TankPrefab;
    private GameObject tank;
    private TankArenaAgent tankArenaAgent;

    private void Start()
    {
        tank = Instantiate(m_TankPrefab);
        tankArenaAgent = tank.GetComponent<TankArenaAgent>();
        tankArenaAgent.tankArena = this;

        currentPointSphere = Instantiate(m_PointSpherePrefab);
    }

    public GameObject GetCurrentPointSphere()
    {
        return currentPointSphere;
    }
}
