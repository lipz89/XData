﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

using NUnit.Framework;

using Winning.SPD.SCM.Domain;

using XData.Meta;

namespace Test
{
    public class TestArea
    {
        public TestArea()
        {
            MapperConfig.HasTableName<Dictionary>("Dictionary2");
            MapperConfig.HasKey<Dictionary>(x => x.ID);
            MapperConfig.IgnoreColumn<Dictionary>(x => x.RowVersion);

            MapperConfig.HasTableName<Model>("DictionaryDetail2");
            MapperConfig.HasKey<Model>(x => x.ID);
            MapperConfig.HasColumnName<Model>(x => x.code, "Code");
            MapperConfig.HasColumnName<Model>(x => x.address, "Name");
            MapperConfig.HasColumnName<Model>(x => x.memo, "Memo");
            MapperConfig.IgnoreColumn<Model>(x => x.pcode);
        }
        [Test, Order(1)]
        public void AddDictionary()
        {
            var id = Guid.Parse("E6E3763F-A8DF-491A-8CAC-191B228D7624");

            var db = Program.NewContext();
            var dic = db.GetByKey<Dictionary>(id);
            if (dic != null)
            {
                db.Delete<Model>(null);
                db.Delete<Dictionary>(dic);
            }
            dic = new Dictionary
            {
                ID = id,
                Name = "行政区域",
                Code = "Area"
            };
            db.Insert<Dictionary>(dic);
        }
        [Test, Order(2)]
        public void AddDetails()
        {
            var details = LoadDetails();
            details = DealDetails(details);
            var db = Program.NewContext();

            db.Delete<Model>(null);

            foreach (var detail in details)
            {
                db.Insert<Model>(detail);
            }

            var count = db.Query<Model>().Count();
            if (count == details.Count)
            {
                Console.WriteLine("插入{0}条成功。", count);
            }
            else
            {
                Console.WriteLine("预计插入{0}条，实际插入{1}条", details.Count, count);
            }
        }

        public List<Model> LoadDetails()
        {
            var str = string.Empty;
            var file = @"D:\sqlscripts\地区代码\地区.json";
            using (var reader = new StreamReader(file))
            {
                str = reader.ReadToEnd();
            }
            var details = JsonConvert.DeserializeObject<List<Model>>(str);

            return details;
        }

        private List<Model> DealDetails(List<Model> details)
        {
            var id = Guid.Parse("E6E3763F-A8DF-491A-8CAC-191B228D7624");

            var area86 = details.Where(x => x.memo != null).OrderBy(x => x.code);

            var result = new List<Model>();
            var indexId = 1;
            foreach (var model in area86)
            {
                model.IndexID = indexId++;
                result.Add(model);
            }

            foreach (var model in area86)
            {
                var subs = details.Where(x => x.pcode == model.code).OrderBy(x => x.code).ToList();

                foreach (var sub in subs)
                {
                    sub.IndexID = indexId++;
                    sub.ParentID = model.ID;
                    result.Add(sub);
                }

                foreach (var sub in subs)
                {
                    var subsub = details.Where(x => x.pcode == sub.code).OrderBy(x => x.code).ToList();
                    foreach (var ss in subsub)
                    {
                        ss.IndexID = indexId++;
                        ss.ParentID = sub.ID;
                        result.Add(ss);
                    }
                }
            }

            result.ForEach(x => x.DictionaryID = id);
            return result.OrderBy(x => x.IndexID).ToList();
        }

        [Test, Order(3)]
        public void TestToJson()
        {
            var id = Guid.Parse("E6E3763F-A8DF-491A-8CAC-191B228D7624");
            var db = Program.NewContext();
            var st = Stopwatch.StartNew();
            var details = db.Query<Model>().Where(x => x.DictionaryID == id).ToList();

            var time = st.ElapsedMilliseconds / 1000.0;
            Console.WriteLine("查询{0}条数据，花费时间{1}秒", details.Count, time);

            var area86 = details.Where(x => x.ParentID == null).OrderBy(x => x.code).ToList();

            var dic = new Dictionary<string, object>();
            var page = GetPage().ToDictionary(x => x.Key, x => area86.Where(x.Value)
                .OrderBy(i => i.memo)
                .Select(i => new { i.code, i.address }).ToList());
            dic.Add("86", page);

            var subs = details.Except(area86).ToList();
            var seconds = new Dictionary<Guid, string>();

            foreach (var area in area86)
            {
                var sub = subs.Where(x => x.ParentID == area.ID).ToList();
                if (sub.Any())
                {
                    var d = new Dictionary<string, object>();
                    foreach (var model in sub)
                    {
                        seconds.Add(model.ID, model.code);
                        d.Add(model.code, model.address);
                    }
                    dic.Add(area.code, d);
                }
            }

            foreach (var code in seconds)
            {
                var sub = subs.Where(x => x.ParentID == code.Key).ToList();
                if (sub.Any())
                {
                    var d = new Dictionary<string, object>();
                    foreach (var model in sub)
                    {
                        d.Add(model.code, model.address);
                    }
                    dic.Add(code.Value, d);
                }
            }

            var json = JsonConvert.SerializeObject(dic);
            var reg = new Regex("\"\\d+\":");
            var rst = reg.Replace(json, (x) => x.Value.Replace("\"", ""));

        }

        private Dictionary<string, Func<Model, bool>> GetPage()
        {
            return new Dictionary<string, Func<Model, bool>>
            {
                {"A-G",x=>"ABCDEFG".Contains( x.memo) },
                {"H-K",x=>"HIJK".Contains( x.memo) },
                {"L-S",x=>"LMNOPQRS".Contains( x.memo) },
                {"T-Z",x=>"TUVWXYZ".Contains( x.memo) },
            };
        }
    }

    public class Model
    {
        public Model()
        {
            ID = Guid.NewGuid();
            DictionaryID = Guid.Parse("E6E3763F-A8DF-491A-8CAC-191B228D7624");
        }
        public Guid ID { get; set; }
        public string code { get; set; }
        public string address { get; set; }
        public string pcode { get; set; }
        public string memo { get; set; }
        public Guid? DictionaryID { get; set; }
        public int IndexID { get; set; }
        public bool IsSys { get; set; }
        public Guid? ParentID { get; set; }
        public string Status { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class City86
    {
        public string code { get; set; }
        public string address { get; set; }
    }
}