using UnityEngine;
using UnityEngine.Events;
using YuoTools.UI;

namespace YuoTools.Extend
{
    [RequireComponent(typeof(MouseInteractable))]
    public class HoverExpandColumn : MonoBehaviour
    {
        public UnityEvent<View_ColumnPanelComponent> OnExpandPanel;
        private View_ColumnPanelComponent panel;

        private void Start()
        {
            var mouse = GetComponent<MouseInteractable>();

            mouse.OnMouseEnterEvent.AddListener(x =>
            {
                if (panel is { isShow: true })
                {
                    panel.AutonomousHiding = false;
                    return;
                }

                if (OnExpandPanel == null) return;
                panel = View_ColumnComponent.GetView().GetPanel();
                OnExpandPanel.Invoke(panel);
                for (var i = 0; i < 12; i++) panel.AddItem();

                panel.SetPosFormTransform(transform);
                panel.Layout();
            });

            mouse.OnMouseExitEvent.AddListener(x =>
            {
                if (OnExpandPanel == null) return;
                if (panel != null)
                {
                    if (panel.inRange)
                    {
                        panel.AutonomousHiding = true;
                    }
                    else
                    {
                        View_ColumnComponent.GetView().RemovePanel(panel);
                        panel = null;
                    }
                }
            });
        }
    }
}