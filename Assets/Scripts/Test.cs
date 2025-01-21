using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YuoTools.Extend.AI;
using YuoTools.UI;

public class Test : SerializedMonoBehaviour
{
    public GameObject role1;
    public GameObject role2;

    private void Start()
    {
        View_DistanceInfoComponent.GetView().ShowDistance(role1, role2);
    }
}