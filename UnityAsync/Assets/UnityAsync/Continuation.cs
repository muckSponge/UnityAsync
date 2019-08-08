using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Object = UnityEngine.Object;

namespace UnityAsync
{
	public interface IContinuation
	{
		bool Evaluate();
		FrameScheduler Scheduler { get; }
	}

	/// <summary>
	/// Encapsulates an <see cref="UnityAsync.IAwaitInstruction"/> with additional information about how the instruction
	/// will be queued and executed. Continuations are intended to be awaited after or shortly after instantiation.
	/// </summary>
	/// <typeparam name="T">The type of <see cref="UnityAsync.IAwaitInstruction"/> to encapsulate.</typeparam>
	public struct Continuation<T> : IContinuation, INotifyCompletion where T : IAwaitInstruction
	{
		Object owner;
		CancellationToken cancellationToken;

		/// <summary>
		/// Evaluate the encapsulated <see cref="UnityAsync.IAwaitInstruction"/>. If the instruction is finished, the
		/// continuation delegate is invoked and the method returns <code>true</code>. If the owner is destroyed or a
		/// cancellation was requested, the method will return true and will not invoke the continuation delegate.
		/// Otherwise, the method will return false, meaning the <see cref="UnityAsync.IAwaitInstruction"/> is not yet
		/// finished.
		/// </summary>
		/// <returns>
		/// <code>true</code> if the <see cref="UnityAsync.IAwaitInstruction"/> is finished or cancelled, otherwise
		/// false.
		/// </returns>
		public bool Evaluate()
		{
			if(!owner || cancellationToken.IsCancellationRequested)
				return true;
			
			if(instruction.IsCompleted())
			{
				continuation();
				
				return true;
			}

			return false;
		}

		public FrameScheduler Scheduler { get; private set; }

		T instruction;
		Action continuation;

		public Continuation(T inst)
		{
			instruction = inst;
			continuation = null;
			owner = AsyncManager.Instance;
			Scheduler = FrameScheduler.Update;
		}

		public Continuation(T inst, FrameScheduler scheduler)
		{
			instruction = inst;
			continuation = null;
			owner = AsyncManager.Instance;
			Scheduler = scheduler;
		}
		
		public Continuation(T inst, CancellationToken cancellationToken, FrameScheduler scheduler)
		{
			instruction = inst;
			continuation = null;
			owner = AsyncManager.Instance;
			this.cancellationToken = cancellationToken;
			Scheduler = scheduler;
		}
		
		public Continuation(T inst, Object owner, FrameScheduler scheduler)
		{
			instruction = inst;
			continuation = null;
			this.owner = owner;
			Scheduler = scheduler;
		}

		public bool IsCompleted => false;

		public void OnCompleted(Action continuation)
		{
			this.continuation = continuation;
			AsyncManager.AddContinuation(this);
		}

		/// <summary>
		/// Link the continuation's lifespan to a <see cref="UnityEngine.Object"/> and configure the type of update
		/// cycle it should be evaluated on.
		/// </summary>
		/// <returns>A new continuation with updated params.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Continuation<T> ConfigureAwait(Object owner, FrameScheduler scheduler)
		{
			this.owner = owner;
			Scheduler = scheduler;
			return this;
		}

		/// <summary>
		/// Link the continuation's lifespan to a <see cref="UnityEngine.Object"/>.
		/// </summary>
		/// <returns>A new continuation with updated params.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Continuation<T> ConfigureAwait(Object owner)
		{
			this.owner = owner;
			return this;
		}

		/// <summary>
		/// Configure the type of update cycle it should be evaluated on.
		/// </summary>
		/// <returns>A new continuation with updated params.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Continuation<T> ConfigureAwait(FrameScheduler scheduler)
		{
			Scheduler = scheduler;
			return this;
		}
		
		/// <summary>
		/// Link the continuation's lifespan to a <see cref="System.Threading.CancellationToken"/> and configure the
		/// type of update cycle it should be evaluated on.
		/// </summary>
		/// <returns>A new continuation with updated params.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Continuation<T> ConfigureAwait(CancellationToken cancellationToken, FrameScheduler scheduler)
		{
			this.cancellationToken = cancellationToken;
			Scheduler = scheduler;
			return this;
		}
		
		/// <summary>
		/// Link the continuation's lifespan to a <see cref="System.Threading.CancellationToken"/>.
		/// </summary>
		/// <returns>A new continuation with updated params.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Continuation<T> ConfigureAwait(CancellationToken cancellationToken)
		{
			this.cancellationToken = cancellationToken;
			return this;
		}

		public void GetResult() { }
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Continuation<T> GetAwaiter() => this;
	}
}