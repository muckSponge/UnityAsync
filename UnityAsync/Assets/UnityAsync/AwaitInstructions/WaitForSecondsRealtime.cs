namespace UnityAsync
{
	public struct WaitForSecondsRealtime : IAwaitInstruction
	{
		readonly float finishTime;

		bool IAwaitInstruction.IsCompleted() => AsyncManager.CurrentUnscaledTime >= finishTime;

		/// <summary>
		/// Waits for the specified number of (unscaled) seconds to pass before continuing.
		/// </summary>
		public WaitForSecondsRealtime(float seconds) => finishTime = AsyncManager.CurrentUnscaledTime + seconds;
	}
}