using System;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify_X1022_v2_Pre220 = CSharpVerifier<RemoveMethodParameterFixTests.Analyzer_X1022>;
using Verify_X1023_v2_Pre220 = CSharpVerifier<RemoveMethodParameterFixTests.Analyzer_X1023>;
using Verify_X1026 = CSharpVerifier<Xunit.Analyzers.TheoryMethodShouldUseAllParameters>;

public class RemoveMethodParameterFixTests
{
	[Fact]
	public async void X1022_RemoveParamsArray()
	{
		var before = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData(1, 2, 3)]
    public void TestMethod([|params int[] values|]) { }
}";

		var after = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData(1, 2, 3)]
    public void TestMethod() { }
}";

		await Verify_X1022_v2_Pre220.VerifyCodeFixAsyncV2(before, after);
	}

	[Fact]
	public async void X1023_RemovesDefaultValue()
	{
		var before = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData(1)]
    public void TestMethod(int arg [|= 0|]) { }
}";

		var after = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData(1)]
    public void TestMethod(int arg) { }
}";

		await Verify_X1023_v2_Pre220.VerifyCodeFixAsyncV2(before, after);
	}

	[Fact]
	public async void X1026_RemovesUnusedParameter()
	{
		var before = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData(1)]
    public void TestMethod(int [|arg|]) { }
}";

		var after = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData(1)]
    public void TestMethod() { }
}";

		await Verify_X1026.VerifyCodeFixAsyncV2(before, after);
	}

	internal class Analyzer_X1022 : TheoryMethodCannotHaveParamsArray
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2Core(compilation, new Version(2, 1, 999));
	}

	internal class Analyzer_X1023 : TheoryMethodCannotHaveDefaultParameter
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2Core(compilation, new Version(2, 1, 999));
	}
}
