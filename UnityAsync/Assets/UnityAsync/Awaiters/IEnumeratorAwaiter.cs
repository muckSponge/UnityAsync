using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;

namespace UnityAsync.Awaiters
{
	struct IEnumeratorAwaiter : INotifyCompletion
	{
		static readonly SendOrPostCallback postCallback = state => ((Action)state)();

		readonly IEnumerator coroutine;

		public IEnumeratorAwaiter(IEnumerator coroutine)
		{
			this.coroutine = coroutine;
		}

		public void OnCompleted(Action continuation)
		{
			if(AsyncManager.InUnityContext)
				AsyncManager.StartCoroutine(ContinuationCoroutine(continuation));
			else
				AsyncManager.UnitySyncContext.Post(postCallback, AsyncManager.StartCoroutine(ContinuationCoroutine(continuation)));
		}

		IEnumerator ContinuationCoroutine(Action continuation)
		{
			yield return AsyncManager.StartCoroutine(coroutine);

			continuation();
		}

		public bool IsCompleted => false;
		public void GetResult() { }
	}
}