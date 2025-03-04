using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertThrowsShouldUseGenericOverloadCheck>;

public class AssertThrowsShouldUseGenericOverloadCheckTests
{
	public static TheoryData<string> Methods = new()
	{
		Constants.Asserts.Throws,
		Constants.Asserts.ThrowsAsync,
	};

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task FindsWarning_ForThrowsCheck_WithExceptionParameter_OnThrowingMethod(string method)
	{
		var source = $@"
class TestClass {{
    System.Threading.Tasks.Task ThrowingMethod() {{
        throw new System.NotImplementedException();
    }}

    void TestMethod() {{
        Xunit.Assert.{method}(typeof(System.NotImplementedException), ThrowingMethod);
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(8, 9, 8, 78 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments(method, "System.NotImplementedException");

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task FindsWarning_ForThrowsCheck_WithExceptionParameter_OnThrowingLambda(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        Xunit.Assert.{method}(typeof(System.NotImplementedException), () => System.Threading.Tasks.Task.Delay(0));
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(4, 9, 4, 106 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments(method, "System.NotImplementedException");

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}

	[Fact]
	public async void FindsCompilerError_ForThrowsCheck_WithExceptionTypeArgument_OnThrowingMethod()
	{
		var source = @"
class TestClass {
    System.Threading.Tasks.Task ThrowingMethod() {
        throw new System.NotImplementedException();
    }

    void TestMethod() {
        Xunit.Assert.Throws<System.NotImplementedException>(ThrowingMethod);
    }
}";
		var expected =
			Verify
				.CompilerError("CS0619")
				.WithSpan(8, 9, 8, 76)
				.WithMessage($"'Assert.Throws<T>(Func<Task>)' is obsolete: 'You must call Assert.ThrowsAsync<T> (and await the result) when testing async code.'");

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}

	[Fact]
	public async void DoesNotFindWarning_ForThrowsAsyncCheck_WithExceptionTypeArgument_OnThrowingMethod()
	{
		var source = @"
class TestClass {
    System.Threading.Tasks.Task ThrowingMethod() {
        throw new System.NotImplementedException();
   }

    async System.Threading.Tasks.Task TestMethod() {
        await Xunit.Assert.ThrowsAsync<System.NotImplementedException>(ThrowingMethod);
    }
}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}

	[Fact]
	public async void FindsCompilerError_ForThrowsCheck_WithExceptionTypeArgument_OnThrowingLambda()
	{
		var source = @"
class TestClass {
    void TestMethod() {
        Xunit.Assert.Throws<System.NotImplementedException>(() => System.Threading.Tasks.Task.Delay(0));
    }
}";
		var expected =
			Verify
				.CompilerError("CS0619")
				.WithSpan(4, 9, 4, 104)
				.WithMessage("'Assert.Throws<T>(Func<Task>)' is obsolete: 'You must call Assert.ThrowsAsync<T> (and await the result) when testing async code.'");

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}

	[Fact]
	public async void DoesNotFindWarning_ForThrowsAsyncCheck_WithExceptionTypeArgument_OnThrowingLambda()
	{
		var source = @"
class TestClass {
    async System.Threading.Tasks.Task TestMethod() {
        await Xunit.Assert.ThrowsAsync<System.NotImplementedException>(() => System.Threading.Tasks.Task.Delay(0));
    }
}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}
}
