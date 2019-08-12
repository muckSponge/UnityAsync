namespace UnityAsync
{
	public struct WaitForSeconds : IAwaitInstruction
	{
		readonly float finishTime;

		bool IAwaitInstruction.IsCompleted() => AsyncManager.CurrentTime >= finishTime;

		/// <summary>
		/// Waits for the specified number of seconds to pass before continuing.
		/// </summary>
		public WaitForSeconds(float seconds) => finishTime = AsyncManager.CurrentTime + seconds;
	}
}