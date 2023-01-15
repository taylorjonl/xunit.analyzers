using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class CollectionDefinitionShouldBeDefinedInTheSameAssemblyAsItsUsages : XunitDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.X1028_CollectionDefinitionShouldBeDefinedInTheSameAssemblyAsItsUsages);

		public override void AnalyzeCompilation(
			CompilationStartAnalysisContext context,
			XunitContext xunitContext)
		{
			if (xunitContext.Core.CollectionAttributeType is null || xunitContext.Core.CollectionDefinitionAttributeType is null)
				return;

			var analyzer = new CompilationAnalyzer(xunitContext.Core.CollectionAttributeType, xunitContext.Core.CollectionDefinitionAttributeType);

			context.RegisterSyntaxNodeAction(analyzer.AnalyzeSyntaxNode, SyntaxKind.Attribute);
			context.RegisterCompilationEndAction(analyzer.CompilationEndAction);
		}

		private class CompilationAnalyzer
		{
			private readonly INamedTypeSymbol collectionAttributeType;
			private readonly INamedTypeSymbol collectionDefinitionAttributeType;
			private readonly ConcurrentBag<AttributeData> collectionAttributes = new ConcurrentBag<AttributeData>();
			private readonly ConcurrentBag<AttributeData> collectionDefinitionAttributes = new ConcurrentBag<AttributeData>();

			public CompilationAnalyzer(INamedTypeSymbol collectionAttributeType, INamedTypeSymbol collectionDefinitionAttributeType)
			{
				this.collectionAttributeType = collectionAttributeType;
				this.collectionDefinitionAttributeType = collectionDefinitionAttributeType;
			}

			public void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
			{
				if (context.Node is not AttributeSyntax attribute)
					return;

				var semanticModel = context.SemanticModel;
				if (Equals(semanticModel.GetTypeInfo(attribute).Type, collectionAttributeType))
				{
					var containingAssembly = context.ContainingSymbol.ContainingAssembly.Identity;
					var attributeData = GetAttributeData(containingAssembly, attribute);
					if (attributeData != null)
					{
						collectionAttributes.Add(attributeData);
					}
				}
				else if (Equals(semanticModel.GetTypeInfo(attribute).Type, collectionDefinitionAttributeType))
				{
					var containingAssembly = context.ContainingSymbol.ContainingAssembly.Identity;
					var attributeData = GetAttributeData(containingAssembly, attribute);
					if (attributeData != null)
					{
						collectionDefinitionAttributes.Add(attributeData);
					}
				}
			}

			public void CompilationEndAction(CompilationAnalysisContext context)
			{
				var collectionDefinitionsByAssembly =
					collectionDefinitionAttributes
						.GroupBy(x => x.ContainingAssembly, x => x.CollectionName)
						.ToDictionary(x => x.Key, x => x.ToList());
				var collectionDefinitionsByCollectionName =
					collectionDefinitionAttributes
						.GroupBy(x => x.CollectionName, x => x.ContainingAssembly)
						.ToDictionary(x => x.Key, x => x.ToList());

				foreach (var collectionAttribute in collectionAttributes)
				{
					var assemblyContainsCollectionDefinition = collectionDefinitionsByAssembly.TryGetValue(collectionAttribute.ContainingAssembly, out List<string> collectionNames) && collectionNames.Contains(collectionAttribute.CollectionName);
					if (assemblyContainsCollectionDefinition)
					{
						continue;
					}
					var otherAssemblyContainsCollectionDefinition = collectionDefinitionsByCollectionName.TryGetValue(collectionAttribute.CollectionName, out List<AssemblyIdentity> assemblies) && assemblies.Any(x => !Equals(x, collectionAttribute.ContainingAssembly));
					if (otherAssemblyContainsCollectionDefinition)
					{
						context.ReportDiagnostic(
							Diagnostic.Create(
								Descriptors.X1028_CollectionDefinitionShouldBeDefinedInTheSameAssemblyAsItsUsages,
								collectionAttribute.Location
							)
						);
					}
				}
			}

			private static string? GetCollectionName(AttributeSyntax attribute)
			{
				if (attribute.ArgumentList.Arguments.Count == 1)
				{
					var attributeArgument = attribute.ArgumentList.Arguments[0];
					if (attributeArgument.Expression is LiteralExpressionSyntax literalExpression)
					{
						if (literalExpression.IsKind(SyntaxKind.StringLiteralExpression))
						{
							return literalExpression.ToString().Trim('"');
						}
					}
					else if (attributeArgument.Expression is RefExpressionSyntax refExpression)
					{

					}
				}
				return null;
			}

			private static AttributeData? GetAttributeData(AssemblyIdentity containingAssembly, AttributeSyntax attribute)
			{
				var collectionName = GetCollectionName(attribute);
				if (collectionName == null)
					return null;

				var location = attribute.GetLocation();

				var attributeData = new AttributeData(containingAssembly, collectionName, location);
				return attributeData;
			}

			private class AttributeData
			{
				public AttributeData(AssemblyIdentity containingAssembly, string collectionName, Location location)
				{
					ContainingAssembly = containingAssembly;
					CollectionName = collectionName;
					Location = location;
				}

				public AssemblyIdentity ContainingAssembly { get; }
				public string CollectionName { get; }
				public Location Location { get; }
			}
		}
	}
}
