using DG.Tweening;
using UnityEngine;
using YuoTools.Extend.Helper;
using YuoTools.Main.Ecs;

namespace YuoTools.UI
{
    public partial class View_DistanceItemComponent
    {
        public Vector3Action startPos;
        public Vector3Action endPos;

        [Range(0, 1)] public float textPos = 0.5f;
        public float offset = 100;
        public float minDistance = 100;


        public void UpdateRender()
        {
            
        }
    }

    public class ViewDistanceItemCreateSystem : YuoSystem<View_DistanceItemComponent>, IUICreate
    {
        public override string Group => "UI/DistanceItem";

        public override void Run(View_DistanceItemComponent view)
        {
            view.FindAll();
        }
    }

    public class ViewDistanceItemOpenSystem : YuoSystem<View_DistanceItemComponent>, IUIOpen
    {
        public override string Group => "UI/DistanceItem";

        public override void Run(View_DistanceItemComponent view)
        {
            if (view.startPos == null || view.endPos == null) return;
            
            //动画
            float animaTime = 0.5f;
        }
    }

    public class ViewDistanceItemCloseSystem : YuoSystem<View_DistanceItemComponent>, IUIClose
    {
        public override string Group => "UI/DistanceItem";

        public override void Run(View_DistanceItemComponent view)
        {
        }
    }

    public class ViewDistanceItemSystem : YuoSystem<View_DistanceItemComponent>, IUIUpdate
    {
        public override string Group => "UI/DistanceItem";

        public override void Run(View_DistanceItemComponent view)
        {
            if (view.startPos == null || view.endPos == null) return;
            var startPos = view.startPos();
            var endPos = view.endPos();

            var canvas = UIManagerComponent.Get.Canvas;
            var canvasCamera = canvas.worldCamera;

            // 将世界坐标转换为屏幕坐标
            Vector2 screenStartPos = RectTransformUtility.WorldToScreenPoint(canvasCamera, startPos);
            Vector2 screenEndPos = RectTransformUtility.WorldToScreenPoint(canvasCamera, endPos);

            // 将屏幕坐标转换为画布坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                screenStartPos,
                canvasCamera,
                out Vector2 canvasStartPos);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                screenEndPos,
                canvasCamera,
                out Vector2 canvasEndPos);

            // 计算在画布上的位置和角度
            var linePos = (canvasStartPos + canvasEndPos) / 2;
            var direction = (canvasEndPos - canvasStartPos).normalized;
            var distance = Vector2.Distance(canvasStartPos, canvasEndPos) - view.offset;

            // 更新UI元素
            view.RectTransform_Line.localPosition = linePos;
            // 计算2D旋转角度
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            view.RectTransform_Line.localRotation = Quaternion.Euler(0, 0, angle);
            // 设置线条长度（通过RectTransform的sizeDelta）
            view.RectTransform_Line.sizeDelta = new Vector2(distance, view.RectTransform_Line.sizeDelta.y);


            view.RectTransform_Line.gameObject.SetActive(distance > view.minDistance);
            // 更新距离文本
            // 使用世界坐标计算实际距离
            var worldDistance = Vector3.Distance(startPos, endPos);
            view.TextMeshProUGUI_DistanceText.text = $"{worldDistance:F1}m";

            var textPos = Vector2.Lerp(canvasStartPos, canvasEndPos, view.textPos);
            view.TextMeshProUGUI_DistanceText.rectTransform.localPosition = textPos;
        }
    }
}