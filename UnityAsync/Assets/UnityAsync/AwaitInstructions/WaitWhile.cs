using System;

#if UNITY_EDITOR
using UnityEngine;
#endif

namespace UnityAsync
{
	public struct WaitWhile : IAwaitInstruction
	{
		readonly Func<bool> condition;

		bool IAwaitInstruction.IsCompleted() => !condition();

		/// <summary>
		/// Waits until the condition returns false before continuing.
		/// </summary>
		public WaitWhile(Func<bool> condition)
		{
			#if UNITY_EDITOR
			if(condition == null)
			{
				condition = () => true;
				Debug.LogError($"{nameof(condition)} should not be null. This check will only appear in edit mode.");
			}
			#endif
			
			this.condition = condition;
		}
		
		public Continuation<WaitWhile> GetAwaiter() => new Continuation<WaitWhile>(this);
	}
}