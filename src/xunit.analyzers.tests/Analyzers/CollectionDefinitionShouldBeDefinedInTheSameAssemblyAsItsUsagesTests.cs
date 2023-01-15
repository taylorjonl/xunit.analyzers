using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.CollectionDefinitionShouldBeDefinedInTheSameAssemblyAsItsUsages>;

public class CollectionDefinitionShouldBeDefinedInTheSameAssemblyAsItsUsagesTests
{
	[Fact]
	public async void ForPublicClass_DoesNotFindError()
	{
		var source = @"
[Xunit.CollectionDefinition(""MyCollection"")]
public class CollectionDefinitionClass { }
[Xunit.Collection(""MyCollection"")]
public class Class { }
";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}

	[Fact]
	public async void ForPublicClass1_DoesNotFindError()
	{
		var source1 = @"
[Xunit.CollectionDefinition(""MyCollection"")]
public class CollectionDefinitionClass { }
";

		var source2 = @"
[Xunit.Collection(""MyCollection"")]
public class Class { }
";

		await Verify.VerifyAnalyzerAsyncV2(new []{ source1, source2 });
	}
}
