using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankShell : MonoBehaviour
{
    public LayerMask m_TankAgentMask;
    public float m_ExplosionForce = 1000f;
    public float m_MaxLifeTime = 2f;
    public float m_ExplosionRadius = 5f;

    private static List<TankShell> s_TankShellList = new List<TankShell>();
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, m_MaxLifeTime);
        s_TankShellList.Add(this);
    }

    public static void DestroyAllShells()
    {
        foreach (TankShell shell in s_TankShellList)
        {
            Destroy(shell.gameObject);
        }
        s_TankShellList.Clear();
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
            float damage = 50;//CalculateDamage(targetRigidbody.position)

            tankAgent.TakeDamage(damage);
        }

        Destroy(gameObject);
        s_TankShellList.Remove(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
