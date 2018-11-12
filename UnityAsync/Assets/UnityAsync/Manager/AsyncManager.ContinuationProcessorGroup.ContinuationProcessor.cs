namespace UnityAsync
{
	public partial class AsyncManager
	{
		partial class ContinuationProcessorGroup
		{
			const int MaxQueueSize = 500000;
			
			class ContinuationProcessor<T> : IContinuationProcessor where T : IContinuation
			{
				public static ContinuationProcessor<T> instance;

				T[] currentQueue;
				T[] futureQueue;

				int futureCount;

				public ContinuationProcessor()
				{
					currentQueue = new T[MaxQueueSize];
					futureQueue = new T[MaxQueueSize];
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
						var c = currentQueue[i];

						if(!c.Evaluate())
						{
							futureQueue[futureCount] = c;
							++futureCount;
						}
					}
				}

				public void Add(T cont)
				{
					futureQueue[futureCount] = cont;
					++futureCount;
				}
			}
		}
	}
}