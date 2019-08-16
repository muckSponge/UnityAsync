using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace UnityAsync.Awaiters
{
	public struct YieldInstructionAwaiter : INotifyCompletion
	{
		static readonly SendOrPostCallback postCallback = state => ((Action)state)();

		readonly YieldInstruction instruction;

		public YieldInstructionAwaiter(YieldInstruction instruction)
		{
			this.instruction = instruction;
		}

		public void OnCompleted(Action continuation)
		{
			if(AsyncManager.InUnityContext)
				AsyncManager.StartCoroutine(ContinuationCoroutine(continuation));
			else
				AsyncManager.UnitySyncContext.Send(postCallback, AsyncManager.StartCoroutine(ContinuationCoroutine(continuation)));
		}

		IEnumerator ContinuationCoroutine(Action continuation)
		{
			yield return instruction;

			continuation();
		}

		public bool IsCompleted => false;
		public void GetResult() { }
	}
}