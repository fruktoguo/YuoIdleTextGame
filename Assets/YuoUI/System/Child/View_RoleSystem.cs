using Manager.Tool;
using UnityEngine;
using UnityEngine.Events;
using YuoTools.Extend;
using YuoTools.Extend.PhysicalCallback;
using YuoTools.Main.Ecs;

namespace YuoTools.UI
{
    public partial class View_RoleComponent
    {
        public View_RoleComponent Target;
        public Vector3 MoveDir;
        public Rigidbody2D Rig;

        public RoleData Data = new();

        public TimerCountDown MoveTimer = new();

        public Vector2 LastVelocity;

        public void SetMass(float mass)
        {
            Rig.mass = mass;
            //根据球形质量求半径,质量为1时半径为1
            var radius = Mathf.Pow(mass, 1 / 3f);
            rectTransform.localScale = Vector3.one * radius;
        }

        public float totalDamage;
    }

    public class TimerCountDown
    {
        public float Timer;

        public float MaxTimer = 1;

        public UnityEvent OnTimerEnd = new();

        public void Update(float deltaTime)
        {
            Timer -= deltaTime;
            while (Timer <= 0)
            {
                Timer += MaxTimer;
                OnTimerEnd?.Invoke();
            }
        }
    }

    public class RoleData
    {
        public ValuePropertyComplex Speed = new(50);

        public ValuePropertyComplex Def = new(0.2f);
    }

    public class ViewRoleCreateSystem : YuoSystem<View_RoleComponent>, IUICreate
    {
        public override string Group => "UI/Role";

        protected override void Run(View_RoleComponent view)
        {
            view.FindAll();

            view.Entity.AddComponent<TransformComponent>().transform = view.rectTransform;
            view.Entity.AddComponent<PhysicalCallback2DComponent>();

            view.Rig = view.rectTransform.GetComponent<Rigidbody2D>();
            view.MoveTimer.MaxTimer = 1;
            view.MoveTimer.OnTimerEnd.AddListener(() =>
            {
                view.Rig.AddForce(view.MoveDir * view.Data.Speed / view.Data.Def);
            });
        }
    }

    public class ViewRoleOpenSystem : YuoSystem<View_RoleComponent>, IUIOpen
    {
        public override string Group => "UI/Role";

        protected override void Run(View_RoleComponent view)
        {
        }
    }

    public class ViewRoleFixedSystem : YuoSystem<View_RoleComponent>, IFixedUpdate
    {
        public override string Group => "UI/Role";

        protected override void Run(View_RoleComponent view)
        {
            if (view.Target != null)
            {
                view.MoveDir = (view.Target.rectTransform.position - view.rectTransform.position).normalized;
                view.MoveTimer.Update(Time.deltaTime * view.Data.Def);
            }

            view.LastVelocity = view.Rig.linearVelocity;
        }
    }

    public class RoleCollisionSystem : YuoSystem<View_RoleComponent, PhysicalCallback2DComponent>,
        IOnCollisionEnter2D
    {
        public override string Group => "UI/Role";

        protected override void Run(View_RoleComponent view, PhysicalCallback2DComponent callback)
        {
            var collider = callback.eventData;

            var entity = callback.eventDataEntity;

            foreach (var contactPoint2D in collider.contacts)
            {
                float maxImpulse = contactPoint2D.normalImpulse;

                float ratio = 1;

                if (contactPoint2D.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
                {
                    // $"{view.Entity} 撞墙了  当前速度{view.Rig.velocity.magnitude:F2}".Log();
                }
                else if (contactPoint2D.collider.gameObject.layer == LayerMask.NameToLayer("Role"))
                {
                    var selfRig = view.Rig;
                    var targetRig = contactPoint2D.rigidbody;

                    var selfRole = view;
                    var targetRole = entity.GetComponent<View_RoleComponent>();

                    var selfLastVelocity = selfRole.LastVelocity;
                    var targetLastVelocity = targetRole.LastVelocity;


                    // 计算速度方向和连线方向的夹角余弦值
                    var collisionDirection = (targetRig.position - selfRig.position).normalized;
                    var selfVelocityDirection = selfLastVelocity.normalized;
                    var targetVelocityDirection = targetLastVelocity.normalized;

                    float selfCosAngle = (Vector2.Dot(selfVelocityDirection, collisionDirection) + 1) / 2;
                    float targetCosAngle = (Vector2.Dot(targetVelocityDirection, -collisionDirection) + 1) / 2;

                    // 计算绝对速度反比
                    var velocityAngleRatioReverse = (selfLastVelocity.magnitude * selfCosAngle) /
                                                    (targetLastVelocity.magnitude * targetCosAngle);

                    //这个反比只会用于减伤
                    velocityAngleRatioReverse.Clamp(1, 100f);
                    
                    $"selfCosAngle {selfCosAngle:f2} targetCosAngle {targetCosAngle:f2}  绝对速度反比 :{velocityAngleRatioReverse:f2}"
                        .Log();

                    var velocityRatio = targetLastVelocity.magnitude / selfLastVelocity.magnitude;
                    velocityRatio.Clamp(0.01f, 100f);
                    var massRatio = targetRig.mass / selfRig.mass;

                    maxImpulse.Max(targetLastVelocity.magnitude * targetRig.mass);

                    var velocityRatioReverse = selfLastVelocity.magnitude / targetLastVelocity.magnitude;
                    velocityRatioReverse.Clamp(0.1f, 100f);

                    if (massRatio > 1)
                    {
                        massRatio -= 1;
                        massRatio /= velocityRatioReverse;
                        massRatio += 1;
                    }
                    else
                    {
                        massRatio = 1 / massRatio;
                        massRatio *= velocityRatioReverse;
                        massRatio = 1 / massRatio;
                    }

                    $"速度倍率 {velocityRatio:f2} 质量倍率 {massRatio:f2} 质量原始倍率 {targetRig.mass / selfRig.mass:f2}"
                        .Log();
                    
                    ratio = velocityRatio * massRatio / velocityAngleRatioReverse;
                    // $"{view.Entity} 撞了 {entity} 速度差{v:F2}".Log();
                }

                var damage = contactPoint2D.normalImpulse * ratio;
                damage.Clamp(maxImpulse);

                if (damage > 0.5f)
                {
                    $"{view.Entity} 受到了 来自 {contactPoint2D.collider.gameObject.name} 的冲量 {contactPoint2D.normalImpulse} 倍率 {ratio:f2} 实际伤害{damage:f2}"
                        .Log();
                    view.totalDamage += damage;
                    View_DamageNumComponent.GetView().ShowNum((long)damage, view.rectTransform.position);
                }
            }
        }
    }
}