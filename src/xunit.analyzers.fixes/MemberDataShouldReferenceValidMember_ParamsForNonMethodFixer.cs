using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;

namespace Xunit.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class MemberDataShouldReferenceValidMember_ParamsForNonMethodFixer : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.X1021_MemberDataNonMethodShouldNotHaveParameters.Id);

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var diagnostic = context.Diagnostics.FirstOrDefault();
			if (diagnostic is null)
				return;

			var diagnosticId = diagnostic.Id;
			var attribute = root.FindNode(context.Span).FirstAncestorOrSelf<AttributeSyntax>();

			context.RegisterCodeFix(
				CodeAction.Create(
					"Remove Arguments",
					createChangedDocument: ct => RemoveUnneededArguments(context.Document, attribute, context.Span, ct),
					equivalenceKey: "Remove MemberData Arguments"
				),
				context.Diagnostics
			);
		}

		async Task<Document> RemoveUnneededArguments(
			Document document,
			AttributeSyntax attribute,
			TextSpan span,
			CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

			foreach (var argument in attribute.ArgumentList.Arguments)
				if (argument.Span.OverlapsWith(span))
					editor.RemoveNode(argument);

			return editor.GetChangedDocument();
		}
	}
}
