using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityAsync.Awaiters
{
	public struct AsyncOperationAwaiter : INotifyCompletion
	{
		 readonly AsyncOperation op;
	
		 public AsyncOperationAwaiter(AsyncOperation op)
		 {
			 this.op = op;
		 }

		 public void GetResult() { }
		 public bool IsCompleted => op.isDone;

		public void OnCompleted(Action action) => op.completed += _ => action();
	}
}