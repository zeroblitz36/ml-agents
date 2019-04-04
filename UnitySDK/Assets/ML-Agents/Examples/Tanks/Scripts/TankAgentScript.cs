using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAgents;

public class TankAgentScript : Agent
{
    public int m_TankNumber = 1;
    public float m_Speed = 12f;
    public float m_TurnSpeed = 180f;
    public float m_StartingHealth = 100f;
    private float m_CurrentHealth;
    public Slider m_Slider;
    public Image m_FillImage;
    public Color m_FullHealthColor = Color.green;
    public Color m_ZeroHealthColor = Color.red;
    public GameObject m_ExplosionPrefab;

    public Vector3 m_SpawnPoint;

    private Rigidbody rBody;
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    public override void AgentReset()
    {
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        this.transform.position = m_SpawnPoint;

        m_CurrentHealth = m_StartingHealth;
    }

    protected bool isDead() {
        return m_CurrentHealth <= 0;
    }

    public override void CollectObservations()
    {
        //Agent position and rotation
        AddVectorObs(this.transform.position.x);
        //The Y position can be ignored, the Tank never flies
        AddVectorObs(this.transform.position.z);
        //The Tank can only rotate around the y axis
        AddVectorObs(this.transform.rotation.y);

        //How much health it has
        if(m_CurrentHealth > 0)
        {
            AddVectorObs(m_CurrentHealth / m_StartingHealth);
        }
        else
        {
            AddVectorObs(0);
        }
    }
}
