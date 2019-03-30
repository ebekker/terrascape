using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Terrascape.LocalProvider;

namespace MsgPackSharp.CLI
{
    class Program
    {
        static byte[] bytes1 = new byte[] {
            138,166,97,112,112,101,110,100,144,168,99,104,101,99,107,115,
            117,109,192,176,99,111,109,112,117,116,101,95,99,104,101,99,
            107,115,117,109,163,109,100,53,167,99,111,110,116,101,110,116,
            217,39,84,104,105,115,32,105,115,32,99,111,110,116,101,110,116,
            32,102,114,111,109,32,105,110,108,105,110,101,32,84,70,32,99,111,
            110,102,105,103,46,10,174,99,111,110,116,101,110,116,95,98,97,115,
            101,54,52,192,172,99,111,110,116,101,110,116,95,112,97,116,104,192,
            171,99,111,110,116,101,110,116,95,117,114,108,192,169,102,117,108,108,
            95,112,97,116,104,192,173,108,97,115,116,95,109,111,100,105,102,105,101,
            100,192,164,112,97,116,104,171,46,47,116,101,115,116,49,46,116,120,116,
        };

        static byte[] bytes2 = new byte[] {
            138,166,97,112,112,101,110,100,145,133,176,99,111,109,112,117,
            116,101,95,99,104,101,99,107,115,117,109,192,167,99,111,110,116,
            101,110,116,188,10,32,42,32,97,112,112,101,110,100,105,110,103,32,
            109,121,32,112,117,98,108,105,99,32,73,80,58,10,174,99,111,110,116,
            101,110,116,95,98,97,115,101,54,52,192,172,99,111,110,116,101,110,116,
            95,112,97,116,104,192,171,99,111,110,116,101,110,116,95,117,114,108,
            192,168,99,104,101,99,107,115,117,109,192,176,99,111,109,112,117,116,
            101,95,99,104,101,99,107,115,117,109,163,109,100,53,167,99,111,110,116,
            101,110,116,217,39,84,104,105,115,32,105,115,32,99,111,110,116,101,110,
            116,32,102,114,111,109,32,105,110,108,105,110,101,32,84,70,32,99,111,110,
            102,105,103,46,10,174,99,111,110,116,101,110,116,95,98,97,115,101,54,52,
            192,172,99,111,110,116,101,110,116,95,112,97,116,104,192,171,99,111,110,
            116,101,110,116,95,117,114,108,192,169,102,117,108,108,95,112,97,116,104,
            192,173,108,97,115,116,95,109,111,100,105,102,105,101,100,192,164,112,97,
            116,104,171,46,47,116,101,115,116,49,46,116,120,116,
        };

        static byte[] bytes3 = new byte[] {
            137,168,99,104,101,99,107,115,117,109,192,176,99,111,109,112,117,
            116,101,95,99,104,101,99,107,115,117,109,163,109,100,53,167,99,
            111,110,116,101,110,116,217,39,84,104,105,115,32,105,115,32,99,111,
            110,116,101,110,116,32,102,114,111,109,32,105,110,108,105,110,101,32,
            84,70,32,99,111,110,102,105,103,46,10,174,99,111,110,116,101,110,116,
            95,98,97,115,101,54,52,192,172,99,111,110,116,101,110,116,95,112,97,116,
            104,192,171,99,111,110,116,101,110,116,95,117,114,108,192,169,102,117,108,
            108,95,112,97,116,104,192,173,108,97,115,116,95,109,111,100,105,102,105,101,
            100,192,164,112,97,116,104,171,46,47,116,101,115,116,49,46,116,120,116
        };

        static byte[] bytes4 = new byte[] {
            137,168,99,104,101,99,107,115,117,109,192,176,99,111,109,112,117,116,
            101,95,99,104,101,99,107,115,117,109,163,109,100,53,167,99,111,110,116,
            101,110,116,217,39,84,104,105,115,32,105,115,32,99,111,110,116,101,110,
            116,32,102,114,111,109,32,105,110,108,105,110,101,32,84,70,32,99,111,110,
            102,105,103,46,10,174,99,111,110,116,101,110,116,95,98,97,115,101,54,52,192,
            172,99,111,110,116,101,110,116,95,112,97,116,104,192,171,99,111,110,116,101,
            110,116,95,117,114,108,192,169,102,117,108,108,95,112,97,116,104,192,173,108,
            97,115,116,95,109,111,100,105,102,105,101,100,192,164,112,97,116,104,171,46,
            47,116,101,115,116,49,46,116,120,116,
        };
        static void Main(string[] args) => Main5();

