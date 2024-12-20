using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using YuoTools.Main.Ecs;

/// <summary>
/// 卡片攻击动画控制器
/// 使用DOTween实现简单的冲击式攻击动画
/// </summary>
public class RoleAttackAnimator : YuoComponent, IComponentInit<Transform>
{
    private Transform targetTransform; // 要进行动画的Transform组件
    private Vector3 originalLocalPosition; // 原始局部位置
    private Sequence currentSequence; // 当前的动画序列

    public Vector3 attackDirection;

    // 动画配置参数
    public float attackDistance = 1f; // 攻击移动距离
    public float attackDuration = 0.2f; // 冲出时间
    public float returnDuration = 0.15f; // 返回时间
    public Ease attackEase = Ease.OutQuad; // 冲出的缓动类型
    public Ease returnEase = Ease.InQuad; // 返回的缓动类型
    public bool useShake = true; // 是否使用震动效果
    public float shakeStrength = 0.1f; // 震动强度
    public float shakeDuration = 0.1f; // 震动持续时间

    public void ComponentInit(Transform data)
    {
        targetTransform = data;
        originalLocalPosition = targetTransform.localPosition;
    }

    /// <summary>
    /// 播放攻击动画
    /// </summary>
    /// <param name="onComplete">动画完成回调</param>
    /// <returns>动画序列，可用于外部控制</returns>
    [Button]
    public Sequence PlayAttack(System.Action onComplete = null)
    {
        // 如果有正在播放的动画，先杀死它
        if (currentSequence != null && currentSequence.IsPlaying())
        {
            currentSequence.Kill();
            targetTransform.localPosition = originalLocalPosition;
        }

        attackDirection = targetTransform.right;

        // 确保位置正确
        originalLocalPosition = targetTransform.localPosition;

        // 计算目标位置（向右移动）
        Vector3 targetPosition = originalLocalPosition + attackDirection * attackDistance;

        // 创建动画序列
        currentSequence = DOTween.Sequence();

        // 添加冲出动画
        currentSequence.Append(
            targetTransform.DOLocalMove(targetPosition, attackDuration)
                .SetEase(attackEase)
        );

        // 添加震动效果（如果启用）
        if (useShake)
        {
            currentSequence.Append(
                targetTransform.DOShakePosition(shakeDuration, shakeStrength, 10, 90, false, true)
                    .SetEase(Ease.OutQuad)
            );
        }

        // 添加返回动画
        currentSequence.Append(
            targetTransform.DOLocalMove(originalLocalPosition, returnDuration)
                .SetEase(returnEase)
        );

        // 设置完成回调
        if (onComplete != null)
        {
            currentSequence.OnComplete(onComplete.Invoke);
        }

        // 设置动画序列的一些通用属性
        currentSequence
            .SetAutoKill(true) // 播放完自动销毁
            .SetUpdate(true) // 使用已缩放的时间
            .SetLink(targetTransform.gameObject); // 关联游戏对象（对象销毁时自动停止动画）

        return currentSequence;
    }

    /// <summary>
    /// 立即停止当前动画
    /// </summary>
    /// <param name="returnToStart">是否返回起始位置</param>
    [Button]
    public void Stop(bool returnToStart = true)
    {
        if (currentSequence != null && currentSequence.IsPlaying())
        {
            currentSequence.Kill();
            if (returnToStart)
            {
                targetTransform.localPosition = originalLocalPosition;
            }
        }
    }
}