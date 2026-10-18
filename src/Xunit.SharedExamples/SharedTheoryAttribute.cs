using System.Runtime.CompilerServices;
using Xunit.v3;

namespace Xunit.SharedExamples;

/// <summary>
/// Marks a default interface method inside a <see cref="SharedExamplesAttribute"/> interface
/// as a data-driven shared example. Behaves exactly like <see cref="TheoryAttribute"/>, so it
/// is combined with data attributes such as <see cref="InlineDataAttribute"/> or
/// <see cref="MemberDataAttribute"/>, except that the test's display name names the
/// including class and the shared examples it behaves like.
/// </summary>
/// <param name="description">
/// A human-readable description of the example, phrased as RSpec's <c>it</c> would be
/// (e.g. <c>"contains every item that was added"</c>). Defaults to the method name.
/// The arguments of each data row are appended to it.
/// </param>
/// <param name="sourceFilePath">This parameter is provided automatically by the compiler. Do not pass a value for it.</param>
/// <param name="sourceLineNumber">This parameter is provided automatically by the compiler. Do not pass a value for it.</param>
[XunitTestCaseDiscoverer(typeof(SharedTheoryDiscoverer))]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class SharedTheoryAttribute(
	string? description = null,
	[CallerFilePath] string? sourceFilePath = null,
	[CallerLineNumber] int sourceLineNumber = -1) :
		TheoryAttribute(sourceFilePath, sourceLineNumber), ISharedExampleAttribute
{
	/// <summary>
	/// Gets the human-readable description of the example.
	/// </summary>
	public string? Description { get; } = description;
}
