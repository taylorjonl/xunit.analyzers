using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualGenericShouldNotBeUsedForStringValue>;

public class AssertEqualGenericShouldNotBeUsedForStringValueTests
{
	public static TheoryData<string, string> Data = new()
	{
		{ "true.ToString()", "\"True\"" },
		{ "1.ToString()", "\"1\"" },
		{ "\"\"", "null" },
		{ "null", "\"\"" },
		{ "\"\"", "\"\"" },
		{ "\"abc\"", "\"abc\"" },
		{ "\"TestMethod\"", "nameof(TestMethod)" },
	};

	[Theory]
	[MemberData(nameof(Data))]
	public async void DoesNotFindWarningForStringEqualityCheckWithoutGenericType(
		string expected,
		string value)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        Xunit.Assert.Equal({expected}, {value});
    }}
}}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}

	[Theory]
	[MemberData(nameof(Data))]
	public async void FindsWarningForStringEqualityCheckWithGenericType(
		string expected,
		string value)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        [|Xunit.Assert.Equal<string>({expected}, {value})|];
    }}
}}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}

	[Theory]
	[MemberData(nameof(Data))]
	public async void FindsWarningForStrictStringEqualityCheck(
		string expected,
		string value)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        [|Xunit.Assert.StrictEqual({expected}, {value})|];
    }}
}}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}

	[Theory]
	[MemberData(nameof(Data))]
	public async void FindsWarningForStrictStringEqualityCheckWithGenericType(
		string expected,
		string value)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        [|Xunit.Assert.StrictEqual<string>({expected}, {value})|];
    }}
}}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}
}
