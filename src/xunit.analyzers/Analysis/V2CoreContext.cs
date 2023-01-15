using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
	public class V2CoreContext : ICoreContext
	{
		internal static readonly Version Version_2_2_0 = new("2.2.0");
		internal static readonly Version Version_2_4_0 = new("2.4.0");

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

		V2CoreContext(
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

		// See: https://github.com/xunit/xunit/pull/1546
		public bool TheorySupportsConversionFromStringToDateTimeOffsetAndGuid =>
			Version >= Version_2_4_0;

		public bool TheorySupportsDefaultParameterValues =>
			Version >= Version_2_2_0;

		public bool TheorySupportsParameterArrays =>
			Version >= Version_2_2_0;

		public Version Version { get; set; }

		public static V2CoreContext? Get(
			Compilation compilation,
			Version? versionOverride = null)
		{
			var version =
				versionOverride ??
				compilation
					.ReferencedAssemblyNames
					.FirstOrDefault(a => a.Name.Equals("xunit.core", StringComparison.OrdinalIgnoreCase))
					?.Version;

			return version is null ? null : new(compilation, version);
		}
	}
}
