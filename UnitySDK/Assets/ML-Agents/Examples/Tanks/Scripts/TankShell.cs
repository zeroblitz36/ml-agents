using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankShell : MonoBehaviour
{
    public LayerMask m_TankAgentMask;
    public float m_ExplosionForce = 1000f;
    public float m_MaxLifeTime = 2f;
    public float m_ExplosionRadius = 5f;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, m_MaxLifeTime);
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
