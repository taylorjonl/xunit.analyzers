using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
	public class V3CoreContext : ICoreContext
	{
		readonly Lazy<INamedTypeSymbol?> lazyClassDataAttributeType;
		readonly Lazy<INamedTypeSymbol?> lazyCollectionAttributeType;
		readonly Lazy<INamedTypeSymbol?> lazyCollectionDefinitionAttributeType;
		readonly Lazy<INamedTypeSymbol?> lazyDataAttributeType;
		readonly Lazy<INamedTypeSymbol?> lazyFactAttributeType;
		readonly Lazy<INamedTypeSymbol?> lazyIClassFixtureType;
		readonly Lazy<INamedTypeSymbol?> lazyICollectionFixtureType;
		readonly Lazy<INamedTypeSymbol?> lazyInlineDataAttributeType;
		readonly Lazy<INamedTypeSymbol?> lazyMemberDataAttributeType;
		readonly Lazy<INamedTypeSymbol?> lazyTheoryAttributeType;

		V3CoreContext(
			Compilation compilation,
			Version version)
		{
			Version = version;

			lazyClassDataAttributeType = new(() => compilation.GetTypeByMetadataName(Constants.Types.XunitClassDataAttribute));
			lazyCollectionAttributeType = new(() => compilation.GetTypeByMetadataName(Constants.Types.XunitCollectionAttribute));
			lazyCollectionDefinitionAttributeType = new(() => compilation.GetTypeByMetadataName(Constants.Types.XunitCollectionDefinitionAttribute));
			lazyDataAttributeType = new(() => compilation.GetTypeByMetadataName(Constants.Types.XunitSdkDataAttribute));
			lazyFactAttributeType = new(() => compilation.GetTypeByMetadataName(Constants.Types.XunitFactAttribute));
			lazyIClassFixtureType = new(() => compilation.GetTypeByMetadataName(Constants.Types.XunitIClassFixtureFixture));
			lazyICollectionFixtureType = new(() => compilation.GetTypeByMetadataName(Constants.Types.XunitICollectionFixtureFixture));
			lazyInlineDataAttributeType = new(() => compilation.GetTypeByMetadataName(Constants.Types.XunitInlineDataAttribute));
			lazyMemberDataAttributeType = new(() => compilation.GetTypeByMetadataName(Constants.Types.XunitMemberDataAttribute));
			lazyTheoryAttributeType = new(() => compilation.GetTypeByMetadataName(Constants.Types.XunitTheoryAttribute));
		}

		public INamedTypeSymbol? ClassDataAttributeType =>
			lazyClassDataAttributeType.Value;

		public INamedTypeSymbol? CollectionAttributeType =>
			lazyCollectionAttributeType.Value;

		public INamedTypeSymbol? CollectionDefinitionAttributeType =>
			lazyCollectionDefinitionAttributeType.Value;

		public INamedTypeSymbol? DataAttributeType =>
			lazyDataAttributeType.Value;

		public INamedTypeSymbol? FactAttributeType =>
			lazyFactAttributeType.Value;

		public INamedTypeSymbol? IClassFixtureType =>
			lazyIClassFixtureType.Value;

		public INamedTypeSymbol? ICollectionFixtureType =>
			lazyICollectionFixtureType.Value;

		public INamedTypeSymbol? InlineDataAttributeType =>
			lazyInlineDataAttributeType.Value;

		public INamedTypeSymbol? MemberDataAttributeType =>
			lazyMemberDataAttributeType.Value;

		public INamedTypeSymbol? TheoryAttributeType =>
			lazyTheoryAttributeType.Value;

		public bool TheorySupportsConversionFromStringToDateTimeOffsetAndGuid => true;

		public bool TheorySupportsDefaultParameterValues => true;

		public bool TheorySupportsParameterArrays => true;

		public Version Version { get; set; }

		public static V3CoreContext? Get(
			Compilation compilation,
			Version? versionOverride = null)
		{
			var version =
				versionOverride ??
				compilation
					.ReferencedAssemblyNames
					.FirstOrDefault(a => a.Name.Equals("xunit.v3.core", StringComparison.OrdinalIgnoreCase))
					?.Version;

			return version is null ? null : new(compilation, version);
		}
	}
}
