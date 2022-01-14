using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = CreatioAnalyzerExtension.Test.CSharpAnalyzerVerifier<
    CreatioAnalyzerExtension.IncorrectEsqComparsionValueAnalyzer>;

namespace CreatioAnalyzerExtension.Test
{
    [TestClass]
    public class IncorrectEsqComparsionTypeAnaylzerUnitTests
    {
        [TestMethod]
        public async Task TestFakeMessages()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestAnalyzer()
        {
            var test = @"
                using Terrasoft.Core.Entities;
                using System;
                using System.Linq;
                using System.Collections.Generic;
                namespace ConsoleApplication
                {
                    public class TestClass
                    {
                        public void TestMethod()
                        {
                            var esq = new EntitySchemaQuery();
                            var guidsList = new List<Guid>();
                            var guidsEnumerable = (IEnumerable<Guid>)new List<Guid>();
                            var guidsArray = new Guid[0];
                            esq.Filters.Add({|IncorrectEsqComparsionValue:esq.CreateFilterWithParameters(FilterComparisonType.Equal, ""Test"", guidsList)|});
                            esq.Filters.Add({|IncorrectEsqComparsionValue:esq.CreateFilterWithParameters(FilterComparisonType.Equal, ""Test"", guidsEnumerable)|});
                            esq.Filters.Add({|IncorrectEsqComparsionValue:esq.CreateFilterWithParameters(FilterComparisonType.Equal, ""Test"", guidsArray)|});
                            esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal,""Test"",guidsList.Select(x => x.ToString())));
                        }
                    }
                }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }
    }
}


