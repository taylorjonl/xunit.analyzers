using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Xunit.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class InlineDataMustMatchTheoryParameters_TooFewValuesFixer : CodeFixProvider
	{
		const string title = "Add Default Values";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.X1009_InlineDataMustMatchTheoryParameters_TooFewValues.Id);

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var node = root.FindNode(context.Span);
			if (node is not AttributeSyntax attribute)
				return;

			var diagnostic = context.Diagnostics.FirstOrDefault();
			if (diagnostic is null)
				return;
			if (!diagnostic.Properties.TryGetValue(Constants.Properties.ParameterArrayStyle, out var arrayStyleText))
				return;

			var diagnosticId = diagnostic.Id;
			var method = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();

			Enum.TryParse<InlineDataMustMatchTheoryParameters.ParameterArrayStyleType>(arrayStyleText, out var arrayStyle);

			context.RegisterCodeFix(
				CodeAction.Create(
					title,
					ct => AddDefaultValues(context.Document, attribute, method, arrayStyle, ct),
					title
				),
				context.Diagnostics
			);
		}

		async Task<Document> AddDefaultValues(
			Document document,
			AttributeSyntax attribute,
			MethodDeclarationSyntax method,
			InlineDataMustMatchTheoryParameters.ParameterArrayStyleType arrayStyle,
			CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
			var arrayInitializer = default(InitializerExpressionSyntax);
			if (arrayStyle == InlineDataMustMatchTheoryParameters.ParameterArrayStyleType.Initializer)
				arrayInitializer = attribute.DescendantNodes().First(n => n.IsKind(SyntaxKind.ArrayInitializerExpression)) as InitializerExpressionSyntax;

			var originalInitializer = arrayInitializer;
			var i = originalInitializer?.Expressions.Count ?? attribute.ArgumentList?.Arguments.Count ?? 0;
			for (; i < method.ParameterList.Parameters.Count; i++)
				if (CreateDefaultValueSyntax(editor, method.ParameterList.Parameters[i].Type) is ExpressionSyntax defaultExpression)
				{
					if (arrayInitializer is not null)
						arrayInitializer = arrayInitializer.AddExpressions(defaultExpression);
					else
						editor.AddAttributeArgument(attribute, defaultExpression);
				}

			if (arrayInitializer is not null)
				editor.ReplaceNode(originalInitializer, arrayInitializer);

			return editor.GetChangedDocument();
		}

		SyntaxNode CreateDefaultValueSyntax(
			DocumentEditor editor,
			TypeSyntax type)
		{
			var t = editor.SemanticModel.GetTypeInfo(type).Type;
			switch (t.SpecialType)
			{
				case SpecialType.System_Boolean:
					return editor.Generator.FalseLiteralExpression();

				case SpecialType.System_Char:
					return editor.Generator.LiteralExpression(default(char));

				case SpecialType.System_Double:
					return editor.Generator.LiteralExpression(default(double));

				case SpecialType.System_Single:
					return editor.Generator.LiteralExpression(default(float));

				case SpecialType.System_UInt32:
				case SpecialType.System_UInt64:
					return editor.Generator.LiteralExpression(default(uint));

				case SpecialType.System_Byte:
				case SpecialType.System_Decimal:
				case SpecialType.System_Int16:
				case SpecialType.System_Int32:
				case SpecialType.System_Int64:
				case SpecialType.System_SByte:
				case SpecialType.System_UInt16:
					return editor.Generator.LiteralExpression(default(int));

				case SpecialType.System_String:
					return editor.Generator.LiteralExpression(string.Empty);
			}

			if (t.TypeKind == TypeKind.Enum)
				return editor.Generator.DefaultExpression(t);

			return editor.Generator.NullLiteralExpression();
		}
	}
}
