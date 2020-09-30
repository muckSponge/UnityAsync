using System.Collections;
using UnityEngine;
using System.Threading;

namespace UnityAsync
{
    [AddComponentMenu("")] // don't show in menu
	public partial class AsyncManager : MonoBehaviour
	{
		/// <summary>
		/// The frame count in the currently active update loop.
		/// </summary>
		public static int CurrentFrameCount { get; private set; }
		
		/// <summary>
		/// The time in the currently active update loop.
		/// </summary>
		public static float CurrentTime { get; private set; }
		
		/// <summary>
		/// The unscaled time in the currently active update loop.
		/// </summary>
		public static float CurrentUnscaledTime { get; private set; }
		
		/// <summary>
		/// Unity's <see cref="System.Threading.SynchronizationContext"/>.
		/// </summary>
		public static SynchronizationContext UnitySyncContext { get; private set; }
		
		/// <summary>
		/// Background (thread pool) <see cref="System.Threading.SynchronizationContext"/>.
		/// </summary>
		public static SynchronizationContext BackgroundSyncContext { get; private set; }
		
		/// <summary>
		/// Returns true if called from Unity's <see cref="System.Threading.SynchronizationContext"/>.
		/// </summary>
		public static bool InUnityContext => Thread.CurrentThread.ManagedThreadId == unityThreadId;

		public static AsyncManager Instance { get; private set; }

		static int unityThreadId, updateCount, lateCount, fixedCount;
		static ContinuationProcessorGroup updates, lateUpdates, fixedUpdates;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void Initialize()
		{
			// clear statics
			
			CurrentFrameCount = updateCount = lateCount = fixedCount = 0;

			updates?.CleanUp();
			lateUpdates?.CleanUp();
			fixedUpdates?.CleanUp();
		
			unityThreadId = Thread.CurrentThread.ManagedThreadId;
			UnitySyncContext = SynchronizationContext.Current;

			BackgroundSyncContext = new SynchronizationContext(); // TODO: confirm this produces desired behaviour

			updates = new ContinuationProcessorGroup();
			lateUpdates = new ContinuationProcessorGroup();
			fixedUpdates = new ContinuationProcessorGroup();

			if(Instance)
				Destroy(Instance);
			
			Instance = new GameObject("Async Manager").AddComponent<AsyncManager>();
			if(!Application.isEditor) // DontDestroyOnLoad can not be called in editor mode
				DontDestroyOnLoad(Instance);
		}

		/// <summary>
		/// Initializes the manager explicitly. Typically used when running Unity Editor tests (NUnit unit tests).
		/// </summary>
		public static void InitializeForEditorTests()
		{
			Initialize();
			
			// Do not run tasks in background when testing:
			BackgroundSyncContext = UnitySyncContext;
		}

		/// <summary>
		/// Queues a continuation.
		/// Intended for internal use only - you shouldn't need to invoke this.
		/// </summary>
		public static void AddContinuation<T>(in T cont) where T : IAwaitInstructionAwaiter
		{
			switch(cont.Scheduler)
			{
				case FrameScheduler.Update:
					updates.Add(cont);
					break;

				case FrameScheduler.LateUpdate:
					lateUpdates.Add(cont);
					break;

				case FrameScheduler.FixedUpdate:
					fixedUpdates.Add(cont);
					break;
			}
		}

		/// <summary>
		/// Start a coroutine from any context without requiring a MonoBehaviour.
		/// </summary>
		public new static Coroutine StartCoroutine(IEnumerator coroutine) => ((MonoBehaviour)Instance).StartCoroutine(coroutine);

		void Update()
		{
			CurrentFrameCount = ++updateCount;
			
			if(CurrentFrameCount <= 1)
				return;
			
			CurrentTime = Time.time;
			CurrentUnscaledTime = Time.unscaledTime;
			
			updates.Process();
		}

		void LateUpdate()
		{
			CurrentFrameCount = ++lateCount;
			
			if(CurrentFrameCount <= 1)
				return;
			
			lateUpdates.Process();
		}

		void FixedUpdate()
		{
			CurrentFrameCount = ++fixedCount;
			
			if(CurrentFrameCount <= 1)
				return;
						
			fixedUpdates.Process();
		}
	}
}