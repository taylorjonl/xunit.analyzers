using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class TestMethodMustNotHaveMultipleFactAttributes : XunitDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.X1002_TestMethodMustNotHaveMultipleFactAttributes);

		public override void AnalyzeCompilation(
			CompilationStartAnalysisContext context,
			XunitContext xunitContext)
		{
			context.RegisterSymbolAction(context =>
			{
				if (xunitContext.Core.FactAttributeType is null)
					return;
				if (context.Symbol is not IMethodSymbol symbol)
					return;

				var attributeTypes = new HashSet<INamedTypeSymbol>();
				var count = 0;

				foreach (var attribute in symbol.GetAttributes())
				{
					var attributeType = attribute.AttributeClass;
					if (xunitContext.Core.FactAttributeType.IsAssignableFrom(attributeType))
					{
						attributeTypes.Add(attributeType);
						count++;
					}
				}

				if (count > 1)
				{
					context.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X1002_TestMethodMustNotHaveMultipleFactAttributes,
							symbol.Locations.First(),
							properties: attributeTypes.ToImmutableDictionary(t => t.ToDisplayString(), t => string.Empty)
						)
					);
				}
			}, SymbolKind.Method);
		}
	}
}
