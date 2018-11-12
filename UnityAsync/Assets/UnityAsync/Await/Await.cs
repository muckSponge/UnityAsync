using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace UnityAsync
{
	public static partial class Await
	{
		static readonly Continuation<WaitForFrames> nextUpdate = new Continuation<WaitForFrames>(new WaitForFrames(1));
		static readonly Continuation<WaitForFrames> nextLateUpdate = new Continuation<WaitForFrames>(new WaitForFrames(1), FrameScheduler.LateUpdate);
		static readonly Continuation<WaitForFrames> nextFixedUpdate = new Continuation<WaitForFrames>(new WaitForFrames(1), FrameScheduler.FixedUpdate);
		
		/// <summary>
		/// Quick access to Unity's <see cref="System.Threading.SynchronizationContext"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SynchronizationContext UnitySyncContext() => AsyncManager.UnitySyncContext;
		
		/// <summary>
		/// Quick access to the background <see cref="System.Threading.SynchronizationContext"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SynchronizationContext BackgroundSyncContext() => AsyncManager.BackgroundSyncContext;

		/// <summary>
		/// Convenience function to skip a single frame, equivalent to Unity's <c>yield return null</c>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Continuation<WaitForFrames> NextUpdate() => nextUpdate;
		
		/// <summary>
		/// Convenience function to skip a number of frames, equivalent to multiple <c>yield return null</c>s.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Continuation<WaitForFrames> Updates(int count) => new Continuation<WaitForFrames>(new WaitForFrames(count));

		/// <summary>
		/// Convenience function to skip a single frame and continue in the LateUpdate loop.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Continuation<WaitForFrames> NextLateUpdate() => nextLateUpdate;
		
		/// <summary>
		/// Convenience function to skip multiple frames and continue in the LateUpdate loop.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Continuation<WaitForFrames> LateUpdates(int count) => new Continuation<WaitForFrames>(new WaitForFrames(count), FrameScheduler.LateUpdate);

		/// <summary>
		/// Convenience function to skip a single FixedUpdate frame and continue in the FixedUpdate loop. Equivalent to
		/// Unity's <see cref="UnityEngine.WaitForFixedUpdate"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Continuation<WaitForFrames> NextFixedUpdate() => nextFixedUpdate;
		
		/// <summary>
		/// Convenience function to skip multiple FixedUpdate frames and continue in the FixedUpdate loop.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Continuation<WaitForFrames> FixedUpdates(int count) => new Continuation<WaitForFrames>(new WaitForFrames(count), FrameScheduler.FixedUpdate);

		/// <summary>
		/// Convenience function to wait for a number of in-game seconds before continuing, equivalent to Unity's
		/// <see cref="UnityEngine.WaitForSeconds"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static WaitForSeconds Seconds(float duration) => new WaitForSeconds(duration);
		
		/// <summary>
		/// Convenience function to wait for a number of unscaled seconds before continuing. Equivalent to Unity's
		/// <see cref="UnityEngine.WaitForSecondsRealtime"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static WaitForSecondsRealtime SecondsRealtime(float duration) => new WaitForSecondsRealtime(duration);

		/// <summary>
		/// Convenience function to wait for a condition to return true. Equivalent to Unity's
		/// <see cref="UnityEngine.WaitUntil"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static WaitUntil Until(Func<bool> condition) => new WaitUntil(condition);
		
		/// <summary>
		/// Convenience function to wait for a condition to return false. Equivalent to Unity's
		/// <see cref="UnityEngine.WaitWhile"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static WaitWhile While(Func<bool> condition) => new WaitWhile(condition);
	}
}