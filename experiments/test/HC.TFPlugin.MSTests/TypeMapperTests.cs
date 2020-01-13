using System;
using System.Collections.Generic;
using HC.TFPlugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HC.TFPlugin.MSTests
{
    [TestClass]
    public class TypeMapperTests
    {
        [TestMethod]
        public void TestSchemaTypeMapperSimple()
        {
            Assert.AreEqual(TypeMapper.TypeBool, TypeMapper.From(typeof(bool)));
            Assert.AreEqual(TypeMapper.TypeInt, TypeMapper.From(typeof(int)));
            Assert.AreEqual(TypeMapper.TypeFloat, TypeMapper.From(typeof(float)));
            Assert.AreEqual(TypeMapper.TypeFloat, TypeMapper.From(typeof(double)));
            Assert.AreEqual(TypeMapper.TypeString, TypeMapper.From(typeof(string)));
            
            Assert.ThrowsException<NotSupportedException>(
                () => Assert.AreEqual(TypeMapper.TypeString, TypeMapper.From(typeof(System.Uri)))
            );
        }

        [TestMethod]
        public void TestSchemaTypeMapperList()
        {
            Assert.AreEqual(TypeMapper.TypeList(TypeMapper.TypeBool), TypeMapper.From(typeof(List<bool>)));
            Assert.AreEqual(TypeMapper.TypeList(TypeMapper.TypeInt), TypeMapper.From(typeof(List<int>)));
            Assert.AreEqual(TypeMapper.TypeList(TypeMapper.TypeFloat), TypeMapper.From(typeof(List<float>)));
            Assert.AreEqual(TypeMapper.TypeList(TypeMapper.TypeFloat), TypeMapper.From(typeof(List<double>)));
            Assert.AreEqual(TypeMapper.TypeList(TypeMapper.TypeString), TypeMapper.From(typeof(List<string>)));
            
            Assert.ThrowsException<NotSupportedException>(
                () => Assert.AreEqual(TypeMapper.TypeList(TypeMapper.TypeString),
                    TypeMapper.From(typeof(List<System.Uri>)))
            );

            Assert.AreEqual(TypeMapper.TypeList(TypeMapper.TypeString), TypeMapper.From(typeof(IMyList)));
        }

        [TestMethod]
        public void TestSchemaTypeMapperMap()
        {
            Assert.AreEqual(TypeMapper.TypeMap(TypeMapper.TypeBool), TypeMapper.From(typeof(Dictionary<string, bool>)));
            Assert.AreEqual(TypeMapper.TypeMap(TypeMapper.TypeInt), TypeMapper.From(typeof(Dictionary<string, int>)));
            Assert.AreEqual(TypeMapper.TypeMap(TypeMapper.TypeFloat), TypeMapper.From(typeof(Dictionary<string, float>)));
            Assert.AreEqual(TypeMapper.TypeMap(TypeMapper.TypeFloat), TypeMapper.From(typeof(Dictionary<string, double>)));
            Assert.AreEqual(TypeMapper.TypeMap(TypeMapper.TypeString), TypeMapper.From(typeof(Dictionary<string, string>)));
            
            Assert.ThrowsException<NotSupportedException>(
                () => Assert.AreEqual(TypeMapper.TypeMap(TypeMapper.TypeString),
                    TypeMapper.From(typeof(Dictionary<string, System.Uri>)))
            );
            
            Assert.ThrowsException<NotSupportedException>(
                () => Assert.AreEqual(TypeMapper.TypeMap(TypeMapper.TypeString),
                    TypeMapper.From(typeof(Dictionary<int, System.Uri>)))
            );
        }

        [TestMethod]
        public void TestSubclassofGenericTypeDefinition()
        {
            Assert.AreEqual(typeof(IList<string>),
                TypeMapper.GetSubclassOfGenericTypeDefinition(typeof(IList<>), typeof(List<string>)));
            Assert.AreEqual(typeof(IDictionary<string, decimal>),
                TypeMapper.GetSubclassOfGenericTypeDefinition(typeof(IDictionary<,>), typeof(Dictionary<string, decimal>)));

            Assert.AreEqual(typeof(IFaceA<decimal>),
                TypeMapper.GetSubclassOfGenericTypeDefinition(typeof(IFaceA<>), typeof(ClassA1<decimal>)));
            Assert.AreEqual(typeof(IFaceA<string>),
                TypeMapper.GetSubclassOfGenericTypeDefinition(typeof(IFaceA<>), typeof(ClassA2)));

            Assert.AreEqual(typeof(IFaceA<decimal>),
                TypeMapper.GetSubclassOfGenericTypeDefinition(typeof(IFaceA<>), typeof(ClassB1<decimal>)));
            Assert.AreEqual(typeof(IFaceA<string>),
                TypeMapper.GetSubclassOfGenericTypeDefinition(typeof(IFaceA<>), typeof(ClassB2)));
            Assert.AreEqual(typeof(IFaceA<long>),
                TypeMapper.GetSubclassOfGenericTypeDefinition(typeof(IFaceA<>), typeof(ClassC1)));
        }
    }

    interface IMyList : IList<string> { }

    interface IFaceA<T> { }
    interface IFaceB<T> : IFaceA<T> { }
    interface IFaceC : IFaceB<long> { }
    class ClassA1<T> : IFaceA<T> { }
    class ClassA2 : IFaceA<string> { }
    class ClassB1<T> : IFaceB<T> { }
    class ClassB2 : IFaceB<string> { }
    class ClassC1 : IFaceC { }
}
