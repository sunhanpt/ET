using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ET
{
    [Config]
    public partial class ResourceConfigCategory : Singleton<ResourceConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, ResourceConfig> dict = new();

        public void Merge(object o)
        {
            ResourceConfigCategory s = o as ResourceConfigCategory;
            foreach (var kv in s.dict)
                this.dict.Add(kv.Key, kv.Value);
        }

        public ResourceConfig Get(int id)
        {
            this.dict.TryGetValue(id, out ResourceConfig item);
            if (item == null)
                throw new Exception($"配置找不到，配置表名: {nameof(ResourceConfig)}，配置id: {id}");
            return item;
        }

        public bool Contain(int id) => this.dict.ContainsKey(id);

        public Dictionary<int, ResourceConfig> GetAll() => this.dict;
    }

    public partial class ResourceConfig : ProtoObject, IConfig
    {
        /// <summary>Id</summary>
        public int Id { get; set; }
        /// <summary>资源名称</summary>
        public string Name { get; set; }
        /// <summary>资源类型 1木材 2石矿 3食物</summary>
        public int ResourceType { get; set; }
        /// <summary>单次采集耗时(毫秒)</summary>
        public int GatherTime { get; set; }
        /// <summary>单次采集数量</summary>
        public int GatherAmount { get; set; }
        /// <summary>资源节点最大储量</summary>
        public int MaxAmount { get; set; }
        /// <summary>预制体路径</summary>
        public string PrefabPath { get; set; }
        /// <summary>采集动画名称</summary>
        public string GatherMotion { get; set; }
        /// <summary>村民靠近距离</summary>
        public float WalkToOffset { get; set; }
    }
}

