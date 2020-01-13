using System;
using System.Collections.Generic;
using HC.TFPlugin;
using Terrascape.AcmeProvider;
using Xunit;

namespace HC.TFPlugin.XUTests
{
    public class SchemaHelperTests
    {
        [Fact]
        public void TestProviderSchema()
        {
            var asm = typeof(AcmeProvider).Assembly;
            var schema = SchemaHelper.GetProviderSchema(asm);
        }

        [Fact]
        public void TestDataSourceSchemas()
        {
            var asm = typeof(AcmeProvider).Assembly;
            var schema = SchemaHelper.GetDataSourceSchemas(asm);
        }

        [Fact]
        public void TestResourceSchemas()
        {
            var asm = typeof(AcmeProvider).Assembly;
            var schemas = SchemaHelper.GetResourceSchemas(asm);
        }
    }
}
