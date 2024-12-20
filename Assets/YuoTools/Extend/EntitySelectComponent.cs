using UnityEngine;
using YuoTools.Main.Ecs;

namespace YuoTools.Extend
{
    public class EntitySelectComponent : YuoComponent, IComponentInit<GameObject>
    {
        public GameObject SelectGameObject;

        public void ComponentInit(GameObject go)
        {
            SelectGameObject = go;
        }
    }
}