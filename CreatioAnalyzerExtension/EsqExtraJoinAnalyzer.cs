using CreatioAnalyzerExtension.Constants;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace CreatioAnalyzerExtension
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EsqExtraJoinAnalyzer : DiagnosticAnalyzer
    {
        private readonly static DiagnosticDescriptor FilterRule =
            new DiagnosticDescriptor(
                DiagnosticId.EsqExtraJoin,
                new LocalizableResourceString(nameof(Resources.ExtraJoinTitle), Resources.ResourceManager, typeof(Resources)).ToString(),
                new LocalizableResourceString(nameof(Resources.ExtraJoinMessageFormat), Resources.ResourceManager, typeof(Resources)).ToString(),
                "Naming",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: new LocalizableResourceString(nameof(Resources.ExtraJoinDescription), Resources.ResourceManager, typeof(Resources)).ToString());

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
                node.ArgumentList.Arguments.Count != 3)
            {
                return;
            }

            var argumentTokens = node.ArgumentList.Arguments[1].Expression.ChildTokens();
            if (argumentTokens.Any())
            {
                var argumentValue = argumentTokens.First().ValueText;
                if (!string.IsNullOrEmpty(argumentValue) && argumentValue.Split('.').Last() == "Id")
                {
                    var diagnostic = Diagnostic.Create(FilterRule, context.Node.GetLocation(), argumentValue,
                        argumentValue.Substring(0, argumentValue.Length - 3));
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
