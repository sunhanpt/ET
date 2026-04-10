namespace ET
{
    /// <summary>
    /// 人口组件，挂在 Scene 上，管理村庄人口上限
    /// 初始上限 = 5；每建成一座 House，上限 +5
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class PopulationComponent : Entity, IAwake, IDestroy
    {
        public int MaxPopulation = 5;
    }
}
