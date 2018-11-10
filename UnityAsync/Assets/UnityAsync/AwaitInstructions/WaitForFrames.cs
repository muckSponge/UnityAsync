#if UNITY_EDITOR
using UnityEngine;
#endif

namespace UnityAsync
{
	public struct WaitForFrames : IAwaitInstruction
	{
		readonly int finishFrame;

		bool IAwaitInstruction.IsCompleted() => finishFrame <= AsyncManager.CurrentFrameCount;
		
		/// <summary>
		/// Waits for the specified number of frames to pass before continuing.
		/// </summary>
		public WaitForFrames(int count)
		{
			#if UNITY_EDITOR
			if(count <= 0)
			{
				count = 1;
				Debug.LogError($"{nameof(count)} should be greater than 0. This check will only appear in edit mode.");
			}
			#endif
			
			finishFrame = AsyncManager.CurrentFrameCount + count;
		}
		
		public Continuation<WaitForFrames> GetAwaiter() => new Continuation<WaitForFrames>(this);
	}
}