namespace Xunit.SharedExamples;

/// <summary>
/// Implemented by the attributes that mark a shared example, so that the description can be
/// read without knowing which kind of example it is.
/// </summary>
interface ISharedExampleAttribute
{
	string? Description { get; }
}
