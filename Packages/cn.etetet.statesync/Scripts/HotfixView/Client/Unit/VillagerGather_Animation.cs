namespace ET.Client
{
    /// <summary>
    /// 监听 VillagerGatherStart 事件，切换村民采集动画
    /// </summary>
    [Event(SceneType.Current)]
    public class VillagerGatherStart_PlayAnimation : AEvent<Scene, VillagerGatherStart>
    {
        protected override async ETTask Run(Scene scene, VillagerGatherStart args)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            Unit villager = unitComponent.Get(args.VillagerUnitId);
            if (villager == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            AnimatorComponent animator = villager.GetComponent<AnimatorComponent>();
            if (animator == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            // 解析采集动画名为 MotionType 枚举，再驱动 Animator
            animator.SetBoolValue("IsGathering", true);
            if (System.Enum.TryParse(args.GatherMotion, out MotionType motionType))
                animator.Play(motionType);
            else
                Log.Warning($"未知采集动画名: {args.GatherMotion}");

            await ETTask.CompletedTask;
        }
    }

    /// <summary>
    /// 监听 VillagerGatherStop 事件，停止采集动画回到 Idle
    /// </summary>
    [Event(SceneType.Current)]
    public class VillagerGatherStop_StopAnimation : AEvent<Scene, VillagerGatherStop>
    {
        protected override async ETTask Run(Scene scene, VillagerGatherStop args)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            Unit villager = unitComponent.Get(args.VillagerUnitId);
            if (villager == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            AnimatorComponent animator = villager.GetComponent<AnimatorComponent>();
            if (animator == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            animator.SetBoolValue("IsGathering", false);
            animator.Play(MotionType.Idle);

            await ETTask.CompletedTask;
        }
    }
}

