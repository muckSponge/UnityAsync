using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityAsync.Awaiters
{
	public struct ResourceRequestAwaiter : INotifyCompletion
	{
		readonly ResourceRequest request;
 
		public ResourceRequestAwaiter(ResourceRequest request)
		{
			this.request = request;
		}
 
		public UnityEngine.Object GetResult() => request.asset;
		public bool IsCompleted => request.isDone;
		public void OnCompleted(Action action) => request.completed += _ => action();
	}
}