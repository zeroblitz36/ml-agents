using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAgents;
using System;

public class TankAgentScript : Agent
{
    public int m_TankId = 0;
    public float m_Speed = 12f;
    public float m_TurnSpeed = 180f;
    public float m_StartingHealth = 100f;
    private float m_CurrentHealth;
    public Slider m_Slider;
    public Image m_FillImage;
    public Color m_FullHealthColor = Color.green;
    public Color m_ZeroHealthColor = Color.red;
    public GameObject m_ExplosionPrefab;
    private ParticleSystem m_ExplosionParticles;

    public Vector3 m_SpawnPoint;

    private Rigidbody rBody;
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    public override void InitializeAgent()
    {
        rBody = GetComponent<Rigidbody>();
        m_ExplosionParticles = Instantiate(m_ExplosionPrefab.GetComponent<ParticleSystem>());
        m_ExplosionParticles.gameObject.SetActive(false);
    }

    public override void AgentReset()
    {
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        this.transform.position = m_SpawnPoint;

        m_CurrentHealth = m_StartingHealth;

        SetHealthUI();
    }

    private void SetHealthUI()
    {
        // Set the slider's value appropriately.
        m_Slider.value = m_CurrentHealth;

        // Interpolate the color of the bar between the choosen colours based on the current percentage of the starting health.
        m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
    }

    public void TakeDamage(float amount)
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

        AddReward((m_CurrentHealth - previousHealth) / m_StartingHealth);

        SetHealthUI();
        if(previousHealth > 0 && m_CurrentHealth <= 0)
        {
            OnDeath();
        }
    }

    private void OnDeath()
    {
        m_ExplosionParticles.transform.position = transform.position;
        m_ExplosionParticles.gameObject.SetActive(true);
        
        m_ExplosionParticles.Play();
        
        Done();
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
