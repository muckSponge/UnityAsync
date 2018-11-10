using System.Collections.Generic;

namespace UnityAsync
{
	public partial class AsyncManager
	{
		partial class ContinuationProcessorGroup
		{
			class ContinuationProcessor<T> : IContinuationProcessor where T : IContinuation
			{
				public static ContinuationProcessor<T> instance;

				Queue<T> currentQueue;
				Queue<T> futureQueue;

				public ContinuationProcessor()
				{
					currentQueue = new Queue<T>(10);
					futureQueue = new Queue<T>(10);
				}

				public void Process()
				{
					// swap queues
					var tmp = currentQueue;
					currentQueue = futureQueue;
					futureQueue = tmp;
					
					int count = currentQueue.Count;

					for(int i = 0; i < count; ++i)
					{
						var c = currentQueue.Dequeue();

						if(!c.Evaluate())
							futureQueue.Enqueue(c);
					}
				}

				public void Add(T cont) => futureQueue.Enqueue(cont);
			}
		}
	}
}