using System;
using System.Collections.Generic;
using HC.TFPlugin;
using Xunit;

namespace HC.TFPlugin.XUTests
{
    public class TypeMapperTests
    {
        [Fact]
        public void TestSchemaTypeMapper()
        {
            Assert.Equal(TypeMapper.TypeBool, TypeMapper.From(typeof(bool)));
        }

        [Fact]
        public void TestSchemaTypeMapperSimple()
        {
            Assert.Equal(TypeMapper.TypeBool, TypeMapper.From(typeof(bool)));
            Assert.Equal(TypeMapper.TypeInt, TypeMapper.From(typeof(int)));
            Assert.Equal(TypeMapper.TypeFloat, TypeMapper.From(typeof(float)));
            Assert.Equal(TypeMapper.TypeFloat, TypeMapper.From(typeof(double)));
            Assert.Equal(TypeMapper.TypeString, TypeMapper.From(typeof(string)));
            
            Assert.Throws<NotSupportedException>(
                () => Assert.Equal(TypeMapper.TypeString, TypeMapper.From(typeof(System.Uri)))
            );
        }

        [Fact]
        public void TestSchemaTypeMapperList()
        {
            Assert.Equal(TypeMapper.TypeList(TypeMapper.TypeBool), TypeMapper.From(typeof(List<bool>)));
            Assert.Equal(TypeMapper.TypeList(TypeMapper.TypeInt), TypeMapper.From(typeof(List<int>)));
            Assert.Equal(TypeMapper.TypeList(TypeMapper.TypeFloat), TypeMapper.From(typeof(List<float>)));
            Assert.Equal(TypeMapper.TypeList(TypeMapper.TypeFloat), TypeMapper.From(typeof(List<double>)));
            Assert.Equal(TypeMapper.TypeList(TypeMapper.TypeString), TypeMapper.From(typeof(List<string>)));
            
            Assert.Throws<NotSupportedException>(
                () => Assert.Equal(TypeMapper.TypeList(TypeMapper.TypeString),
                    TypeMapper.From(typeof(List<System.Uri>)))
            );

            Assert.Equal(TypeMapper.TypeList(TypeMapper.TypeString), TypeMapper.From(typeof(IMyList)));
        }

        [Fact]
        public void TestSchemaTypeMapperMap()
        {
            Assert.Equal(TypeMapper.TypeMap(TypeMapper.TypeBool), TypeMapper.From(typeof(Dictionary<string, bool>)));
            Assert.Equal(TypeMapper.TypeMap(TypeMapper.TypeInt), TypeMapper.From(typeof(Dictionary<string, int>)));
            Assert.Equal(TypeMapper.TypeMap(TypeMapper.TypeFloat), TypeMapper.From(typeof(Dictionary<string, float>)));
            Assert.Equal(TypeMapper.TypeMap(TypeMapper.TypeFloat), TypeMapper.From(typeof(Dictionary<string, double>)));
            Assert.Equal(TypeMapper.TypeMap(TypeMapper.TypeString), TypeMapper.From(typeof(Dictionary<string, string>)));
            
            Assert.Throws<NotSupportedException>(
                () => Assert.Equal(TypeMapper.TypeMap(TypeMapper.TypeString),
                    TypeMapper.From(typeof(Dictionary<string, System.Uri>)))
            );
            
            Assert.Throws<NotSupportedException>(
                () => Assert.Equal(TypeMapper.TypeMap(TypeMapper.TypeString),
                    TypeMapper.From(typeof(Dictionary<int, System.Uri>)))
            );
        }

        [Fact]
        public void TestSubclassofGenericTypeDefinition()
        {
            Assert.Equal(typeof(IList<string>),
                TypeMapper.GetSubclassOfGenericTypeDefinition(typeof(IList<>), typeof(List<string>)));
            Assert.Equal(typeof(IDictionary<string, decimal>),
                TypeMapper.GetSubclassOfGenericTypeDefinition(typeof(IDictionary<,>), typeof(Dictionary<string, decimal>)));

            Assert.Equal(typeof(IFaceA<decimal>),
                TypeMapper.GetSubclassOfGenericTypeDefinition(typeof(IFaceA<>), typeof(ClassA1<decimal>)));
            Assert.Equal(typeof(IFaceA<string>),
                TypeMapper.GetSubclassOfGenericTypeDefinition(typeof(IFaceA<>), typeof(ClassA2)));

            Assert.Equal(typeof(IFaceA<decimal>),
                TypeMapper.GetSubclassOfGenericTypeDefinition(typeof(IFaceA<>), typeof(ClassB1<decimal>)));
            Assert.Equal(typeof(IFaceA<string>),
                TypeMapper.GetSubclassOfGenericTypeDefinition(typeof(IFaceA<>), typeof(ClassB2)));
            Assert.Equal(typeof(IFaceA<long>),
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
