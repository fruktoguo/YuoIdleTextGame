  using UnityEngine;
  using System.Collections.Generic;
  using System.Runtime.InteropServices;

  public class MetaBall : MonoBehaviour
  {
      private static List<MetaBall> activeBalls = new();
      private static ComputeBuffer ballBuffer;
      private static readonly int BallBuffer = Shader.PropertyToID("_BallBuffer");
      private static readonly int BallCount = Shader.PropertyToID("_BallCount");

      [System.Serializable]
      [StructLayout(LayoutKind.Sequential)]
      public struct BallData
      {
          public Vector2 position;
          public float radius;
          public Vector4 color;  // 使用Vector4来存储Color (rgba)
      }

      [SerializeField] private float radius = 1f;
      [SerializeField] private Material targetMaterial;
      [SerializeField] private Color ballColor = Color.white;  // 添加颜色属性

      private void OnEnable()
      {
          if (!activeBalls.Contains(this))
          {
              activeBalls.Add(this);
              UpdateBuffer();
          }
      }

      private void OnDisable()
      {
          if (activeBalls.Contains(this))
          {
              activeBalls.Remove(this);
              UpdateBuffer();
          }
      }

      private static void UpdateBuffer()
      {
          if (ballBuffer != null)
          {
              ballBuffer.Release();
              ballBuffer = null;
          }

          if (activeBalls.Count > 0)
          {
              // 更新结构体大小：Vector2(8) + float(4) + Vector4(16) = 28 bytes
              ballBuffer = new ComputeBuffer(activeBalls.Count, Marshal.SizeOf<BallData>());

              // 更新所有使用这个buffer的材质
              foreach (var ball in activeBalls)
              {
                  if (ball.targetMaterial != null)
                  {
                      ball.targetMaterial.SetBuffer(BallBuffer, ballBuffer);
                      ball.targetMaterial.SetInt(BallCount, activeBalls.Count);
                  }
              }
          }
      }

      private int isUpdate;

      private void LateUpdate()
      {
          if (ballBuffer == null || targetMaterial == null) return;
          if (isUpdate == Time.frameCount) return;
          isUpdate = Time.frameCount;
          
          BallData[] ballData = new BallData[activeBalls.Count];
          for (int i = 0; i < activeBalls.Count; i++)
          {
              ballData[i] = new BallData
              {
                  position = activeBalls[i].transform.position,
                  radius = activeBalls[i].radius * activeBalls[i].transform.lossyScale.x,
                  color = activeBalls[i].ballColor  // 设置每个球的颜色
              };
          }

          ballBuffer.SetData(ballData);
      }

      private void OnDestroy()
      {
          if (activeBalls.Count == 0 && ballBuffer != null)
          {
              ballBuffer.Release();
              ballBuffer = null;
          }
      }

  #if UNITY_EDITOR
      private void OnValidate()
      {
          if (Application.isPlaying && gameObject.activeInHierarchy)
          {
              UpdateBuffer();
          }
      }
  #endif
  }