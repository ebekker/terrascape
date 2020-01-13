using System;
using System.Collections.Generic;
using HC.TFPlugin;
using Newtonsoft.Json;
using Terrascape.AcmeProvider;
using Xunit;

namespace HC.TFPlugin.XUTests
{
    public class ComputedValueTests
    {
        [Fact]
        public void TestUnknownStringValue()
        {
            var computedString = new Computed<string>();
            Assert.False(computedString.IsKnown);
            Assert.Throws<Exception>(() => computedString.Value);
            Assert.Null(computedString.ValueOrDefault());
            Assert.Equal("noval", computedString.ValueOrDefault("noval"));
            Assert.Null((string)computedString);
        }

        [Fact]
        public void TestUnknownIntValue()
        {
            var computedInt = new Computed<int>();
            Assert.False(computedInt.IsKnown);
            Assert.Throws<Exception>(() => computedInt.Value);
            Assert.Equal(0, computedInt.ValueOrDefault());
            Assert.Equal(99, computedInt.ValueOrDefault(99));
            Assert.Equal(0, (int)computedInt);
        }

        [Fact]
        public void TestKnownStringValue()
        {
            var computedString = Computed.Create("FOO");
            Assert.True(computedString.IsKnown);
            Assert.Equal("FOO", computedString.Value);
            Assert.Equal("FOO", computedString.ValueOrDefault());
            Assert.Equal("FOO", computedString.ValueOrDefault("noval"));
            Assert.Equal("FOO", computedString);
        }

        [Fact]
        public void TestKnownIntValue()
        {
            var computedInt = Computed.Create(88);
            Assert.True(computedInt.IsKnown);
            Assert.Equal(88, computedInt.Value);
            Assert.Equal(88, computedInt.ValueOrDefault());
            Assert.Equal(88, computedInt.ValueOrDefault(99));
            Assert.Equal<int>(88, computedInt);
        }

        [Fact]
        public void TestStringAssignment()
        {
            Computed<string> computedString;

            computedString = null;

            Assert.True(computedString.IsKnown);
            Assert.Null(computedString.Value);

            computedString = "FOO";

            Assert.True(computedString.IsKnown);
            Assert.Equal("FOO", computedString.Value);

            computedString = Computed<string>.Unknown;

            Assert.False(computedString.IsKnown);
            Assert.Throws<Exception>(() => computedString.Value);
        }

        // [Fact]
        // public void TestJsonSerialization()
        // {
        //     var json = JsonConvert.SerializeObject(new Test1());

        //     Assert.Equal("", json);
        // }

        // public class Test1
        // {
        //     public string String1 { get; set; } = "Value1";

        //     public int Int1 { get; set; } = 99;

        //     public Computed<string> String2 { get; set; } = "Value2";

        //     public Computed<string> String3 { get; set; }
        // }
    }
}
