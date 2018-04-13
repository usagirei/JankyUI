using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using JankyUI.Nodes;
using JankyUI.Nodes.Binding;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JankyUI.Tests
{
    [TestClass]
    public class NodeHelperTests
    {
        private static AssemblyBuilder _ab;
        private static ModuleBuilder _mb;
        private static TypeBuilder _tb;

        private static JankyNodeContext _ctx;

        public enum EnumTest
        {
            value0,
            Value1,
            VALUE2,
        }

        public class DataContext
        {
            public float BindFloatProp { get; set; }
            public float BindFloatField;
            public static float BindFloatPropStatic { get; set; }
            public static float BindFloatFieldStatic;

            public int BindIntProp { get; set; }
            public int BindIntField;

            public EnumTest BindEnumProp { get; set; }
            public EnumTest BindEnumField;
        }

        [ClassInitialize]
        public static void Setup(TestContext ctx)
        {
            AssemblyName name = new AssemblyName("JankyUI.Tests");
            _ab = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);
            _mb = _ab.DefineDynamicModule("JankyUI.Tests.NodeHelper", "JankyUI.Tests.Dynamic.dll");

            var dc = new DataContext()
            {
                BindFloatProp = 123.45f,
                BindFloatField = 678.9f,

                BindIntProp = 42,
                BindIntField = -42,

                BindEnumProp = EnumTest.Value1,
                BindEnumField = EnumTest.Value1
            };

            DataContext.BindFloatFieldStatic = 100;
            DataContext.BindFloatPropStatic = 200;

            _ctx = new JankyNodeContext(dc);
            _ctx.Resources["RES_INT"] = 13;
            _ctx.Resources["RES_INT_STR"] = "-37";
            _ctx.Resources["RES_FLOAT"] = -66.6f;
            _ctx.Resources["RES_FLOAT_STR"] = "-66.6";
            _ctx.Resources["RES_ENUM"] = EnumTest.value0;
            _ctx.Resources["RES_ENUM_STR"] = "VaLuE2";
            _ctx.DataContextStack.Begin();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            _ctx.DataContextStack.End();
            _ab.Save("JankyUI.Tests.Dynamic.dll");
        }

        [DataTestMethod]
        [DataRow(typeof(Single), "1.5", 1.5f, false,                              DisplayName = "Test Float Inline Positive")]
        [DataRow(typeof(Single), "-1.5", -1.5f, false,                            DisplayName = "Test Float Inline Negative")]
        [DataRow(typeof(Single), "NaN", float.NaN, false,                         DisplayName = "Test Float Inline NaN")]
        [DataRow(typeof(Single), "#RES_FLOAT", -66.6f, false,                     DisplayName = "Test Float Value Resource")]
        [DataRow(typeof(Single), "#RES_FLOAT_STR", -66.6f, false,                 DisplayName = "Test Float String Resource")]
        [DataRow(typeof(Single), "@BindFloatProp", 123.45f, true,                 DisplayName = "Test Float Property Binding")]
        [DataRow(typeof(Single), "@BindFloatField", 678.9f, true,                 DisplayName = "Test Float Field Binding")]
        [DataRow(typeof(Single), "@BindFloatPropStatic", 200.0f, true,            DisplayName = "Test Float Static Property Binding")]
        [DataRow(typeof(Single), "@BindFloatFieldStatic", 100.0f, true,           DisplayName = "Test Float Static Field Binding")]
        //                                                                                                              
        [DataRow(typeof(Int32), "1", 1, false,                                    DisplayName = "Test Int Positive")]
        [DataRow(typeof(Int32), "-3", -3, false,                                  DisplayName = "Test Int Negative")]
        [DataRow(typeof(Int32), "#RES_INT", 13, false,                            DisplayName = "Test Int Value Resource")]
        [DataRow(typeof(Int32), "#RES_INT_STR", -37, false,                       DisplayName = "Test Int String Resource")]
        [DataRow(typeof(Int32), "@BindIntProp", 42, true,                         DisplayName = "Test Int Property Binding")]
        [DataRow(typeof(Int32), "@BindIntField", -42, true,                       DisplayName = "Test Int Field Binding")]
        //                                                                                                              
        [DataRow(typeof(EnumTest), "VALUE0", EnumTest.value0, false,              DisplayName = "Test Enum Inline #1")]
        [DataRow(typeof(EnumTest), "vAlUe1", EnumTest.Value1, false,              DisplayName = "Test Enum Inline #2")]
        [DataRow(typeof(EnumTest), "value2", EnumTest.VALUE2, false,              DisplayName = "Test Enum Inline #3")]
        [DataRow(typeof(EnumTest), "#RES_ENUM", EnumTest.value0, false,           DisplayName = "Test Enum Value Resource")]
        [DataRow(typeof(EnumTest), "#RES_ENUM_STR", EnumTest.VALUE2, false,       DisplayName = "Test Enum String Resource")]
        [DataRow(typeof(EnumTest), "@BindEnumProp", EnumTest.Value1, true,        DisplayName = "Test Enum Property Binding")]
        [DataRow(typeof(EnumTest), "@BindEnumField", EnumTest.Value1, true,       DisplayName = "Test Enum Field Binding")]
        public void TestPropertySetter(Type propType, string testData, object expected, bool skipNotJanky)
        {
            var classType = MakeNodeType(propType);

            var helper = (NodeHelper)typeof(NodeHelper<>).MakeGenericType(classType).GetProperty("Instance").GetValue(null);

            var node = helper.Activate(_ctx);
            helper.SetProperty(node, "jfield", testData);
            var actualProp = classType.GetField("JankField").GetValue(node);
            var actualValue = actualProp.GetType().GetProperty("Value").GetValue(actualProp);
            Assert.AreEqual(expected, actualValue);

            node = helper.Activate(_ctx);
            helper.SetProperty(node, "jprop", testData);
            actualProp = classType.GetProperty("JankProperty").GetValue(node);
            actualValue = actualProp.GetType().GetProperty("Value").GetValue(actualProp);
            Assert.AreEqual(expected, actualValue);

            if (!skipNotJanky)
            {
                node = helper.Activate(_ctx);
                helper.SetProperty(node, "field", testData);
                actualValue = classType.GetField("Field").GetValue(node);
                Assert.AreEqual(expected, actualValue);

                node = helper.Activate(_ctx);
                helper.SetProperty(node, "prop", testData);
                actualValue = classType.GetProperty("Property").GetValue(node);
                Assert.AreEqual(expected, actualValue);
            }
        }

        private Type MakeNodeType(Type elemType)
        {
            var typeName = "<MakeNodeType>_" + Regex.Replace(elemType.FullName, "[^a-zA-Z0-9_]", "_");
            var __builtClassType__ = _mb.GetType(typeName);

            if (__builtClassType__ == null)
            {
                var jankPropType = typeof(JankyProperty<>).MakeGenericType(elemType);
                _tb = _mb.DefineType(typeName, TypeAttributes.Class | TypeAttributes.Public, typeof(Node));
                var ctor = typeof(Attributes.JankyPropertyAttribute).GetConstructor(new[] { typeof(string), typeof(string) });
                _tb.SetCustomAttribute(new CustomAttributeBuilder(ctor, new[] { "jfield", "JankField" }));
                _tb.SetCustomAttribute(new CustomAttributeBuilder(ctor, new[] { "jprop", "JankProperty" }));
                _tb.SetCustomAttribute(new CustomAttributeBuilder(ctor, new[] { "field", "Field" }));
                _tb.SetCustomAttribute(new CustomAttributeBuilder(ctor, new[] { "prop", "Property" }));

                var jankField = _tb.DefineField("JankField", jankPropType, FieldAttributes.Public);
                var jankProp = _tb.DefineProperty("JankProperty", PropertyAttributes.None, jankPropType, Type.EmptyTypes);

                var field = _tb.DefineField("Field", elemType, FieldAttributes.Public);
                var prop = _tb.DefineProperty("Property", PropertyAttributes.None, elemType, Type.EmptyTypes);

                var getter = _tb.DefineMethod("get_JankProperty", MethodAttributes.Public, jankPropType, Type.EmptyTypes);
                var gen = getter.GetILGenerator();
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, jankField);
                gen.Emit(OpCodes.Ret);

                var setter = _tb.DefineMethod("set_JankProperty", MethodAttributes.Public, typeof(void), new[] { jankPropType });
                gen = setter.GetILGenerator();
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Stfld, jankField);
                gen.Emit(OpCodes.Ret);

                jankProp.SetGetMethod(getter);
                jankProp.SetSetMethod(setter);

                getter = _tb.DefineMethod("get_Property", MethodAttributes.Public, elemType, Type.EmptyTypes);
                gen = getter.GetILGenerator();
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, field);
                gen.Emit(OpCodes.Ret);

                setter = _tb.DefineMethod("set_Property", MethodAttributes.Public, typeof(void), new[] { elemType });
                gen = setter.GetILGenerator();
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Stfld, field);
                gen.Emit(OpCodes.Ret);

                prop.SetGetMethod(getter);
                prop.SetSetMethod(setter);

                __builtClassType__ = _tb.CreateType();
            }

            return __builtClassType__;
        }
    }
}
