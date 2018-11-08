using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;
using XData.Common.Fast;
using XData.Extentions;
using XData.Meta;

namespace Test
{
    public class TestFast
    {
        [Test, Order(0)]
        public void Test()
        {
            var meta = TableMeta.From<Model>();
            var con1 = meta.Type.GetDefaultCtor();
            var model1 = con1.FastCreate();
            foreach (var column in meta.Columns)
            {
                if (column.Type == typeof(string))
                    column.SetValue(model1, "test");
                if (column.Type == typeof(int))
                    column.SetValue(model1, 200);

                var v = column.GetValue(model1);
                Console.WriteLine(v);
            }

            var con2 = meta.Type.GetConstructor(new Type[] { typeof(string) });
            var model2 = con2.FastCreate("测试");
            var con3 = meta.Type.GetConstructor(new Type[] { typeof(int) });
            var model3 = con3.FastCreate(35);

            var staticProp = meta.Type.GetProperty("StaticName");
            var staticField = meta.Type.GetField("StaticField");
            staticProp.FastSetValue(null, "Static");

            var p = staticProp.FastGetValue(null);
            Console.WriteLine(p);
            Model.StaticField = "tttt、///";
            staticField.FastSetValue(null, "StaticField");
            var f = staticField.FastGetValue(null);
            Console.WriteLine(f);
        }
    }


    public class Model
    {
        public string Name { get; set; }
        public static string StaticName { get; set; }
        public int Field;
        public static string StaticField;

        public Model(int field)
        {
            Field = field;
        }
        public Model(string name)
        {
            Name = name;
        }

        public Model()
        {

        }
    }

    public class TestGetSet
    {
        public string GetName(Model model)
        {
            return model.Name;
        }
        public string GetStaticName()
        {
            return Model.StaticName;
        }
        public int GetField(Model model)
        {
            return model.Field;
        }
        public string GetStaticField()
        {
            return Model.StaticField;
        }

        public void SetName(Model model, string name)
        {
            model.Name = name;
        }

        public void SetStaticName(string name)
        {
            Model.StaticName = name;
        }

        public void SetField(Model model, int name)
        {
            model.Field = name;
        }

        public void SetStaticField(string name)
        {
            Model.StaticField = name;
        }

        public object Test(string name)
        {
            return new { Name = name };
        }
    }
}