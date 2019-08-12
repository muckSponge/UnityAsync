using System;

namespace UnityAsync
{
	public struct WaitWhile<TState> : IAwaitInstruction
	{
		readonly Func<TState, bool> condition;
		readonly TState state;

		bool IAwaitInstruction.IsCompleted() => !condition(state);

		/// <summary>
		/// Waits until the condition returns true before continuing.
		/// </summary>
		public WaitWhile(TState state, Func<TState, bool> condition)
		{
			#if UNITY_EDITOR
			if(condition == null)
				throw new ArgumentNullException(nameof(condition), "This check only occurs in edit mode.");
			#endif
			
			this.condition = condition;
			this.state = state;
		}
	}
}