  using UnityEngine;
  using UnityEngine.UI;
  using UnityEditor;

  public class UIStencilDebugger : MonoBehaviour
  {
      public Material visualizerMaterial;
      private Canvas[] canvases;
      private Mask[] masks;

      void OnEnable()
      {
          UpdateStencilInfo();
      }

      void UpdateStencilInfo()
      {
          canvases = FindObjectsOfType<Canvas>();
          masks = FindObjectsOfType<Mask>();

          Debug.Log("=== UI Stencil Debug Info ===");
          
          // 显示Canvas信息
          foreach (var canvas in canvases)
          {
              int stencilValue = 0;
              if (canvas.overrideSorting)
              {
                  Canvas parent = canvas.transform.parent?.GetComponentInParent<Canvas>();
                  stencilValue = parent != null ? GetCanvasStencilDepth(canvas) : 1;
              }
              
              Debug.Log($"Canvas '{canvas.name}' - Stencil Value: {stencilValue}");
          }

          // 显示Mask信息
          foreach (var mask in masks)
          {
              int maskValue = GetMaskStencilValue(mask);
              Debug.Log($"Mask '{mask.name}' - Stencil Value: {maskValue}");
          }
      }

      int GetCanvasStencilDepth(Canvas canvas)
      {
          int depth = 0;
          Transform current = canvas.transform;
          
          while (current != null)
          {
              Canvas parentCanvas = current.GetComponent<Canvas>();
              if (parentCanvas != null && parentCanvas.overrideSorting)
                  depth++;
              current = current.parent;
          }
          
          return depth;
      }

      int GetMaskStencilValue(Mask mask)
      {
          int baseValue = 1;
          Transform current = mask.transform;
          
          while (current != null)
          {
              if (current.GetComponent<Mask>() != mask)
                  baseValue++;
              current = current.parent;
          }
          
          return baseValue;
      }
  }
  
  

#if UNITY_EDITOR
  [CustomEditor(typeof(UIStencilDebugger))]
  public class UIStencilDebuggerEditor : Editor
  {
      public override void OnInspectorGUI()
      {
          DrawDefaultInspector();
          
          UIStencilDebugger debugger = (UIStencilDebugger)target;
          
          if (GUILayout.Button("Update Stencil Info"))
          {
              debugger.SendMessage("UpdateStencilInfo");
          }

          EditorGUILayout.HelpBox(
              "常见Stencil值:\n" +
              "0: 默认值\n" +
              "1: 第一层Mask\n" +
              "2: 第二层Mask\n" +
              "增加值表示更深的嵌套层级",
              MessageType.Info);
      }
  }
#endif