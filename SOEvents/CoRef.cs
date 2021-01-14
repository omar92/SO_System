﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoRef : MonoBehaviour
{
    public static CoRef instance;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this);
        }
    }

    public static void CreateCorotineReferance()
    {
        if (!instance)
        {
            GameObject gameObject = new GameObject("GameEventCorotuineStarter");
            gameObject.AddComponent<CoRef>();
        }
    }

}