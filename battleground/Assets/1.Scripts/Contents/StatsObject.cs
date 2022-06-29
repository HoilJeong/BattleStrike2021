using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Stats", menuName = "Stats System/New Chracter Stats New")]
public class StatsObject : ScriptableObject
{
    public Attribute[] attributes;

    public int Health
    {
        get;set;
    }

    public float HealthPercentage
    {
        get
        {
            int health = Health;
            int maxHealth = Health;

            foreach (Attribute attribute in attributes)
            {
                if (attribute.type == AttributeType.Health)
                {
                    maxHealth = attribute.value.ModifiedValue;
                }
            }

            return (maxHealth > 0 ? ((float)health / (float)maxHealth) : 0f);
        }
    }

    public event Action<StatsObject> OnChangedStats;

    [NonSerialized]
    private bool isInitialize = false;

    public void OnEnable()
    {
        InitializeAttribute();
    }

    public void InitializeAttribute()
    {
        if (isInitialize)
        {
            return;
        }

        isInitialize = true;
        Debug.Log("InitializeAttributes");

        foreach (Attribute attribute in attributes)
        {
            attribute.value = new ModifiableInt(OnModifiedValue);
        }

        SetBaseValue(AttributeType.Health, 100);

        Health = GetModifiedValue(AttributeType.Health);
    }

    private void OnModifiedValue(ModifiableInt value)
    {
        OnChangedStats?.Invoke(this);
    }

    public int GetBaseValue(AttributeType type)
    {
        foreach (Attribute attribute in attributes)
        {
            if (attribute.type == type)
            {
                return attribute.value.BaseValue;
            }
        }

        return -1;
    }

    public int GetModifiedValue(AttributeType type)
    {
        foreach (Attribute attribute in attributes)
        {
            if (attribute.type == type)
            {
                return attribute.value.ModifiedValue;
            }
        }

        return -1;
    }

    public int SetBaseValue(AttributeType type, int value)
    {
        foreach (Attribute attribute in attributes)
        {
            if (attribute.type == type)
            {
                attribute.value.BaseValue = value;
            }
        }

        return -1;
    }

    public int AddHealth(int value)
    {
        Health += value;
        if (Health >= 100)
        {
            Health = 100;
        }
        OnChangedStats?.Invoke(this);

        return Health;
    }
}
