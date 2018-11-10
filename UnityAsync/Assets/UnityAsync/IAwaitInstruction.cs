namespace UnityAsync
{
	public enum FrameScheduler { Update, LateUpdate, FixedUpdate }

	/// <summary>
	/// Allows awaitable instructions to be implemented in a similar fashion to
	/// <see cref="UnityEngine.CustomYieldInstruction"/>s without the use of abstract classes and heap allocations.
	/// For maximum versatility, any struct which implements this should have a <c>public Continuation{T} GetAwaiter()</c>
	/// method exposed. See <see cref="UnityAsync.WaitForFrames"/> for a concise example.
	/// </summary>
	public interface IAwaitInstruction
	{
		bool IsCompleted();
	}
}