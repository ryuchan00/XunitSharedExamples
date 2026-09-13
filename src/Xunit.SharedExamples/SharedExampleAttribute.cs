using System.Runtime.CompilerServices;
using Xunit.v3;

namespace Xunit.SharedExamples;

/// <summary>
/// Marks a default interface method inside a <see cref="SharedExamplesAttribute"/> interface
/// as a single shared example. Behaves exactly like <see cref="FactAttribute"/>, except that
/// the test's display name names the including class and the shared examples it behaves like.
/// </summary>
/// <param name="description">
/// A human-readable description of the example, phrased as RSpec's <c>it</c> would be
/// (e.g. <c>"is empty when created"</c>). Defaults to the method name.
/// </param>
/// <param name="sourceFilePath">This parameter is provided automatically by the compiler. Do not pass a value for it.</param>
/// <param name="sourceLineNumber">This parameter is provided automatically by the compiler. Do not pass a value for it.</param>
[XunitTestCaseDiscoverer(typeof(SharedExampleDiscoverer))]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class SharedExampleAttribute(
	string? description = null,
	[CallerFilePath] string? sourceFilePath = null,
	[CallerLineNumber] int sourceLineNumber = -1) :
		FactAttribute(sourceFilePath, sourceLineNumber)
{
	/// <summary>
	/// Gets the human-readable description of the example.
	/// </summary>
	public string? Description { get; } = description;
}
