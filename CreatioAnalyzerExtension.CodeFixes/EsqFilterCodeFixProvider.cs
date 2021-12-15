using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CreatioAnalyzerExtension
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EsqFilterCodeFixProvider)), Shared]
    public class EsqFilterCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(EsqFilterAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                var diagnosticSpan = diagnostic.Location.SourceSpan;
                SyntaxNode node = root.FindToken(diagnosticSpan.Start).Parent;

                //TODO: handle infinite loop
                while (node.Kind() != SyntaxKind.InvocationExpression)
                {
                    node = node.Parent;
                }

                if (node is InvocationExpressionSyntax expressionNode && expressionNode.ArgumentList.Arguments.Count == 3)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: CodeFixResources.CodeFixTitle,
                            createChangedDocument: c => FixFilterPath(context.Document, (InvocationExpressionSyntax)node, c),
                            equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
                        diagnostic);
                }
            }
        }

        private async Task<Document> FixFilterPath(Document document, InvocationExpressionSyntax currentFilter,
            CancellationToken cancellationToken)
        {
            var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var oldArgument = currentFilter.ArgumentList.Arguments[1].Expression.ChildTokens().First();

            var newArgument = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, 
                SyntaxFactory.Literal(oldArgument.Text.Substring(1, oldArgument.Text.Length - 5))));
            var argumentList = SyntaxFactory.SeparatedList(new[] { currentFilter.ArgumentList.Arguments[0], 
                newArgument, currentFilter.ArgumentList.Arguments[2] });
          
            var fixedFilter = SyntaxFactory
                    .InvocationExpression(currentFilter.Expression)
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList()
                            .WithOpenParenToken(
                                SyntaxFactory.Token(
                                    SyntaxKind.OpenParenToken))
                            .WithArguments(SyntaxFactory.SeparatedList<ArgumentSyntax>(argumentList))
                            .WithCloseParenToken(
                                SyntaxFactory.Token(
                                    SyntaxKind.CloseParenToken)));

            var newSyntaxRoot = syntaxRoot.ReplaceNode(currentFilter, fixedFilter);
            return document.WithSyntaxRoot(newSyntaxRoot);
        }
    }
}
