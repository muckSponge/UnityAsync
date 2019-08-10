using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
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
		static Exception exception;
		
		Object owner;
		CancellationToken cancellationToken;

		/// <summary>
		/// Evaluate the encapsulated <see cref="UnityAsync.IAwaitInstruction"/>. If the instruction is finished, the
		/// continuation delegate is invoked and the method returns <code>true</code>. If the owner is destroyed, the
		/// method will return true without invoking the continuation delegate. If it was cancelled, an exception is
		/// is thrown when the continuation delegate is invoked. Otherwise, returns false (i.e. the instruction is not
		/// finished).
		/// </summary>
		/// <returns>
		/// <code>true</code> if the <see cref="UnityAsync.IAwaitInstruction"/> is finished, its owner destroyed,
		/// or has been cancelled, otherwise <code>false</code>.
		/// </returns>
		public bool Evaluate()
		{
			try
			{
				// if cancelled, throw exception
				cancellationToken.ThrowIfCancellationRequested();
				
				// if owner is destroyed, behaves like a UnityEngine.Coroutine, ie:
				// "With this object's death, the thread of prophecy is severed. Restore a saved game to restore the
				// weave of fate, or persist in the doomed world you have created."
				if(!owner)
					return true;

				// if not completed, return false to put it back into a queue for next frame
				if(!instruction.IsCompleted())
					return false;
			}
			catch(Exception e)
			{
				// store exception in static field
				exception = e;
				
				// exception is rethrown in GetResult, at start of continuation
				continuation();

				return true;
			}
			
			// if we get here, it completed without exceptions and we can call continuation and be done with it
			continuation();
			
			return true;
		}

		public FrameScheduler Scheduler { get; private set; }

		T instruction;
		Action continuation;

		public Continuation(T inst)
		{
			instruction = inst;
			continuation = null;
			owner = AsyncManager.Instance;
			exception = null;
			Scheduler = FrameScheduler.Update;
		}

		public Continuation(T inst, FrameScheduler scheduler)
		{
			instruction = inst;
			continuation = null;
			owner = AsyncManager.Instance;
			exception = null;
			Scheduler = scheduler;
		}
		
		public Continuation(T inst, CancellationToken cancellationToken, FrameScheduler scheduler)
		{
			instruction = inst;
			continuation = null;
			owner = AsyncManager.Instance;
			exception = null;
			this.cancellationToken = cancellationToken;
			Scheduler = scheduler;
		}
		
		public Continuation(T inst, Object owner, FrameScheduler scheduler)
		{
			instruction = inst;
			continuation = null;
			this.owner = owner;
			exception = null;
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

		public void GetResult()
		{
			if(exception != null)
			{
				var e = exception;
				exception = null;

				throw e;
			}
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Continuation<T> GetAwaiter() => this;
	}
}