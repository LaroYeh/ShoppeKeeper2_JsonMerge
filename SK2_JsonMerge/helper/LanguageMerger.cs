﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SK2_JsonMerge.helper
{
    class LanguageMerger
    {
        public string RefPath { get; private set; }
        public string LocalPath { get; private set; }

        public LanguageMerger(string refPath, string localPath)
        {
            //DirPath + Filename +  ext
            RefPath = refPath;
            LocalPath = localPath;
        }
        public List<LocalText> GetMergePriview()
        {
            //Deserialize Json
            List<JsonObject> refObj = DeserializeFile(RefPath);
            List<JsonObject> localObj = DeserializeFile(LocalPath);
            List<ExtraObject> extraObj = GetAdditionalObject(refObj, localObj);
            List<LocalText> datasource = new List<LocalText>();

            //Merge them
            var data = (from r in refObj
                        join l in localObj on r.Name equals l.Name into _l
                        from l in _l.DefaultIfEmpty()

                        select new LocalText
                        {
                            Id = r.Id,
                            Name = r.Name,
                            Value = r.Value,
                            LocalValue = (l != null) ? l.Value : string.Empty
                        })
           .Union(from extra in extraObj
                  select new LocalText
                  {
                      Id = Convert.ToSingle(string.Format("{0}.{1}", refObj.Where(p => p.Name == extra.PrevName).First().Id, extra.Id)),
                      Name = extra.Name,
                      Value = "(Localization Only)",
                      LocalValue = extra.Value
                  })
                   .OrderBy(p => p.Id);
            return data.ToList(); ;
        }

        private List<JsonObject> DeserializeFile(string path)
        {
            List<JsonObject> data = new List<JsonObject>();

            //Reference: https://stackoverflow.com/questions/13652983/dynamic-jcontainer-json-net-iterate-over-properties-at-runtime
            int index = 0;
            string NameTxt = File.ReadAllText(path).Replace(Environment.NewLine, string.Empty).Replace("\t", string.Empty);
            dynamic dynObj_name = JsonConvert.DeserializeObject(NameTxt);

            //JContainer is the base class
            foreach (JToken token in (dynObj_name as JObject).Children())
            {
                index++;
                if (token is JProperty)
                {
                    var prop = token as JProperty;
                    data.Add(new JsonObject() { Id = index, Name = prop.Name, Value = prop.Value.ToString() });
                }
            }

            return data;
        }
        /// <summary>
        /// 取得LocalFile比RefFile還多的部分
        /// </summary>
        /// <param name="refObj"></param>
        /// <param name="localObj"></param>
        /// <returns></returns>
        private List<ExtraObject> GetAdditionalObject(List<JsonObject> refObj, List<JsonObject> localObj)
        {
            List<ExtraObject> ExtraObj = new List<ExtraObject>();

            int index = 0;
            var names = refObj.Select(p => p.Name);
            var loc_names = localObj.Select(p => p.Name);
            var extra_names = loc_names.Except(names);

            foreach (var name in extra_names)
            {
                index++;
                var locObj = localObj.Where(p => p.Name == name).First();
                ExtraObj.Add(new ExtraObject()
                {
                    Id = index,
                    Name = locObj.Name,
                    Value = locObj.Value,
                    PrevName = GetPrevName(localObj, locObj.Name, extra_names.ToArray()),
                });
            }

            return ExtraObj;
        }

        #region Sub Function
        /// <summary>
        /// 取得最後一次有重複的屬性名(可能連續兩個都沒有，所以用遞迴處理)
        /// 如果Reference少很多，效能會超級拖，還在想辦法節省步驟
        /// </summary>
        /// <param name="localObj">尋找的母體</param>
        /// <param name="name">額外的屬性名</param>
        /// <param name="extraName">所有的格外名</param>
        /// <returns></returns>
        private string GetPrevName(List<JsonObject> localObj, string name, string[] extraName)
        {
            var preID = (localObj.Where(p => p.Name == name).FirstOrDefault().Id) - 1;
            var preName = localObj.Where(p => p.Id == preID).First().Name;
            if (extraName.Contains(preName))
            {
                //上一個也是參照來源沒有的
                return GetPrevName(localObj, preName, extraName);
            }
            else
            {
                //上一個參照來源有
                return preName;
            }
        }

        #endregion
    }

    #region Object
    public class JsonObject
    {
        public float Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
    public class ExtraObject : JsonObject
    {
        public string PrevName { get; set; } //上一個跟比較物有重複的名字
    }
    public class LocalText : JsonObject
    {
        //public float Id { get; set; }
        //public string Name { get; set; }
        //public string Value { get; set; }
        public string LocalValue { get; set; }
    }
    #endregion

}