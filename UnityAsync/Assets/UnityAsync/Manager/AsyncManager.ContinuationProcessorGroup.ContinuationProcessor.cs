using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityAsync
{
	public partial class AsyncManager
	{
		partial class ContinuationProcessorGroup
		{
			const int MaxQueueSize = 1 << 16;

			class ContinuationProcessor<T> : IContinuationProcessor where T : IAwaitInstructionAwaiter
			{
				public static ContinuationProcessor<T> instance;

				T[] currentQueue, futureQueue;
				int currentCount, currentIndex, futureCount, maxIndex;

				public ContinuationProcessor(int capacity)
				{
					AssertQueueSize(capacity);
					
					maxIndex = capacity - 1;
					
					currentQueue = new T[capacity];
					futureQueue = new T[capacity];
				}

				public void Process()
				{
					currentCount = futureCount;
					futureCount = 0;
					
					// swap queues
					var tmp = currentQueue;
					currentQueue = futureQueue;
					futureQueue = tmp;

					for(; currentIndex < currentCount; ++currentIndex)
					{
						ref var c = ref currentQueue[currentIndex];

						if(!c.Evaluate())
						{
							// this is hottest path so we don't do a bounds check here (see Add)
							futureQueue[futureCount] = c;
							++futureCount;
						}
					}

					currentIndex = 0;
					currentCount = 0;
					
					Array.Clear(currentQueue, 0, currentCount);
				}

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public void Add(in T cont)
				{
					if(InUnityContext)
						AddFast(cont);
					else
						AddThreadSafe(cont);
				}

				// only call in UnitySynchronizationContext - not thread safe
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				void AddFast(in T cont)
				{
					try
					{
						// we might have awaiters yet to be processed this frame; when they are re-added we skip any
						// bounds checks as an optimisation, so we need to make sure the queue has enough space to
						// re-add them all in case none of them have finished
						int potentialFutureCount = futureCount + currentCount - currentIndex;
						
						if(potentialFutureCount >= maxIndex)
						{
							AssertQueueSize(potentialFutureCount + 1);
							
							int newQueueSize = Math.Min(MaxQueueSize, Math.Max(potentialFutureCount, futureQueue.Length * 3 / 2));
						
							Array.Resize(ref futureQueue, newQueueSize);
							Array.Resize(ref currentQueue, newQueueSize);
						
							maxIndex = newQueueSize - 1;
						}

						futureQueue[futureCount] = cont;
						++futureCount;
					}
					catch(Exception e)
					{
						Debug.LogException(e);
					}
				}

				// use when you may be adding an awaiter from outside of UnitySynchronizationContext
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				async void AddThreadSafe(T cont)
				{
					await UnitySyncContext;

					AddFast(cont);
				}

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				static void AssertQueueSize(int queueSize)
				{
					if(queueSize > MaxQueueSize)
						throw new InvalidOperationException($"Cannot exceed queue size of {MaxQueueSize}.");
				}
			}
		}
	}
}