using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityAsync
{
	public partial class AsyncManager
	{
		partial class ContinuationProcessorGroup
		{
			const int InitialCapacity = 1 << 10;
			
			interface IContinuationProcessor
			{
				void Process();
				void CleanUpStatics();
			}

			readonly List<IContinuationProcessor> processors;

			public ContinuationProcessorGroup()
			{
				processors = new List<IContinuationProcessor>(16);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Add<T>(in T cont) where T : IAwaitInstructionAwaiter
			{
				var p = ContinuationProcessor<T>.instance;

				if(p == null)
				{
					p = ContinuationProcessor<T>.instance = new ContinuationProcessor<T>(InitialCapacity);
					processors.Add(ContinuationProcessor<T>.instance);
				}

				p.Add(cont);
			}

			public void Process()
			{
				for(int i = 0; i < processors.Count; ++i)
					processors[i].Process();
			}

			public void CleanUp()
			{
				foreach(var p in processors)
					p.CleanUpStatics();
			}
		}
	}
}