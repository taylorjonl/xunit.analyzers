using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AssertEqualLiteralValueShouldBeFirst : AssertUsageAnalyzerBase
	{
		static readonly string[] targetMethods =
		{
			Constants.Asserts.Equal,
			Constants.Asserts.StrictEqual,
			Constants.Asserts.NotEqual,
			Constants.Asserts.NotStrictEqual
		};

		public AssertEqualLiteralValueShouldBeFirst()
			: base(Descriptors.X2000_AssertEqualLiteralValueShouldBeFirst, targetMethods)
		{ }

		protected override void Analyze(
			OperationAnalysisContext context,
			IInvocationOperation invocationOperation,
			IMethodSymbol method)
		{
			var arguments = invocationOperation.Arguments;
			if (arguments.Length < 2)
				return;

			var expectedArg = arguments.FirstOrDefault(arg => arg.Parameter.Name == Constants.AssertArguments.Expected);
			var actualArg = arguments.FirstOrDefault(arg => arg.Parameter.Name == Constants.AssertArguments.Actual);
			if (expectedArg is null || actualArg is null)
				return;

			if (IsLiteralOrConstant(actualArg.Value) && !IsLiteralOrConstant(expectedArg.Value))
			{
				var parentMethod = context.ContainingSymbol;
				var parentType = parentMethod.ContainingType;

				context.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.X2000_AssertEqualLiteralValueShouldBeFirst,
						invocationOperation.Syntax.GetLocation(),
						actualArg.Value.Syntax.ToString(),
						SymbolDisplay.ToDisplayString(
							method,
							SymbolDisplayFormat
								.CSharpShortErrorMessageFormat
								.WithGenericsOptions(SymbolDisplayGenericsOptions.None)
								.WithParameterOptions(SymbolDisplayParameterOptions.IncludeName)
						),
						parentMethod.Name,
						parentType?.Name ?? "<unknown>"
					)
				);
			}
		}

		static bool IsLiteralOrConstant(IOperation operation) =>
			operation.ConstantValue.HasValue || operation.Kind == OperationKind.TypeOf;
	}
}
