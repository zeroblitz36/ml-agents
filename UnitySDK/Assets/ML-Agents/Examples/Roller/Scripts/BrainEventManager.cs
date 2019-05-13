using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BrainEventManager
{

    public class BrainEvent
    {
        public float timeWhenItTriggered;
        public string eventName;

        public BrainEvent(float timeWhenItTriggered, string eventName)
        {
            this.timeWhenItTriggered = timeWhenItTriggered;
            this.eventName = eventName;
        }

        public override string ToString()
        {
            return timeWhenItTriggered + " " + eventName;
        }
    }
    public class BrainEventChain
    {
        private bool isGood;
        public List<BrainEvent> brainEventList = new List<BrainEvent>();
        public float startTime;

        public BrainEventChain(float startTime)
        {
            this.startTime = startTime;
        }

        public bool IsGood { get => isGood; set => isGood = value; }

        public bool isEmpty()
        {
            return brainEventList.Count == 0;
        }

        public void AddEvent(BrainEvent e)
        {
            e.timeWhenItTriggered -= startTime;
            brainEventList.Add(e);
        }

        public override string ToString()
        {
            string s = "";
            s += "BrainEventChain ";
            s += (isGood ? "SUCCESS" : "FAILED") + " ";
            s += "startTime = " + startTime + "\n";
            foreach(BrainEvent b in brainEventList)
            {
                s += '\t' + b.ToString() + '\n';
            }
            return s;
        }
    }

    private BrainEventChain currentWorkingBrainEventChain = null;
    private List<BrainEventChain> collectionOfAllBrainEventChains = new List<BrainEventChain>();
    public string rootFileName;
    private string completeFileName;

    public BrainEventManager(string rootFileName)
    {
        this.rootFileName = rootFileName;
    }

    public void StartRecordingNewEvents()
    {
        if(currentWorkingBrainEventChain != null)
        {
            Debug.LogError("The last event chain was not properly finished !");
        }
        currentWorkingBrainEventChain = new BrainEventChain(Time.time);
        currentWorkingBrainEventChain.AddEvent(new BrainEvent(Time.time, "START"));
    }

    public void RecordEvent(string eventName)
    {
        currentWorkingBrainEventChain.AddEvent(new BrainEvent(Time.time, eventName));
    }

    public void RecordGoalSucces()
    {
        if (currentWorkingBrainEventChain == null)
        {
            Debug.LogError("No new event chain was created !");
            return;
        }
        currentWorkingBrainEventChain.AddEvent(new BrainEvent(Time.time, "SUCCESS"));
        currentWorkingBrainEventChain.IsGood = true;
        collectionOfAllBrainEventChains.Add(currentWorkingBrainEventChain);
        WriteToFile(currentWorkingBrainEventChain);
        currentWorkingBrainEventChain = null;
    }

    public void RecordGoalFailed()
    {
        if (currentWorkingBrainEventChain == null)
        {
            Debug.LogError("No new event chain was created !");
            return;
        }
        currentWorkingBrainEventChain.AddEvent(new BrainEvent(Time.time, "FAILED"));
        currentWorkingBrainEventChain.IsGood = false;
        collectionOfAllBrainEventChains.Add(currentWorkingBrainEventChain);
        WriteToFile(currentWorkingBrainEventChain);
        currentWorkingBrainEventChain = null;
    }

    private void WriteToFile(BrainEventChain eventChain)
    {
        if(rootFileName == null)
        {
            Debug.LogError("No rootFileName was defined!");
            rootFileName = "DefaultBrainEventChainFile";
        }

        if(completeFileName == null)
        {
            completeFileName = rootFileName + "_" +
                DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + 
                ".txt";
        }

        using (StreamWriter sw = File.AppendText(completeFileName))
        {
            sw.Write(eventChain.ToString());
            sw.Flush();
        }

    }
}
