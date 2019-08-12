using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UnityAsync.Awaiters;
using Object = UnityEngine.Object;

namespace UnityAsync
{
	static class Extensions
	{
		static readonly Func<Object, IntPtr> ObjectPtrGetter;
		
		static Extensions()
		{
			ReflectionUtility.GenerateFieldGetter("m_CachedPtr", out ObjectPtrGetter);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsAlive(this Object o) => ObjectPtrGetter(o) != IntPtr.Zero;
		
		/// <summary>
		/// Link the <see cref="UnityAsync.IAwaitInstruction"/>'s lifespan to a <see cref="UnityEngine.Object"/> and
		/// configure the type of update cycle it should be evaluated on.
		/// </summary>
		/// <returns>A continuation with updated params.</returns>
		public static AwaitInstructionAwaiter<T> ConfigureAwait<T>(this T i, Object parent, FrameScheduler scheduler) where T : IAwaitInstruction
			=> new AwaitInstructionAwaiter<T>(i, parent, scheduler);

		/// <summary>
		/// Link the <see cref="UnityAsync.IAwaitInstruction"/>'s lifespan to a <see cref="UnityEngine.Object"/>.
		/// </summary>
		/// <returns>A continuation with updated params.</returns>
		public static AwaitInstructionAwaiter<T> ConfigureAwait<T>(this T i, Object parent) where T : IAwaitInstruction
			=> new AwaitInstructionAwaiter<T>(i, parent, FrameScheduler.Update);

		/// <summary>
		/// Configure the type of update cycle it should be evaluated on.
		/// </summary>
		/// <returns>A continuation with updated params.</returns>
		public static AwaitInstructionAwaiter<T> ConfigureAwait<T>(this T i, FrameScheduler scheduler) where T : IAwaitInstruction
			=> new AwaitInstructionAwaiter<T>(i, scheduler);
		
		/// <summary>
		/// Link the <see cref="UnityAsync.IAwaitInstruction"/>'s lifespan to a
		/// <see cref="System.Threading.CancellationToken"/> and configure the type of update cycle it should be
		/// evaluated on.
		/// </summary>
		/// <returns>A continuation with updated params.</returns>
		public static AwaitInstructionAwaiter<T> ConfigureAwait<T>(this T i, CancellationToken cancellationToken, FrameScheduler scheduler) where T : IAwaitInstruction
			=> new AwaitInstructionAwaiter<T>(i, cancellationToken, scheduler);

		/// <summary>
		/// Link the <see cref="UnityAsync.IAwaitInstruction"/>'s lifespan to a <see cref="System.Threading.CancellationToken"/>.
		/// </summary>
		/// <returns>A continuation with updated params.</returns>
		public static AwaitInstructionAwaiter<T> ConfigureAwait<T>(this T i, CancellationToken cancellationToken) where T : struct, IAwaitInstruction
			=> new AwaitInstructionAwaiter<T>(i, cancellationToken, FrameScheduler.Update);

		/// <summary>
		/// Encapsulate the <see cref="System.Threading.Tasks.Task"/> in a <see cref="UnityEngine.CustomYieldInstruction"/>
		/// so that it can be yielded in an IEnumerator coroutine.
		/// </summary>
		public static TaskYieldInstruction AsYieldInstruction(this Task t) => new TaskYieldInstruction(t);
		
		/// <summary>
		/// Encapsulate the <see cref="System.Threading.Tasks.Task{TResult}"/> in a <see cref="UnityEngine.CustomYieldInstruction"/>
		/// so that it can be yielded in an IEnumerator coroutine. The result can be obtained through
		/// <see cref="TaskYieldInstruction{T}.Current"/> after yielding.
		/// </summary>
		public static TaskYieldInstruction<T> AsYieldInstruction<T>(this Task<T> t) => new TaskYieldInstruction<T>(t);

		public static SynchronizationContextAwaiter GetAwaiter(this SynchronizationContext s) => new SynchronizationContextAwaiter(s);
		public static IEnumeratorAwaiter GetAwaiter(this IEnumerator e) => new IEnumeratorAwaiter(e);
		public static YieldInstructionAwaiter GetAwaiter(this YieldInstruction y) => new YieldInstructionAwaiter(y);
		public static ResourceRequestAwaiter GetAwaiter(this ResourceRequest r) => new ResourceRequestAwaiter(r);
		public static AsyncOperationAwaiter GetAwaiter(this AsyncOperation r) => new AsyncOperationAwaiter(r);
		
		public static AwaitInstructionAwaiter<T> GetAwaiter<T>(this T i) where T : struct, IAwaitInstruction => new AwaitInstructionAwaiter<T>(i);
		
		public static AwaitInstructionAwaiter<T> GetAwaiter<T>(in this AwaitInstructionAwaiter<T> a) where T : IAwaitInstruction => a;
	}
}