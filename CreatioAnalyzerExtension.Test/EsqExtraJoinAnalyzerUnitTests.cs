using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = CreatioAnalyzerExtension.Test.CSharpCodeFixVerifier<
    CreatioAnalyzerExtension.EsqExtraJoinAnalyzer,
    CreatioAnalyzerExtension.EsqFilterCodeFixProvider>;

namespace CreatioAnalyzerExtension.Test
{
    [TestClass]
    public class CreatioAnalyzerExtensionUnitTest
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task TestEsqFilterFake()
        {
            var test = @"";
           
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestEsqFilterCodeFix()
        {
            var test = @"
                using Terrasoft.Core.Entities;
                namespace ConsoleApplication
                {
                    public class TestClass
                    {
                        public void TestMethod()
                        {
                            var esq = new EntitySchemaQuery();
                            {|ExtraJoin:esq.CreateFilterWithParameters(FilterComparisonType.Equal,""Test.Id"",null)|};
                        }
                    }
                }";

            var fixtest = @"
                using Terrasoft.Core.Entities;
                namespace ConsoleApplication
                {
                    public class TestClass
                    {
                        public void TestMethod()
                        {
                            var esq = new EntitySchemaQuery();
                            esq.CreateFilterWithParameters(FilterComparisonType.Equal, ""Test"", null);
                        }
                    }
                }";
            await VerifyCS.VerifyCodeFixAsync(test, fixtest);
        }
    }
}
