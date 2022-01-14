using CreatioAnalyzerExtension.Constants;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace CreatioAnalyzerExtension
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class IncorrectEsqComparsionValueAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor FilterRule =
            new DiagnosticDescriptor(
                DiagnosticId.IncorrectEsqComparsionValue,
                new LocalizableResourceString(nameof(Resources.IncorrectComparsionTitle), Resources.ResourceManager, typeof(Resources)).ToString(),
                new LocalizableResourceString(nameof(Resources.IncorrrectComparsionMessageFormat), Resources.ResourceManager, typeof(Resources)).ToString(),
                "Naming",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: new LocalizableResourceString(nameof(Resources.IncorrectComparsionDescription), Resources.ResourceManager, typeof(Resources)).ToString());

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(FilterRule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeFilter, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeFilter(SyntaxNodeAnalysisContext context)
        {
            var node = (InvocationExpressionSyntax) context.Node;

            if (!(node.Expression is MemberAccessExpressionSyntax filterAccess) ||
                filterAccess.Name.Identifier.Text != "CreateFilterWithParameters" ||
                node.ArgumentList.Arguments.Count != 3 || 
                context.Compilation == null)
            {
                return;
            }

            //TODO: compare by interfaces
            var incorrectTypes = GetIncorrectTypes(context.SemanticModel.Compilation);
            var comparsionType = context.SemanticModel.GetTypeInfo(node.ArgumentList.Arguments[2].Expression).Type;

            if (comparsionType != null)
            {
                var isExists = incorrectTypes.TryGetValue(comparsionType, out var possibleType);

                if (isExists)
                {
                    var diagnostic = Diagnostic.Create(FilterRule, context.Node.GetLocation(), comparsionType.ToString(),
                        possibleType.ToString());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private Dictionary<ITypeSymbol, ITypeSymbol> GetIncorrectTypes(Compilation compilation)
        {
            var genericTypes = new string[] 
            { 
                "System.Collections.Generic.IEnumerable`1", 
                "System.Collections.Generic.List`1",
                "System.Collections.Generic.IList`1"
            };

            var result = new Dictionary<ITypeSymbol, ITypeSymbol>();
            var guidType = compilation.GetTypeByMetadataName("System.Guid");
            var stringType = compilation.GetTypeByMetadataName("System.String");

            foreach (var genericType in genericTypes)
            {
                var genericTypeMetadata = compilation.GetTypeByMetadataName(genericType);
                result.Add(genericTypeMetadata.Construct(guidType), genericTypeMetadata.Construct(stringType));
            }

            result.Add(compilation.CreateArrayTypeSymbol(guidType), compilation.CreateArrayTypeSymbol(stringType));
            return result;
        }
    }
}
