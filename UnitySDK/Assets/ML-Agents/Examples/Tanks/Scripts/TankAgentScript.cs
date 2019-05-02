using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAgents;
using System;
using UnityEngine.AI;

public class TankAgentScript : Agent
{
    public int m_TankId = 0;
    public float m_Speed = 12f;
    public float m_TurnSpeed = 180f;
    public float m_StartingHealth = 100f;
    public float m_CurrentHealth;
    public Slider m_Slider;
    public Image m_FillImage;
    public Color m_FullHealthColor = Color.green;
    public Color m_ZeroHealthColor = Color.red;
    public GameObject m_ExplosionPrefab;
    public Camera cam;
    private NavMeshAgent navMeshAgent;
    private Vector3 navDestination;
    public GameObject directionSphere;
    private bool isDestinationSet = false;

    public Vector3 m_SpawnPoint;

    public TankAgentScript[] enemyTankAgents;

    private Rigidbody rBody;
    private RayPerception rayPer = null;
    public TankAcademy tankAcademy;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        rBody = GetComponent<Rigidbody>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;
        rayPer = GetComponent<RayPerception>();
    }

    public override void AgentReset()
    {
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        this.transform.position = m_SpawnPoint;

        m_CurrentHealth = m_StartingHealth;

        SetHealthUI();

        if (tankAcademy)
        {
            float signedAngle = tankAcademy.GetSignedAngleBetweenTankAndGoalSphere(transform);
            rBody.rotation *= Quaternion.Euler(0, -signedAngle, 0);
        }
        
    }

    private void SetHealthUI()
    {
        // Set the slider's value appropriately.
        m_Slider.value = m_CurrentHealth;

        // Interpolate the color of the bar between the choosen colours based on the current percentage of the starting health.
        m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
        Monitor.Log("health", m_CurrentHealth/m_StartingHealth, transform);
    }

    public void TakeDamage(float amount, int bulletTankId)
    {
        if(amount < 0)
        {
            Debug.LogError("TakeDamage must not receive a negative value");
            return;
        }
        if(IsDead())
        {
            //nothing to do , the tank is already dead
            return;
        }
        float previousHealth = m_CurrentHealth;
        m_CurrentHealth -= amount;
        m_CurrentHealth = Mathf.Max(0, m_CurrentHealth);
        
        SetHealthUI();
        if(previousHealth > 0 && m_CurrentHealth <= 0)
        {
            OnDeath();
        }
    }

    public void HitPoint()
    {
        AddReward(2);
        Done();
    }

    private void OnDeath()
    {
        //Done();
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

        AddVectorObs(transform.InverseTransformDirection(rBody.velocity));
   
        AddVectorObs(tankAcademy.GetRelativeGoalSphereObservations(transform));

        int maxNumRays = 18;
        float[] rayAngles = new float[maxNumRays];
        for(int i = 0; i < maxNumRays; i++)
        {
            rayAngles[i] = i * (360 / maxNumRays);
        }
        List<float> rayList = rayPer.Perceive(100, rayAngles, new string[] { "goal" }, 1.5f, 1.5f);
        AddVectorObs(rayList);
    }

    private float m_TimeOfLastShot = 0;
    public float m_TimeBetweenShots = 0.5f;
    public Rigidbody m_Shell;
    public Transform m_FireTransform;
    public float m_ShellSpeed = 20;
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        if (IsDead())
        {
            return;
        }

        float forwardMovement = vectorAction[0];
        float turnValue = vectorAction[1];
        bool tryingToShoot = vectorAction[2] > 0.5f;
        if(forwardMovement < 0)
        {
            turnValue *= -1;
        }
        forwardMovement = Mathf.Clamp(forwardMovement, -1, 1);
        turnValue = Mathf.Clamp(turnValue, -1, 1);

        rBody.AddForce(transform.forward * forwardMovement * m_Speed, ForceMode.VelocityChange);
        float turn = turnValue * m_TurnSpeed * Time.deltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rBody.MoveRotation(rBody.rotation * turnRotation);

        if(tryingToShoot && Time.time > m_TimeOfLastShot + m_TimeBetweenShots)
        {
            m_TimeOfLastShot = Time.time;
            TankShell.FireShell(m_Shell, 
                m_FireTransform.position, 
                m_FireTransform.rotation,
                m_ShellSpeed * m_FireTransform.forward,
                m_TankId);
        }

        AddReward(-1f / agentParameters.maxStep);

        if(Math.Abs(transform.position.x) > 35 ||
            Math.Abs(transform.position.z) > 35)
        {
            SetReward(-1);
            Done();
        }
    }
}
