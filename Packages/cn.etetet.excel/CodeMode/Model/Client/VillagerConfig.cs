using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class VillagerConfigCategory : Singleton<VillagerConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, VillagerConfig> dict = new();
		
        public void Merge(object o)
        {
            VillagerConfigCategory s = o as VillagerConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public VillagerConfig Get(int id)
        {
            this.dict.TryGetValue(id, out VillagerConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (VillagerConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, VillagerConfig> GetAll()
        {
            return this.dict;
        }

        public VillagerConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            
            var enumerator = this.dict.Values.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current; 
        }
    }

	public partial class VillagerConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>名字</summary>
		public string Name { get; set; }
		/// <summary>等级</summary>
		public int Level { get; set; }
		/// <summary>预制体路径</summary>
		public string PrefabPath { get; set; }

	}
}
