using System.Collections.Generic;
using UnityEngine;

public class TankShell : MonoBehaviour
{
    public LayerMask m_TankAgentMask;
    public float m_ExplosionForce = 1000f;
    public float m_MaxLifeTime = 2f;
    private float creationTime;
    public float m_ExplosionRadius = 5f;
    public int m_TankIdOwner;

    private static Stack<TankShell> s_DisabledShellList = new Stack<TankShell>();
    private static List<TankShell> s_EnabledShellList = new List<TankShell>(); 

    private void OnEnable()
    {
        creationTime = Time.time;
        s_EnabledShellList.Add(this);
    }

    private void OnDisable()
    {
        s_EnabledShellList.Remove(this);
        s_DisabledShellList.Push(this);
    }

    public static void FireShell(Rigidbody m_ShellPrefab, Vector3 position, Quaternion rotation, Vector3 velocity, int tankId)
    {
        if(s_DisabledShellList.Count > 0)
        {
            TankShell s = s_DisabledShellList.Pop();
            s.transform.position = position;
            s.transform.rotation = rotation;
            s.GetComponent<Rigidbody>().velocity = velocity;
            s.gameObject.SetActive(true);
            s.m_TankIdOwner = tankId;
        }
        else
        {
            Rigidbody shellInstance = Instantiate(m_ShellPrefab, position, rotation) as Rigidbody;
            shellInstance.velocity = velocity;
            shellInstance.GetComponent<TankShell>().m_TankIdOwner = tankId;
        }
    }

    public static void DisableAllShells()
    {
        while(s_EnabledShellList.Count > 0)
        {
            s_EnabledShellList[0].gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "goal")
        {
            TankAcademy academy = GameObject.Find("Academy").GetComponent<TankAcademy>();
            academy.EventGoalSphereHitByBullet(m_TankIdOwner);
        }
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time-creationTime > m_MaxLifeTime)
        {
            gameObject.SetActive(false);
        }
    }
}