        static void Main6()
        {
            var vals = new object[] {
                // byte.MinValue, byte.MaxValue,
                // sbyte.MinValue, sbyte.MaxValue,
                // int.MinValue, int.MaxValue,
                //uint.MinValue, uint.MaxValue,
            };

            foreach (var v in vals)
            {
                byte[] b = null;
                switch (v)
                {
                    case byte n: b = BitConverter.GetBytes((ushort)(byte)n); break;
                    case sbyte n: b = BitConverter.GetBytes((sbyte)n); break;
                    case int n: b = BitConverter.GetBytes((int)n); break;
                    case uint n: b = BitConverter.GetBytes((uint)n); break;
                }

                Console.WriteLine($"[{v}]:");
                Console.WriteLine($"  [{JsonConvert.SerializeObject(b.Select(x => (int)x))}]");
            }

        }

        static void Main5()
        {
            var vals = new object[] {
                -99, 99,
                sbyte.MinValue, sbyte.MaxValue,
                byte.MinValue, byte.MaxValue,

                short.MinValue, short.MaxValue,
                ushort.MinValue, ushort.MaxValue,

                int.MinValue, int.MaxValue,
                uint.MinValue, uint.MaxValue,

                long.MinValue, long.MaxValue,
                ulong.MinValue, (ulong.MaxValue / 2), // ulong.MaxValue,

                float.MinValue, float.MaxValue,
                double.MinValue, double.MaxValue,

                Encoding.UTF8.GetBytes("Hello World!"),
                "Goodbye World!",

                new[] {
                    1, 2, 3, 4,
                },
                new List<string> {
                    "FOO", "BAR", "NON",
                },
                new ArrayList {
                    1, true, "FOO", Encoding.UTF8.GetBytes("BAR"),
                },

                new MPExt(10, Encoding.UTF8.GetBytes("H")),
                new MPExt(10, Encoding.UTF8.GetBytes("HELO")),
                new MPExt(10, Encoding.UTF8.GetBytes("HELOWRLD")),
                new MPExt(10, Encoding.UTF8.GetBytes("G'DAYTOTHEWORLD!")),
                new MPExt(10, Encoding.UTF8.GetBytes("G'DAYTOTHEWORLD!")),

                new MPExt(10, Encoding.UTF8.GetBytes("HE")),
                new MPExt(10, Encoding.UTF8.GetBytes("HE LO")),
            };

            int ndx = 0;
            byte[] msBytes;
            using (var ms = new MemoryStream())
            {
                var ctx = MPConverterContext.CreateDefault();
                var mpw = new MPWriter(ms);
                foreach (var v in vals)
                {
                    Console.WriteLine($"[{ndx++,2}] Writing [{v}]:");
                    var mpo = ctx.Encode(v.GetType(), v);
                    mpw.Emit(mpo);
                }

                msBytes = ms.ToArray();
            }

            Console.WriteLine();
            Console.WriteLine($"Wrote [{msBytes.Length}] bytes");
            Console.WriteLine();

            ndx = 0;
            using (var ms = new MemoryStream(msBytes))
            {
                var mpo = MPReader.Parse(ms);
                while (mpo != null)
                {
                    Console.WriteLine($"[{ndx++,2}] Read [{mpo.Value.Type}=[{mpo.Value.Value}]");
                    if (mpo.Value.Type == MPType.Ext)
                    {
                        var ext = (MPExt)mpo.Value.Value;
                        Console.WriteLine($"  EXT [{ext.Type}]: [{Encoding.UTF8.GetString(ext.Data.ToArray())}]");
                    }

                    mpo = MPReader.Parse(ms);
                }
            }
        }

        static void Main1()
        {
            // Console.WriteLine(Formats.NegativeFixInt.Start.Value);
            // Console.WriteLine(Formats.NegativeFixInt.End.Value);
            // Console.WriteLine(MsgPack.Deserialize(new byte[] {
            //     (byte)Formats.NegativeFixInt.Start.Value,
            // }));
            // Console.WriteLine(MsgPack.Deserialize(new byte[] {
            //     (byte)Formats.NegativeFixInt.End.Value,
            // }));

            object o;
            var bytes = bytes4;

            using (var stream = new MemoryStream(bytes))
            {
                while ((o = MPReader.ParseRaw(stream)) != null)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
                }
            }

