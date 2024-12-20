using UnityEngine;
using UnityEngine.UI;

namespace YuoTools
{
    public static class YuoRectTransformExtension
    {
        public static void SetSize(this RectTransform rectTransform, float width, float height)
        {
            Temp.V2.Set(width, height);
            rectTransform.sizeDelta = Temp.V2;
        }

        public static void SetSize(this RectTransform rectTransform, Vector2 size)
        {
            rectTransform.sizeDelta = size;
        }

        public static void SetSizeX(this RectTransform rectTransform, float size)
        {
            rectTransform.sizeDelta = rectTransform.sizeDelta.RSetX(size);
        }

        public static void SetSizeY(this RectTransform rectTransform, float size)
        {
            rectTransform.sizeDelta = rectTransform.sizeDelta.RSetY(size);
        }

        public static void Copy(this RectTransform target, RectTransform from)
        {
            target.localScale = from.localScale;
            target.anchorMin = from.anchorMin;
            target.anchorMax = from.anchorMax;
            target.pivot = from.pivot;
            target.sizeDelta = from.sizeDelta;
            target.anchoredPosition3D = from.anchoredPosition3D;
        }

        public static void FullScreen(this RectTransform target, bool resetScaleToOne)
        {
            if (resetScaleToOne) target.ResetLocalScaleToOne();
            target.AnchorMinToZero();
            target.AnchorMaxToOne();
            target.CenterPivot();
            target.SizeDeltaToZero();
            target.ResetAnchoredPosition3D();
            target.ResetLocalPosition();
        }

        public static void Center(this RectTransform target, bool resetScaleToOne)
        {
            if (resetScaleToOne) target.ResetLocalScaleToOne();
            target.AnchorMinToCenter();
            target.AnchorMaxToCenter();
            target.CenterPivot();
            target.SizeDeltaToZero();
        }

        public static void ResetAnchoredPosition3D(this RectTransform target)
        {
            target.anchoredPosition3D = Vector3.zero;
        }

        public static void ResetLocalPosition(this RectTransform target)
        {
            target.localPosition = Vector3.zero;
        }

        public static void ResetLocalScaleToOne(this RectTransform target)
        {
            target.localScale = Vector3.one;
        }

        public static void AnchorMinToZero(this RectTransform target)
        {
            target.anchorMin = Vector2.zero;
        }

        public static void AnchorMinToCenter(this RectTransform target)
        {
            target.anchorMin = Vector2.one * 0.5f;
        }

        public static void AnchorMaxToOne(this RectTransform target)
        {
            target.anchorMax = Vector2.one;
        }

        public static void AnchorMaxToCenter(this RectTransform target)
        {
            target.anchorMax = Vector2.one * 0.5f;
        }

        public static void CenterPivot(this RectTransform target)
        {
            target.pivot = Vector2.one * 0.5f;
        }

        public static void SetPosRatioInParent(this RectTransform target, float x, float y)
        {
            x.Clamp();
            y.Clamp();

            target.anchorMin = new(x, 1 - y);
            target.anchorMax = new(x, 1 - y);
            target.anchoredPosition = Vector2.zero;
        }

        public static void SizeDeltaToZero(this RectTransform target)
        {
            target.sizeDelta = Vector2.zero;
        }

        public static float GetPreferredWidth(this RectTransform target)
        {
            return LayoutUtility.GetPreferredWidth(target);
        }

        public static float GetPreferredHeight(this RectTransform target)
        {
            return LayoutUtility.GetPreferredHeight(target);
        }

        public static Vector2 GetPreferredSize(this RectTransform target)
        {
            return Temp.V2.RSet(target.GetPreferredWidth(), target.GetPreferredHeight());
        }

        public static Vector3 SetAnchoredPosX(this RectTransform tran, float posX)
        {
            tran.anchoredPosition = Temp.V2.RSet(posX, tran.anchoredPosition.y);
            return tran.anchoredPosition;
        }

        public static Vector3 SetAnchoredPosY(this RectTransform tran, float posY)
        {
            tran.anchoredPosition = Temp.V2.RSet(tran.anchoredPosition.x, posY);
            return tran.anchoredPosition;
        }

        public static Vector2 AddAnchoredPosX(this RectTransform tran, float posX)
        {
            Temp.V2.Set(tran.anchoredPosition.x + posX, tran.anchoredPosition.y);
            tran.anchoredPosition = Temp.V2;
            return tran.anchoredPosition;
        }

        public static Vector2 AddAnchoredPosY(this RectTransform tran, float posY)
        {
            Temp.V2.Set(tran.anchoredPosition.x, tran.anchoredPosition.y + posY);
            tran.anchoredPosition = Temp.V2;
            return tran.anchoredPosition;
        }

        public static Vector2 AutoPreferredSizeX(this RectTransform rect, RectTransform target, float add = 0)
        {
            rect.sizeDelta = Temp.V2.RSet(target.GetPreferredWidth() + add, rect.sizeDelta.y);
            return rect.sizeDelta;
        }

        public static Vector2 AutoPreferredSizeY(this RectTransform rect, RectTransform target, float add = 0)
        {
            rect.sizeDelta = Temp.V2.RSet(rect.sizeDelta.x, target.GetPreferredHeight() + add);
            return rect.sizeDelta;
        }

        public static Vector2 AutoPreferredSize(this RectTransform rect, RectTransform target, Vector2 add = default)
        {
            rect.sizeDelta = Temp.V2.RSet(target.GetPreferredWidth() + add.x, target.GetPreferredHeight() + add.y);
            return rect.sizeDelta;
        }

        public static void ForceRebuildLayout(this RectTransform transform)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
        }
    }
}