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

    public bool IsDead() {
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

    private float m_TimeOfLastShot = 0;
    public float m_TimeBetweenShots = 0.5f;
    public Rigidbody m_Shell;
    public Transform m_FireTransform;
    public float m_ShellSpeed = 20;
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        float forwardMovement = vectorAction[0];
        float turnValue = vectorAction[1];
        bool tryingToShoot = vectorAction[2] > 0.5f;
        if(forwardMovement < 0)
        {
            turnValue *= -1;
        }
        forwardMovement = Mathf.Clamp(forwardMovement, -1, 1);
        turnValue = Mathf.Clamp(turnValue, -1, 1);

        Vector3 movement = transform.forward * forwardMovement * m_Speed * Time.deltaTime;
        rBody.MovePosition(rBody.position + movement);

        float turn = turnValue * m_TurnSpeed * Time.deltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rBody.MoveRotation(rBody.rotation * turnRotation);

        if(tryingToShoot && Time.time > m_TimeOfLastShot + m_TimeBetweenShots)
        {
            m_TimeOfLastShot = Time.time;
            //BANG BANG
            Rigidbody shellInstance = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;
            shellInstance.velocity = m_ShellSpeed * m_FireTransform.forward;
        }
    }
}
