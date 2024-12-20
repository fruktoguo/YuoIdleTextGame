using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YuoTools.UI
{
    public partial class SpawnUICodeConfig
    {
        public const string UIComponentTag = "C_";
        public const string GeneralUITag = "G_";
        public const string ChildUITag = "D_";
        public const string VariantChildUITag = "DV_";
        public const string VariantChildComponentTag = "CV_";

        public static List<string> AllTag = new List<string>()
        {
            UIComponentTag,
            GeneralUITag,
            ChildUITag,
            VariantChildUITag,
            VariantChildComponentTag,
        };

        public List<Type> NoneSpawnType = new()
        {
            typeof(CanvasRenderer),
            typeof(UISetting),
        };

        [LabelText("相斥组件")] public Dictionary<Type, Type> RemoveType = new()
        {
            { typeof(Button), typeof(Image) },
            { typeof(Dropdown), typeof(Image) },
            { typeof(InputField), typeof(Image) },
            { typeof(Toggle), typeof(Image) },
            { typeof(ToggleGroup), typeof(Image) },
            { typeof(Slider), typeof(Image) },
            { typeof(ScrollRect), typeof(Image) },
            { typeof(Scrollbar), typeof(Image) },
            { typeof(ParticleSystem), typeof(Image) },
            { typeof(TMP_InputField), typeof(Image) },
            { typeof(TMP_Dropdown), typeof(Image) }
        };

        [LabelText("System的默认命名空间")] public List<string> SystemNameSpace = new()
        {
            "using DG.Tweening;",
            "using YuoTools.Extend.Helper;",
            "using YuoTools.Main.Ecs;",
        };
    }
}