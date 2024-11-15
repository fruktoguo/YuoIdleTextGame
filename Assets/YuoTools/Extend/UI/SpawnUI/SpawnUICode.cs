using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using YuoTools.Extend.Helper;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace YuoTools.UI
{
    public static class SpawnUICode
    {
        public const string DefaultBasePath = "YuoUI";
        public static string BasePath = DefaultBasePath;

        public static void SpawnCode(GameObject gameObject)
        {
            if (null == gameObject) return;
            string UIName = gameObject.name;
            string strDlgName = $"View_{UIName}Component";

            string strFilePath = $"{Application.dataPath}/{BasePath}/View";

            if (!Directory.Exists(strFilePath))
            {
                Directory.CreateDirectory(strFilePath);
            }

            strFilePath = strFilePath + "/" + strDlgName + ".cs";

            StreamWriter sw = new StreamWriter(strFilePath, false, Encoding.UTF8);

            StringBuilder strBuilder = new StringBuilder();

            Dictionary<string, string> allTypes = new();

            List<Type> allOriginalTypes = new();

            strBuilder.AppendLine(CodeSpawn_AddAllComponent(SpawnUICodeConfig.UIComponentTag, allTypes,
                allOriginalTypes,
                gameObject.transform, true));

            strBuilder.AppendLine(CodeSpawn_FindAll(allTypes));

            strBuilder.AppendLine("\t\t}");

            strBuilder.AppendLine("\t}\r}");

            //引入命名空间
            StringBuilder final = new StringBuilder();

            final.Append(CodeSpawn_AddNameSpace(strDlgName, allTypes, allOriginalTypes));

            final.AppendLine("\tpublic static partial class ViewType");
            final.AppendLine("\t{");
            final.AppendLine($"\t\tpublic const string {UIName} = \"{UIName}\";");
            final.AppendLine("\t}");
            final.AppendLine();

            final.AppendLine($"\tpublic partial class {strDlgName} : UIComponent \n\t{{");

            final.AppendLine("");
            final.AppendLine(
                $"\t\tpublic static {strDlgName} GetView() => UIManagerComponent.Get.GetUIView<{strDlgName}>();");
            final.AppendLine("");

            final.Append(strBuilder);

            sw.Write(final.ToString());

            sw.Close();

            SpawnSystemCode(UIName);

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        public static void SpawnGeneralUICode(GameObject gameObject)
        {
            SpawnUICodeBase(gameObject, SpawnUICodeConfig.GeneralUITag, "General", UIType.General);
        }

        private static void SpawnChildUICode(GameObject gameObject)
        {
            SpawnUICodeBase(gameObject, SpawnUICodeConfig.ChildUITag, "Child", UIType.Child, findThis: true);
        }

        private static void SpawnVariantChildUICode(GameObject gameObject)
        {
            SpawnUICodeBase(gameObject, SpawnUICodeConfig.VariantChildUITag, "Child", UIType.VariantChild);
        }

        private static void SpawnUICodeBase(GameObject gameObject, string tag, string outputPath, UIType uiType,
            bool findThis = false)
        {
            if (null == gameObject) return;

            string uiName = gameObject.name.Replace(tag, "");
            if (uiName.Contains("_") && uiType == UIType.Child)
            {
                uiName = uiName.Split('_')[0];
            }

            string strDlgName = $"View_{uiName}{(uiType == UIType.VariantChild ? "Variant" : "")}Component";

            string strFilePath = $"{Application.dataPath}/{BasePath}/View/{outputPath}";

            if (!Directory.Exists(strFilePath))
            {
                Directory.CreateDirectory(strFilePath);
            }

            strFilePath = strFilePath + "/" + strDlgName + ".cs";

            using (StreamWriter sw = new StreamWriter(strFilePath, false, Encoding.UTF8))
            {
                StringBuilder strBuilder = new StringBuilder();
                Dictionary<string, string> allTypes = new();
                List<Type> allOriginalTypes = new();

                strBuilder.AppendLine(CodeSpawn_AddAllComponent(
                    uiType == UIType.VariantChild
                        ? SpawnUICodeConfig.VariantChildComponentTag
                        : SpawnUICodeConfig.UIComponentTag,
                    allTypes,
                    allOriginalTypes,
                    gameObject.transform,
                    findThis: findThis
                ));

                strBuilder.AppendLine(CodeSpawn_FindAll(allTypes));
                strBuilder.AppendLine("\t\t}");
                strBuilder.AppendLine("\t}\r}");

                StringBuilder final = new StringBuilder();
                final.AppendLine(CodeSpawn_AddNameSpace(strDlgName, allTypes, allOriginalTypes));
                final.AppendLine($"\tpublic partial class {strDlgName} : UIComponent \n\t{{");
                final.Append(strBuilder);

                sw.Write(final.ToString());
            }

            SpawnSystemCode(uiName + (uiType == UIType.VariantChild ? "Variant" : ""), uiType);
        }

        public static string CodeSpawn_AddNameSpace(string strDlgName, Dictionary<string, string> allTypes,
            List<Type> allOriginalTypes)
        {
            StringBuilder strBuilder = new StringBuilder();

            List<string> allNameSpace = new();
            if (allOriginalTypes != null)
            {
                foreach (var item in allOriginalTypes)
                {
                    var nameSpace = item.Namespace;
                    if (!allNameSpace.Contains(nameSpace))
                        allNameSpace.Add(nameSpace);
                }
            }


            strBuilder.AppendLine("using System.Collections.Generic;\nusing Sirenix.OdinInspector;");

            foreach (var item in allNameSpace)
            {
                if (item.IsNullOrSpace()) continue;
                strBuilder.AppendLine($"using {item};");
            }


            // foreach (var space in SpawnConfig.ComponentNameSpace)
            // {
            //     strBuilder.AppendLine(space);
            // }
            //
            // foreach (var type in allTypes.Keys)
            // {
            //     foreach (var item in SpawnConfig.ComponentAddNameSpace)
            //     {
            //         if (item.Key.Name == type)
            //             strBuilder.AppendLine(item.Value);
            //     }
            // }

            strBuilder.AppendLine();
            //设置命名空间
            strBuilder.AppendLine("namespace YuoTools.UI\n{");

            return strBuilder.ToString();
        }

        public static void CodeSpawn_AddNameSpace(Type type)
        {
            string nameSpace = type.Namespace;
            $"{type.Name} 的 命名空间为 {nameSpace}".Log();
        }

        public static string CodeSpawn_AddAllComponent(string tag, Dictionary<string, string> allTypes,
            List<Type> allOriginalTypes, Transform transform, bool findThis = false)
        {
            var strBuilder = new StringBuilder();
            var tranInfo = new TransformInfo(transform);
            FindAll(tranInfo);

            var (trans, generals, children, variantChildren) = CategorizeComponents(tranInfo, tag);

            if (findThis)
            {
                ProcessMainComponent(transform, allTypes, allOriginalTypes, strBuilder);
            }

            ProcessComponents(trans, tag, allTypes, allOriginalTypes, strBuilder, transform);
            ProcessExtraComponents(generals, children, variantChildren, allTypes, strBuilder, transform);

            return strBuilder.ToString();
        }

        private static (List<Transform>, List<Transform>, List<Transform>, List<Transform>) CategorizeComponents(
            TransformInfo info, string tag)
        {
            var trans = new List<Transform>();
            var generals = new List<Transform>();
            var children = new List<Transform>();
            var variantChildren = new List<Transform>();

            void Categorize(TransformInfo node)
            {
                foreach (var child in node.children)
                {
                    if (child.transform.name.StartsWith(SpawnUICodeConfig.GeneralUITag))
                        generals.Add(child.transform);
                    else if (child.transform.name.StartsWith(SpawnUICodeConfig.ChildUITag))
                        children.Add(child.transform);
                    else if (child.transform.name.StartsWith(SpawnUICodeConfig.VariantChildUITag))
                        variantChildren.Add(child.transform);
                    else
                    {
                        trans.Add(child.transform);
                        Categorize(child);
                    }
                }
            }

            Categorize(info);
            trans.Remove(info.transform);
            return (trans, generals, children, variantChildren);
        }

        private static void ProcessMainComponent(Transform transform, Dictionary<string, string> allTypes,
            List<Type> allOriginalTypes, StringBuilder strBuilder)
        {
            var types = GetTypes(transform);
            var oTypes = GetOriginalTypes(transform);
            foreach (var type in types)
            {
                allTypes.TryAdd(type, "");
                allTypes[type] += $"\n\t\t\tall_{type}.Add(Main{type});";
                strBuilder.AppendLine($"\n\t\tprivate {type} main{type};");
                strBuilder.AppendLine(
                    $"\n\t\tpublic {type} Main{type}\n\t\t{{\n\t\t\tget\n\t\t\t{{\n\t\t\t\tif (main{type} == null)\n\t\t\t\t\tmain{type} = rectTransform.GetComponent<{type}>();\n\t\t\t\treturn main{type};\n\t\t\t}}\n\t\t}}");
            }

            allOriginalTypes.AddRange(oTypes);
        }

        private static void ProcessComponents(List<Transform> components, string tag,
            Dictionary<string, string> allTypes, List<Type> allOriginalTypes, StringBuilder strBuilder,
            Transform transform)
        {
            foreach (var item in components)
            {
                if (item.name.StartsWith(tag))
                {
                    string name = item.name.Replace(tag, "");
                    var types = GetTypes(item);
                    var oTypes = GetOriginalTypes(item);

                    types = types.Count == 0 ? new List<string> { "RectTransform" } : types;
                    oTypes = oTypes.Count == 0 ? new List<Type> { typeof(RectTransform) } : oTypes;

                    foreach (var type in types)
                    {
                        CodeSpawn_AddFindAll(allTypes, type, name);
                        strBuilder.AppendLine(CodeSpawn_AddComponent(type, name, GetRelativePath(item, transform)));
                    }

                    allOriginalTypes.AddRange(oTypes);
                }
            }
        }

        private static void ProcessExtraComponents(List<Transform> generals, List<Transform> children,
            List<Transform> variantChildren, Dictionary<string, string> allTypes, StringBuilder strBuilder,
            Transform transform)
        {
            ProcessGeneralComponents(generals, strBuilder, transform);
            ProcessChildComponents(children, allTypes, strBuilder, transform);
            ProcessVariantChildComponents(variantChildren, allTypes, strBuilder, transform);
        }

        private static void ProcessGeneralComponents(List<Transform> generals, StringBuilder strBuilder,
            Transform transform)
        {
            foreach (var general in generals)
            {
                Debug.Log(general);
                string tag = SpawnUICodeConfig.ChildUITag;
                SpawnGeneralUICode(general.gameObject);
                string name = general.name.Replace(tag, "");
                string type = $"View_{name}Component";
                string get = $@"{{
            get
            {{
                if (mGeneral_{name} == null)
                {{
                    mGeneral_{name} = Entity.AddChild<{type}>();
                    mGeneral_{name}.rectTransform = rectTransform.Find(""{GetRelativePath(general, transform)}"") as RectTransform;
                }}
                return mGeneral_{name};
            }}
        }}";

                strBuilder.AppendLine($"\n\t\tprivate {type} mGeneral_{name};");
                strBuilder.AppendLine($"\n\t\tpublic {type} General_{name}\n\t\t{get}");
            }
        }

        private static void ProcessChildComponents(List<Transform> children, Dictionary<string, string> allTypes,
            StringBuilder strBuilder, Transform transform)
        {
            foreach (var child in children)
            {
                string tag = SpawnUICodeConfig.ChildUITag;
                SpawnChildUICode(child.gameObject);
                string name = child.name.Replace(tag, "");
                string typeName = child.name.Replace(tag, "").Split('_')[0];
                string type = $"View_{typeName}Component";
                Debug.Log($"检索到子物体 [ {child} ]  _ 类型为 [ {type} ]");
                allTypes.TryAdd(type, "");
                allTypes[type] += $"\n\t\t\tall_{type}.Add(Child_{name});";

                string get = $@"{{
            get
            {{
                if (mChild_{name} == null)
                {{
                    mChild_{name} = Entity.AddChild<{type}>();
                    mChild_{name}.Entity.EntityName = ""{name}"";
                    mChild_{name}.rectTransform = rectTransform.Find(""{GetRelativePath(child, transform)}"") as RectTransform;
                    mChild_{name}.RunSystem<IUICreate>();
                }}
                return mChild_{name};
            }}
        }}";

                strBuilder.AppendLine($"\n\t\tprivate {type} mChild_{name};");
                strBuilder.AppendLine($"\n\t\tpublic {type} Child_{name}\n\t\t{get}");
            }
        }

        private static void ProcessVariantChildComponents(List<Transform> variantChildren,
            Dictionary<string, string> allTypes, StringBuilder strBuilder, Transform transform)
        {
            foreach (var variantChild in variantChildren)
            {
                string tag = SpawnUICodeConfig.VariantChildUITag;
                SpawnChildUICode(variantChild.gameObject);
                SpawnVariantChildUICode(variantChild.gameObject);
                string name = variantChild.name.Replace(tag, "");
                string typeName = variantChild.name.Replace(tag, "").Split('_')[0];
                string typeVariantName = variantChild.name.Replace(tag, "");
                string type = $"View_{typeName}Component";
                string typeVariant = $"View_{typeVariantName}VariantComponent";
                Debug.Log($"检索到子物体变体 [ {variantChild} ]  _ 类型为 [ {type} ]");
                allTypes.TryAdd(type, "");
                allTypes[type] += $"\n\t\t\tall_{type}.Add(Child_{name});";

                string get = $@"{{
            get
            {{
                if (mChild_{name} == null)
                {{
                    mChild_{name} = Entity.AddChild<{type}>();
                    mChild_{name}.Entity.EntityName = ""{name}"";
                    mChild_{name}Variant = mChild_{name}.AddComponent<{typeVariant}>();
                    mChild_{name}.rectTransform = rectTransform.Find(""{GetRelativePath(variantChild, transform)}"") as RectTransform;
                    mChild_{name}Variant.rectTransform = mChild_{name}.rectTransform;
                    mChild_{name}.Entity.RunSystem<IUICreate>();
                }}
                return mChild_{name};
            }}
        }}";

                strBuilder.AppendLine($"\n\t\tprivate {type} mChild_{name};");
                strBuilder.AppendLine($"\n\t\tpublic {type} Child_{name}\n\t\t{get}");

                string getVariant = $@"{{
            get
            {{
                return mChild_{name}Variant;
            }}
        }}";

                strBuilder.AppendLine($"\n\t\tprivate {typeVariant} mChild_{name}Variant;");
                strBuilder.AppendLine($"\n\t\tpublic {typeVariant} Child_{name}Variant\n\t\t{getVariant}");
            }
        }

        public static string CodeSpawn_AddComponent(string type, string name, string relativePath)
        {
            StringBuilder strBuilder = new StringBuilder();
            
            string get =$@"=> m{type}_{name} ??= rectTransform.Find(""{relativePath}"").GetComponent<{type}>();";
            strBuilder.AppendLine($"\n\t\tprivate {type} m{type}_{name};");

            strBuilder.AppendLine($"\n\t\tpublic {type} {type}_{name} {get}");
            return strBuilder.ToString();
        }

        public static void CodeSpawn_AddFindAll(Dictionary<string, string> allTypes, string type, string name)
        {
            allTypes.TryAdd(type, "");
            allTypes[type] += $"\n\t\t\tall_{type}.Add({type}_{name});";
        }

        public static string CodeSpawn_FindAll(Dictionary<string, string> allTypes)
        {
            StringBuilder strBuilder = new StringBuilder();
            foreach (var item in allTypes.Keys)
            {
                strBuilder.AppendLine("\t\t[FoldoutGroup(\"ALL\")]");
                strBuilder.AppendLine($"\t\tpublic List<{item}> all_{item} = new();");
            }

            strBuilder.AppendLine("\n\t\tpublic void FindAll()\n\t\t{");
            foreach (var item in allTypes)
            {
                strBuilder.AppendLine($"\t\t\t\t{item.Value}");
            }

            return strBuilder.ToString();
        }

        public enum UIType
        {
            Window,
            Child,
            General,
            VariantChild
        }

        public static void SpawnSystemCode(string name, UIType uiType = UIType.Window)
        {
            string strFilePath = $"{Application.dataPath}/{BasePath}/System";
            if (uiType != UIType.Window) strFilePath += "/Child";
            if (!Directory.Exists(strFilePath))
            {
                Directory.CreateDirectory(strFilePath);
            }

            string strDlgName = $"View_{name}System";
            strFilePath = strFilePath + "/" + strDlgName + ".cs";

            if (File.Exists(strFilePath))
            {
                Debug.LogWarning($"{strDlgName}已存在");
                return;
            }

            StreamWriter sw = new StreamWriter(strFilePath, false, Encoding.UTF8);

            StringBuilder strBuilder = new StringBuilder();
            foreach (var space in SpawnConfig.SystemNameSpace)
            {
                strBuilder.AppendLine(space);
            }

            //设置命名空间
            strBuilder.AppendLine("namespace YuoTools.UI");
            strBuilder.AppendLine("{");
            strBuilder.AppendLine(
                $"\tpublic partial class View_{name}Component");
            strBuilder.AppendLine("\t{");
            strBuilder.AppendLine("\t}");
            switch (uiType)
            {
                case UIType.Window:
                    strBuilder.AppendLine(
                        $"\tpublic class View{name}CreateSystem :YuoSystem<View_{name}Component>, IUICreate");
                    strBuilder.AppendLine("\t{");
                    strBuilder.Append("\t\t");
                    strBuilder.Append(@$"public override string Group =>""UI/{name}"";");
                    strBuilder.AppendLine("");
                    strBuilder.AppendLine("");
                    strBuilder.AppendLine($"\t\tprotected override void Run(View_{name}Component view)");
                    strBuilder.AppendLine($"\t\t{{");
                    strBuilder.AppendLine("\t\t\tview.FindAll();");
                    strBuilder.AppendLine("\t\t\t//关闭窗口的事件注册,名字不同请自行更");
                    strBuilder.AppendLine("\t\t\tview.Button_Close.SetUIClose(view.ViewName);");
                    strBuilder.AppendLine(" \t\t\tview.Button_Mask.SetUIClose(view.ViewName);");
                    strBuilder.AppendLine("\t\t}");
                    strBuilder.AppendLine("\t}");

                    strBuilder.AppendLine(
                        $"\tpublic class View{name}OpenSystem :YuoSystem<View_{name}Component>, IUIOpen");
                    strBuilder.AppendLine("\t{");
                    strBuilder.Append("\t\t");
                    strBuilder.Append(@$"public override string Group =>""UI/{name}"";");
                    strBuilder.AppendLine("");
                    strBuilder.AppendLine("");
                    strBuilder.AppendLine(
                        $"\t\tprotected override void Run(View_{name}Component view)");
                    strBuilder.AppendLine($"\t\t{{");
                    strBuilder.AppendLine("\t\t}");
                    strBuilder.AppendLine("\t}");

                    strBuilder.AppendLine(
                        $"\tpublic class View{name}CloseSystem :YuoSystem<View_{name}Component>, IUIClose");
                    strBuilder.AppendLine("\t{");
                    strBuilder.Append("\t\t");
                    strBuilder.Append(@$"public override string Group =>""UI/{name}"";");
                    strBuilder.AppendLine("");
                    strBuilder.AppendLine("");
                    strBuilder.AppendLine(
                        $"\t\tprotected override void Run(View_{name}Component view)");
                    strBuilder.AppendLine($"\t\t{{");
                    strBuilder.AppendLine("\t\t}");
                    strBuilder.AppendLine("\t}");

                    strBuilder.AppendLine(
                        $"\tpublic class View{name}OpenAnimaSystem :YuoSystem<View_{name}Component,UIAnimaComponent>, IUIOpen");
                    strBuilder.AppendLine("\t{");
                    strBuilder.Append("\t\t");
                    strBuilder.Append(@$"public override string Group =>""UI/{name}"";");
                    strBuilder.AppendLine("");
                    strBuilder.AppendLine("");
                    strBuilder.AppendLine(
                        $"\t\tprotected override void Run(View_{name}Component view, UIAnimaComponent anima)");
                    strBuilder.AppendLine($"\t\t{{");
                    strBuilder.AppendLine("\t\t\tview.Button_Mask.image.SetColorA(0);\n");
                    strBuilder.AppendLine("\t\t\tview.Button_Mask.image.DOFade(0.6f, anima.AnimaDuration);");
                    strBuilder.AppendLine("\t\t}");
                    strBuilder.AppendLine("\t}");

                    strBuilder.AppendLine(
                        $"\tpublic class View{name}CloseAnimaSystem :YuoSystem<View_{name}Component,UIAnimaComponent>, IUIClose");
                    strBuilder.AppendLine("\t{");
                    strBuilder.Append("\t\t");
                    strBuilder.Append(@$"public override string Group =>""UI/{name}"";");
                    strBuilder.AppendLine("");
                    strBuilder.AppendLine("");
                    strBuilder.AppendLine(
                        $"\t\tprotected override void Run(View_{name}Component view, UIAnimaComponent anima)");
                    strBuilder.AppendLine("\t\t{");
                    strBuilder.AppendLine("\t\t\tview.Button_Mask.image.DOFade(0f, anima.AnimaDuration);");
                    strBuilder.AppendLine("\t\t}");
                    strBuilder.AppendLine("\t}");
                    break;
                case UIType.General:
                    strBuilder.AppendLine(
                        $"\tpublic class View{name}ActiveSystem :YuoSystem<View_{name}Component>, IUIActive");
                    strBuilder.AppendLine("\t{");
                    strBuilder.Append("\t\t");
                    strBuilder.Append(@$"public override string Group =>""UI/{name}"";");
                    strBuilder.AppendLine("");
                    strBuilder.AppendLine("");
                    strBuilder.AppendLine($"\t\tprotected override void Run(View_{name}Component view)");
                    strBuilder.AppendLine($"\t\t{{");
                    strBuilder.AppendLine("\t\t\tview.FindAll();");
                    strBuilder.AppendLine("\t\t}");
                    strBuilder.AppendLine("\t}");
                    break;
                case UIType.Child:
                    strBuilder.AppendLine(
                        $"\tpublic class View{name}CreateSystem :YuoSystem<View_{name}Component>, IUICreate");
                    strBuilder.AppendLine("\t{");
                    strBuilder.Append("\t\t");
                    strBuilder.Append(@$"public override string Group =>""UI/{name}"";");
                    strBuilder.AppendLine("");
                    strBuilder.AppendLine("");
                    strBuilder.AppendLine($"\t\tprotected override void Run(View_{name}Component view)");
                    strBuilder.AppendLine($"\t\t{{");
                    strBuilder.AppendLine("\t\t\tview.FindAll();");
                    strBuilder.AppendLine("\t\t}");
                    strBuilder.AppendLine("\t}");

                    strBuilder.AppendLine(
                        $"\tpublic class View{name}OpenSystem :YuoSystem<View_{name}Component>, IUIOpen");
                    strBuilder.AppendLine("\t{");
                    strBuilder.Append("\t\t");
                    strBuilder.Append(@$"public override string Group =>""UI/{name}"";");
                    strBuilder.AppendLine("");
                    strBuilder.AppendLine("");
                    strBuilder.AppendLine(
                        $"\t\tprotected override void Run(View_{name}Component view)");
                    strBuilder.AppendLine($"\t\t{{");
                    strBuilder.AppendLine("\t\t}");
                    strBuilder.AppendLine("\t}");

                    strBuilder.AppendLine(
                        $"\tpublic class View{name}CloseSystem :YuoSystem<View_{name}Component>, IUIClose");
                    strBuilder.AppendLine("\t{");
                    strBuilder.Append("\t\t");
                    strBuilder.Append(@$"public override string Group =>""UI/{name}"";");
                    strBuilder.AppendLine("");
                    strBuilder.AppendLine("");
                    strBuilder.AppendLine(
                        $"\t\tprotected override void Run(View_{name}Component view)");
                    strBuilder.AppendLine($"\t\t{{");
                    strBuilder.AppendLine("\t\t}");
                    strBuilder.AppendLine("\t}");
                    break;
                default:
                    break;
            }

            strBuilder.AppendLine("}");

            sw.Write(strBuilder.ToString());
            sw.Close();
        }

        public static SpawnUICodeConfig SpawnConfig
        {
            get
            {
#if UNITY_EDITOR
                if (_spawnConfig == null)
                {
                    _spawnConfig = new();
                    ReflexHelper.InvokeMethodByPrefix(_spawnConfig, "Init");
                }
#endif
                return _spawnConfig;
            }
        }

        private static SpawnUICodeConfig _spawnConfig;

        private static List<string> GetTypes(Transform transform)
        {
            List<string> ts = new List<string>();

            foreach (var item in SpawnConfig.SpawnType)
            {
                Get(item);
            }

            foreach (var item in transform.GetComponents<IGenerateCode>())
            {
                ts.Add(item.GetType().Name);
            }

            foreach (var item in SpawnConfig.RemoveType)
            {
                if (ts.Contains(item.Key.Name)) ts.Remove(item.Value.Name);
            }


            if (ts.Count == 0)
            {
                ts.Add(nameof(RectTransform));
            }

            return ts;

            void Get(Type type)
            {
                var t = transform.GetComponent(type);
                if (t != null)
                {
                    ts.Add(t.GetType().Name);
                }
            }
        }

        private static List<Type> GetOriginalTypes(Transform transform)
        {
            List<Type> ts = new List<Type>();

            foreach (var item in SpawnConfig.SpawnType)
            {
                Get(item);
            }

            foreach (var item in transform.GetComponents<IGenerateCode>())
            {
                ts.Add(item.GetType());
            }

            foreach (var item in SpawnConfig.RemoveType)
            {
                if (ts.Contains(item.Key)) ts.Remove(item.Value);
            }


            if (ts.Count == 0)
            {
                ts.Add(typeof(RectTransform));
            }

            return ts;

            void Get(Type type)
            {
                var t = transform.GetComponent(type);
                if (t != null)
                {
                    ts.Add(t.GetType());
                }
            }
        }

        private static string GetRelativePath(Transform child, Transform parent)
        {
            if (child == parent)
            {
                return parent.name;
            }

            var path = child.name;
            Transform nowParent = child.parent;
            while (nowParent != parent && nowParent != null)
            {
                path = nowParent.name + "/" + path;
                nowParent = nowParent.parent;
            }

            return path;
        }
        
        class TransformInfo
        {
            public readonly Transform transform;
            public readonly List<TransformInfo> children = new();

            public TransformInfo(Transform transform)
            {
                this.transform = transform;
            }
        }

        private static void FindAll(TransformInfo transform)
        {
            for (int i = 0; i < transform.transform.childCount; i++)
            {
                if (transform.transform.GetChild(i).childCount > 0)
                {
                    var child = new TransformInfo(transform.transform.GetChild(i));
                    transform.children.Add(child);
                    FindAll(child);
                }
                else
                {
                    var child = new TransformInfo(transform.transform.GetChild(i));
                    transform.children.Add(child);
                }
            }
        }
    }
}