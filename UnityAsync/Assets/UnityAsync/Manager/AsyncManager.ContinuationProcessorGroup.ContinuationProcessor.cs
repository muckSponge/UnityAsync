using System;
using System.Runtime.CompilerServices;

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
				int futureCount, maxIndex;

				public ContinuationProcessor(int capacity)
				{
					AssertQueueSize(capacity);
					
					maxIndex = capacity - 1;
					
					currentQueue = new T[capacity];
					futureQueue = new T[capacity];
				}

				public void Process()
				{
					int count = futureCount;
					futureCount = 0;
					
					// swap queues
					var tmp = currentQueue;
					currentQueue = futureQueue;
					futureQueue = tmp;

					for(int i = 0; i < count; ++i)
					{
						ref var c = ref currentQueue[i];

						if(!c.Evaluate())
						{
							futureQueue[futureCount] = c;
							++futureCount;
						}
					}
					
					Array.Clear(currentQueue, 0, count);
				}

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public void Add(in T cont)
				{
					if(futureCount == maxIndex)
					{
						AssertQueueSize(futureCount + 1);
						
						int newQueueSize = Math.Min(MaxQueueSize, futureQueue.Length * 3 / 2);
					
						Array.Resize(ref futureQueue, newQueueSize);
						Array.Resize(ref currentQueue, newQueueSize);
					
						maxIndex = newQueueSize - 1;
					}
					
					futureQueue[futureCount] = cont;
					++futureCount;
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