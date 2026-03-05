using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ET
{
    [Config]
    public partial class BuildingConfigCategory : Singleton<BuildingConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, BuildingConfig> dict = new();

        public void Merge(object o)
        {
            BuildingConfigCategory s = o as BuildingConfigCategory;
            foreach (var kv in s.dict)
                this.dict.Add(kv.Key, kv.Value);
        }

        public BuildingConfig Get(int id)
        {
            this.dict.TryGetValue(id, out BuildingConfig item);
            if (item == null)
                throw new Exception($"配置找不到，配置表名: {nameof(BuildingConfig)}，配置id: {id}");
            return item;
        }

        public bool Contain(int id) => this.dict.ContainsKey(id);

        public Dictionary<int, BuildingConfig> GetAll() => this.dict;
    }

    public partial class BuildingConfig : ProtoObject, IConfig
    {
        /// <summary>Id</summary>
        public int Id { get; set; }
        /// <summary>建筑名称</summary>
        public string Name { get; set; }
        /// <summary>建筑类型 1仓库 2伐木场 3采石场 4农场 5村民小屋</summary>
        public int BuildingType { get; set; }
        /// <summary>最大工人数量(0=不需要工人)</summary>
        public int MaxWorkers { get; set; }
        /// <summary>建造消耗木材</summary>
        public int CostWood { get; set; }
        /// <summary>建造消耗石矿</summary>
        public int CostStone { get; set; }
        /// <summary>建造消耗食物</summary>
        public int CostFood { get; set; }
        /// <summary>建造耗时(毫秒)</summary>
        public int BuildTime { get; set; }
        /// <summary>预制体路径</summary>
        public string PrefabPath { get; set; }
        /// <summary>仓库容量(0=不是仓库)</summary>
        public int StorageCapacity { get; set; }
    }
}

