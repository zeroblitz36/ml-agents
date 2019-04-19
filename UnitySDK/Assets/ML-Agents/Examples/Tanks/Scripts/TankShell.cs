using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankShell : MonoBehaviour
{
    public LayerMask m_TankAgentMask;
    public float m_ExplosionForce = 1000f;
    public float m_MaxLifeTime = 2f;
    private float creationTime;
    public float m_ExplosionRadius = 5f;

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

    public static void FireShell(Rigidbody m_ShellPrefab, Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        if(s_DisabledShellList.Count > 0)
        {
            TankShell s = s_DisabledShellList.Pop();
            s.transform.position = position;
            s.transform.rotation = rotation;
            s.GetComponent<Rigidbody>().velocity = velocity;
            s.gameObject.SetActive(true);
        }
        else
        {
            Rigidbody shellInstance = Instantiate(m_ShellPrefab, position, rotation) as Rigidbody;
            shellInstance.velocity = velocity;
        }

    }

    public static void DisableAllShells()
    {
        /*
        foreach (TankShell shell in s_EnabledShellList)
        {
            if(shell != null)
            {
                Destroy(shell.gameObject);
            }
        }
        */
        while(s_EnabledShellList.Count > 0)
        {
            s_EnabledShellList[0].gameObject.SetActive(false);
        }
        //s_DisabledShellList.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_TankAgentMask);
        for(int i = 0; i < colliders.Length; i++)
        {
            Rigidbody targetRigidBody = colliders[i].GetComponent<Rigidbody>();
            if (!targetRigidBody)
                continue;
            targetRigidBody.AddExplosionForce(m_ExplosionForce, transform.position, m_ExplosionRadius);
            TankAgentScript tankAgent = targetRigidBody.GetComponent<TankAgentScript>();
            if (!tankAgent)
                continue;
            float damage = 10;//CalculateDamage(targetRigidbody.position)

            tankAgent.TakeDamage(damage);
        }

        //Destroy(gameObject);
        //s_DisabledShellList.Remove(this);
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
