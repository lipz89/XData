using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        [Test]
        public void AddDictionary()
        {
            var id = Guid.Parse("E6E3763F-A8DF-491A-8CAC-191B228D7624");

            var db = Program.NewContext();
            var dic = db.GetByKey<Dictionary>(id);
            if (dic != null)
            {
                db.Delete<Dictionary>(dic).Execute();
            }
            dic = new Dictionary
            {
                ID = id,
                Name = "行政区域",
                Code = "Area"
            };
            db.Insert<Dictionary>(dic).Execute();
        }
        [Test]
        public void AddDetails()
        {
            var details = LoadDetails();
            details = DealDetails(details);
            var db = Program.NewContext();

            db.Delete<Model>().Execute();

            foreach (var detail in details)
            {
                db.Insert<Model>(detail).Execute();
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
                var sub = details.Where(x => x.pcode == model.code).OrderBy(x => x.code).ToList();

                foreach (var s in sub)
                {
                    s.IndexID = indexId++;
                    s.ParentID = model.ID;
                    result.Add(s);
                }

                foreach (var s in sub)
                {
                    var subsub = details.Where(x => x.pcode == s.code).OrderBy(x => x.code).ToList();
                    foreach (var ss in subsub)
                    {
                        ss.IndexID = indexId++;
                        ss.ParentID = model.ID;
                        result.Add(ss);
                    }
                }
            }

            result.ForEach(x => x.DictionaryID = id);
            return result.OrderBy(x => x.IndexID).ToList();
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
}