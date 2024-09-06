using System;
using YuoTools.Main.Ecs;

namespace Combat
{
    #region 攻击治疗数值

    /// <summary>
    ///  伤害数值计算前,在暴击等数值计算前
    /// </summary>
    public interface IOnAttackValueBefore : ISystemTag
    {
    }

    /// <summary>
    ///  伤害数值计算后
    /// </summary>
    public interface IOnAttackValueAfter : ISystemTag
    {
    }

    /// <summary>
    ///  伤害数值计算
    /// </summary>
    public interface IOnAttackValue : ISystemTag
    {
    }

    /// <summary>
    ///  被伤害数值计算前,原始数值,没有被护甲减免
    /// </summary>
    public interface IOnHurtValueBefore : ISystemTag
    {
    }

    /// <summary>
    ///  被伤害数值计算后,最终数值,只读,修改不会影响伤害
    /// </summary>
    public interface IOnHurtValueAfter : ISystemTag
    {
    }

    /// <summary>
    /// 被伤害数值计算,经过护甲和免伤计算后的数值,可以继续修改
    /// </summary>
    public interface IOnHurtValue : ISystemTag
    {
    }

    /// <summary>
    ///  治疗数值计算前
    /// </summary>
    public interface IOnCureValueBefore : ISystemTag
    {
    }

    /// <summary>
    ///  治疗数值计算后
    /// </summary>
    public interface IOnCureValueAfter : ISystemTag
    {
    }

    /// <summary>
    ///  治疗数值计算
    /// </summary>
    public interface IOnCureValue : ISystemTag
    {
    }

    /// <summary>
    ///  被治疗数值计算前
    /// </summary>
    public interface IOnBeCureValueBefore : ISystemTag
    {
    }

    /// <summary>
    ///  被治疗数值计算后
    /// </summary>
    public interface IOnBeCureValueAfter : ISystemTag
    {
    }

    /// <summary>
    ///  被治疗数值计算
    /// </summary>
    public interface IOnBeCureValue : ISystemTag
    {
    }

    #endregion

    #region 攻击治疗行为

    /// <summary>
    ///  攻击前,动画播放前会调用
    /// </summary>
    public interface IOnAttackBeforeAnima : ISystemTag
    {
    }

    /// <summary>
    ///  普通攻击触发时调用
    /// </summary>
    public interface IOnAttackBefore : ISystemTag
    {
    }

    public interface IOnAttackAnima : ISystemTag
    {
    }

    /// <summary>
    ///  攻击后
    /// </summary>
    public interface IOnAttackAfter : ISystemTag
    {
    }

    /// <summary>
    ///  被攻击
    /// </summary>
    public interface IOnHurtBefore : ISystemTag
    {
    }

    /// <summary>
    ///  被攻击后
    /// </summary>
    public interface IOnHurtAfter : ISystemTag
    {
    }


    /// <summary>
    ///  治疗前
    /// </summary>
    public interface IOnCureBefore : ISystemTag
    {
    }

    /// <summary>
    ///  治疗后
    /// </summary>
    public interface IOnCureAfter : ISystemTag
    {
    }

    /// <summary>
    ///  治疗
    /// </summary>
    public interface IOnCure : ISystemTag
    {
    }

    /// <summary>
    ///  被治疗前
    /// </summary>
    public interface IOnBeCureBefore : ISystemTag
    {
    }

    /// <summary>
    ///  被治疗后
    /// </summary>
    public interface IOnBeCureAfter : ISystemTag
    {
    }

    /// <summary>
    ///  被治疗
    /// </summary>
    public interface IOnBeCure : ISystemTag
    {
    }

    #endregion

    public enum AttackType
    {
        /// <summary>
        ///  物理伤害
        /// </summary>
        Physical,

        /// <summary>
        ///  魔法伤害
        /// </summary>
        Magic,

        /// <summary>
        ///  真实伤害
        /// </summary>
        Real,

        /// <summary>
        ///  混合伤害
        /// </summary>
        Mix,
    }

    public enum CureType
    {
        /// <summary>
        ///  治疗
        /// </summary>
        Heal,

        /// <summary>
        ///  护盾
        /// </summary>
        Shield,

        /// <summary>
        ///  回蓝
        /// </summary>
        Mana,
    }

    public class AtkSystemTagType
    {
        public static readonly Type IOnAttackBeforeAnima = typeof(IOnAttackBeforeAnima);
        public static readonly Type IOnAttackValueBefore = typeof(IOnAttackValueBefore);
        public static readonly Type IOnAttackValueAfter = typeof(IOnAttackValueAfter);
        public static readonly Type IOnAttackValue = typeof(IOnAttackValue);
        public static readonly Type IOnHurtValueBefore = typeof(IOnHurtValueBefore);
        public static readonly Type IOnHurtValueAfter = typeof(IOnHurtValueAfter);
        public static readonly Type IOnHurtValue = typeof(IOnHurtValue);
        public static readonly Type IOnCureValueBefore = typeof(IOnCureValueBefore);
        public static readonly Type IOnCureValueAfter = typeof(IOnCureValueAfter);
        public static readonly Type IOnCureValue = typeof(IOnCureValue);
        public static readonly Type IOnBeCureValueBefore = typeof(IOnBeCureValueBefore);
        public static readonly Type IOnBeCureValueAfter = typeof(IOnBeCureValueAfter);
        public static readonly Type IOnBeCureValue = typeof(IOnBeCureValue);
    }
}