using System;

#if UNITY_EDITOR
using UnityEngine;
#endif

namespace UnityAsync
{
	public struct WaitUntil : IAwaitInstruction
	{
		readonly Func<bool> condition;

		bool IAwaitInstruction.IsCompleted() => condition();

		/// <summary>
		/// Waits until the condition returns true before continuing.
		/// </summary>
		public WaitUntil(Func<bool> condition)
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
		
		public Continuation<WaitUntil> GetAwaiter() => new Continuation<WaitUntil>(this);
	}
}