using System;
using System.Threading;

namespace UnityAsync
{
	public static partial class Await
	{
		/// <summary>
		/// Quick access to Unity's <see cref="System.Threading.SynchronizationContext"/>.
		/// </summary>
		public static SynchronizationContext UnitySyncContext() => AsyncManager.UnitySyncContext;
		
		/// <summary>
		/// Quick access to the background <see cref="System.Threading.SynchronizationContext"/>.
		/// </summary>
		public static SynchronizationContext BackgroundSyncContext() => AsyncManager.BackgroundSyncContext;

		/// <summary>
		/// Convenience function to skip a single frame, equivalent to Unity's <c>yield return null</c>.
		/// </summary>
		public static WaitForFrames NextUpdate() => new WaitForFrames(1);
		
		/// <summary>
		/// Convenience function to skip a number of frames, equivalent to multiple <c>yield return null</c>s.
		/// </summary>
		public static WaitForFrames Updates(int count) => new WaitForFrames(count);

		/// <summary>
		/// Convenience function to skip a single frame and continue in the LateUpdate loop.
		/// </summary>
		public static Continuation<WaitForFrames> NextLateUpdate() => new WaitForFrames(1).ConfigureAwait(FrameScheduler.LateUpdate);
		
		/// <summary>
		/// Convenience function to skip multiple frames and continue in the LateUpdate loop.
		/// </summary>
		public static Continuation<WaitForFrames> LateUpdates(int count) => new WaitForFrames(count).ConfigureAwait(FrameScheduler.LateUpdate);

		/// <summary>
		/// Convenience function to skip a single FixedUpdate frame and continue in the FixedUpdate loop. Equivalent to
		/// Unity's <see cref="UnityEngine.WaitForFixedUpdate"/>.
		/// </summary>
		public static Continuation<WaitForFrames> NextFixedUpdate() => new WaitForFrames(1).ConfigureAwait(FrameScheduler.FixedUpdate);
		
		/// <summary>
		/// Convenience function to skip multiple FixedUpdate frames and continue in the FixedUpdate loop.
		/// </summary>
		public static Continuation<WaitForFrames> FixedUpdates(int count) => new WaitForFrames(count).ConfigureAwait(FrameScheduler.FixedUpdate);

		/// <summary>
		/// Convenience function to wait for a number of in-game seconds before continuing, equivalent to Unity's
		/// <see cref="UnityEngine.WaitForSeconds"/>.
		/// </summary>
		public static WaitForSeconds Seconds(float duration) => new WaitForSeconds(duration);
		
		/// <summary>
		/// Convenience function to wait for a number of unscaled seconds before continuing. Equivalent to Unity's
		/// <see cref="UnityEngine.WaitForSecondsRealtime"/>.
		/// </summary>
		public static WaitForSecondsRealtime SecondsRealtime(float duration) => new WaitForSecondsRealtime(duration);

		/// <summary>
		/// Convenience function to wait for a condition to return true. Equivalent to Unity's
		/// <see cref="UnityEngine.WaitUntil"/>.
		/// </summary>
		public static WaitUntil Until(Func<bool> condition) => new WaitUntil(condition);
		
		/// <summary>
		/// Convenience function to wait for a condition to return false. Equivalent to Unity's
		/// <see cref="UnityEngine.WaitWhile"/>.
		/// </summary>
		public static WaitWhile While(Func<bool> condition) => new WaitWhile(condition);
	}
}