            using (var stream = new MemoryStream(bytes))
            {
                while ((o = MPReader.Parse(stream)) != null)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
                }
            }
        }

        static void Main2()
        {
            var ctx = MPConverterContext.CreateDefault();

            (Type type, object value) Make<T>(T val) => (typeof(T), val);

            var vals = new[] {
                // Boolean
                Make(false),
                Make(true),
                Make((bool?)null),
                Make((bool?)true),
                Make((bool?)false),

                // Integer
                Make(byte.MinValue),    Make((byte?)byte.MinValue),
                Make(byte.MaxValue),    Make((byte?)byte.MaxValue),
                Make(sbyte.MinValue),   Make((sbyte?)sbyte.MinValue),
                Make(sbyte.MaxValue),   Make((sbyte?)sbyte.MaxValue),
                Make( short.MinValue),  Make( (short?)short.MinValue),
                Make( short.MaxValue),  Make( (short?)short.MaxValue),
                Make(ushort.MinValue),  Make((ushort?)ushort.MinValue),
                Make(ushort.MaxValue),  Make((ushort?)ushort.MaxValue),
                Make( int.MinValue),    Make( (int?)int.MinValue),
                Make( int.MaxValue),    Make( (int?)int.MaxValue),
                Make(uint.MinValue),    Make((uint?)uint.MinValue),
                Make(uint.MaxValue),    Make((uint?)uint.MaxValue),
                Make( long.MinValue),   Make( (long?)long.MinValue),
                Make( long.MaxValue),   Make( (long?)long.MaxValue),
                Make(ulong.MinValue),   Make((ulong?)ulong.MinValue),
                Make(ulong.MaxValue),   Make((ulong?)ulong.MaxValue),
                Make( char.MinValue),   Make( (char?)char.MinValue),
                Make( char.MaxValue),   Make( (char?)char.MaxValue),

                // Float
                Make(float.MinValue),   Make((float?)float.MinValue),
                Make(float.MaxValue),   Make((float?)float.MaxValue),
                Make(double.MinValue),  Make((double?)double.MinValue),
                Make(double.MaxValue),  Make((double?)double.MaxValue),
                Make(decimal.MinValue), Make((decimal?)decimal.MinValue),
                Make(decimal.MaxValue), Make((decimal?)decimal.MaxValue),

                // Binary
                Make(Encoding.UTF8.GetBytes("ABCDEFG")),

                // String
                Make("abcdefg"),
            };

            foreach (var tv in vals)
            {
                Console.WriteLine($"[{tv.type.Name,-15}]");
                Console.WriteLine($"  Original [{tv.value,45}]:");
                var encoded = ctx.Encode(tv.type, tv.value);
                Console.WriteLine($"  Encoded: [{encoded,45}]");
                var decoded = ctx.Decode(tv.type, encoded);
                Console.WriteLine($"  Decoded: [{decoded,45}]");

                if (!object.Equals(tv.value, decoded))
                    throw new Exception("encoded->decoded does not match original value");
            }
        }

        static void Main3()
        {
            var ctx = MPConverterContext.CreateDefault();

            (Type type, object value) Make<T>(T val) => (typeof(T), val);

            var vals = new[] {
                // Make(new[] { 1, 2, 3, 4, }),
                // Make(new object[] { 5, 6, 7, 8, }),
                // Make(new List<int> { 3, 4, 5, 6, }),
                // Make(new ArrayList { 1, 2, 7, 8, }),
                // Make(new Collection<object> { 'a', 'b', 'c', }),
                // Make(new Collection<object> { 'a', 1, false, }),

                Make(new Dictionary<string, string> {
                    ["a"] = "A",
                    ["b"] = "B",
                    ["c"] = "C",
                }),
                Make(new Hashtable {
                    ["x"] = "X",
                    ["y"] = "Y",
                    ["z"] = "Z",
                }),
                Make(new Dictionary<string, object> {
                    ["p1"] = 1,
                    ["p2"] = true,
                    ["p3"] = "ABC",
                    ["p4"] = Encoding.UTF8.GetBytes("ABC"),
                    ["p5"] = new[] { 1, 2, 3, 4, },
                }),

                Make(new Sample1 {
                }),
            };

            foreach (var tv in vals)
            {
                Console.WriteLine($"[{tv.type.Name,-15}]");
                Console.WriteLine($"    Original [{JsonConvert.SerializeObject(tv.value),45}]:");
                var encoded = ctx.Encode(tv.type, tv.value);
                Console.WriteLine($"    Encoded: [{JsonConvert.SerializeObject(encoded),45}]");
                var decoded = ctx.Decode(tv.type, encoded);
                Console.WriteLine($"    Decoded: [{JsonConvert.SerializeObject(decoded),45}]");
                Console.WriteLine($"    Rebuilt: [{JsonConvert.SerializeObject(tv.value),45}]:");
            }
        }

        static void Main4()
        {
            var ctx = MPConverterContext.CreateDefault(
                new [] { new Converters.ObjectConverter(new MyNameResolver()) }, true);

            // ctx = new MPConverterContext
            // {
            //     Converters = {
            //         Converters.BasicConverter.Instance,
            //         new Converters.CommonConverter(true, true),
            //         Converters.MapConverter.Instance,
            //         Converters.ArrayConverter.Instance,
            //         new Converters.ObjectConverter(new MyNameResolver()),
            //         Converters.DefaultConverter.Instance,
            //     },
            // };


            (Type type, object value) Make<T>(T val) => (typeof(T), val);

            var vals = new[] {
                Make(new Sample1 {
                }),
            };

            foreach (var tv in vals)
            {
                Console.WriteLine($"[{tv.type.Name,-15}]");
                Console.WriteLine($"    Original [{JsonConvert.SerializeObject(tv.value),45}]:");
                var encoded = ctx.Encode(tv.type, tv.value);
                Console.WriteLine($"    Encoded: [{JsonConvert.SerializeObject(encoded),45}]");
                var decoded = ctx.Decode(tv.type, encoded);
                Console.WriteLine($"    Decoded: [{JsonConvert.SerializeObject(decoded),45}]");
                Console.WriteLine($"    Rebuilt: [{JsonConvert.SerializeObject(tv.value),45}]:");

                Console.WriteLine();
                Console.WriteLine(JsonConvert.SerializeObject(new {
                    type = tv.type.Name,
                    orig = tv.value,
                    enco = encoded,
                    deco = decoded,
                }));
            }
        }

        public class MyNameResolver : Converters.ObjectConverter.IPropertyNamesResolver
        {
            public IReadOnlyDictionary<string, PropertyInfo> ResolvePropertyNames(Type type)
            {
                var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                string Snake(string name)
                {
                    var newName = new List<char>();
                    for (int i = 0; i < name.Length; ++i)
                    {
                        var c = name[i];
                        if (char.IsUpper(c))
                        {
                            if (i > 0 && !char.IsUpper(name[i - 1]))
                                newName.Add('_');
                            newName.Add(char.ToLower(c));
                        }
                        else if (char.IsDigit(c))
                        {
                            if (i > 0 && !char.IsDigit(name[i - 1]))
                                newName.Add('_');
                            newName.Add(c);
                        }
                        else
                        {
                            newName.Add(c);
                        }

                    }
                    return new String(newName.ToArray());
                }

                return props.ToDictionary(p => Snake(p.Name), p => p);
            }
        }

        public class Sample1
        {
            public int MinInt { get; set; } = int.MinValue;
            public int MaxInt { get; set; } = int.MaxValue;

            public uint MinUint { get; set; } = uint.MinValue;
            public uint MaxUint { get; set; } = uint.MaxValue;

            public string String { get; set; } = "Hello World!";

            public byte[] Binary { get; set; } = Encoding.UTF8.GetBytes("Hello World!");

            public Sample1Sub1 Nested1 { get; set; } = new Sample1Sub1();

            public Sample1Sub1 NestedNull1 { get; set; }

        }

        public class Sample1Sub1
        {
            public List<string> Values { get; set; } = new List<string> {
                "Foo", "Bar", "Non",
            };

            public MPType SampleEnum1 { get; set; } = MPType.Integer;
            public MPType SampleEnum2 { get; set; } = MPType.Array;
            public MPType SampleEnum3 { get; set; } = MPType.Map;

            public Guid EmptyGuid { get; set; } = Guid.Empty;

            public Guid RandomGuid { get; set; } = Guid.NewGuid();
        }
    }
}
