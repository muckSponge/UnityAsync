using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace UnityAsync.Awaiters
{
	public struct SynchronizationContextAwaiter : INotifyCompletion
	{
		static readonly SendOrPostCallback postCallback = state => ((Action)state)();

		readonly SynchronizationContext context;

		public SynchronizationContextAwaiter(SynchronizationContext context)
		{
			this.context = context;
		}

		public bool IsCompleted => context == SynchronizationContext.Current;
		public void OnCompleted(Action continuation) => context.Post(postCallback, continuation);
		public void GetResult() { }
	}
}