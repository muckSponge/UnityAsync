#UnityAsync
UnityAsync is a coroutine framework for Unity built around the async API. This is not only a more efficient and effective replacement of Unity's IEnumerator coroutines, but also seamlessly integrates Unity with .NET 4+ asynchronous APIs.

With this library you can:
* Write allocation-free coroutines
* Seamlessly integrate with Task-based and async APIs
* Integrate with IEnumerator-based coroutines and YieldInstructions
* Easily switch sync contexts (main to background and vice-versa)
* Define your own custom await instructions (allocation free)
* Return results at the end of your coroutine
#Performance
Rest assured; UnityAsync coroutines will generally always perform better than Unity's built-in coroutines because:
* They rarely cause heap allocations
* They don't weave in and out of native code
* They don't rely on a monolithic state machine

Benchmarks with 100,000 simultaneous update loop coroutines show a performance increase of over 150% (that's 2.5x), and this doesn't test against `YieldInstructions`. A test with uncached `WaitForSeconds` using a random number as input yields a performance increase of over 290% (3.9x). The benchmarks included the time it took to instantiate the coroutines.

The single downside is that if you want to nest multiple coroutines inside each other, you must store a reference to them, which means returning a `Task` object. This causes a 226 byte heap allocation, whereas Unity's coroutines allocate less memory (but they still allocate on the heap). This often isn't a problem because nested coroutines rarely execute in their entirety every frame and these allocations are still tiny.

#Usage
##Replacing existing coroutines
Let's say we want to replace a pretty straight-forward update loop IEnumerator coroutine:
```c#
using UnityEngine;
using System.Collections;

...

IEnumerator UpdateLoop()
{
	while(true)
		yield return null;
}

void Start()
{
	StartCoroutine(UpdateLoop());
}

...

```
UnityAsync coroutines are defined by async methods, which can return void, `Task`, or `Task<TResult>`:
```c#
using UnityEngine;
using UnityAsync;

...

async void UpdateLoop()
{
	while(true)
		await new WaitForFrames(1);
}

void Start()
{
	UpdateLoop();
}

...
```
Easy-peasy, right? WaitForFrames is an `IAwaitInstruction`. When you await it, it spawns a `Continuation<T>`, which is (automatically) inserted into a list and evaluated every frame until it is finished; in this case it will take one frame. We could return Task instead of void if we wanted to store the coroutine.

##Returning a result
Let's say we have some work we want to perform across the main thread, but it's too much to perform in a single frame. We can create a coroutine for this and return the result of the work once it is finished.
```c#
using UnityEngine;
using UnityAsync;
using System.Threading.Tasks;

...

async Task<int> GenerateResult()
{
	// contrived examples are the best examples, right?
	int result = 1;
	
	for(int i = 0; i < 10; ++i)
	{
		for(int j = 0; j < 100000; ++j)
			result = Mathf.Sin(result * j); 
			
		await new WaitForFrames(1);
	}
	
	return result;
}

async void Start()
{
	int result = await GenerateResult();

	// ...10 frames later
	Debug.Log(result);
}

...
```
We are awaiting the result in Start so it needs to be an async method (this doesn't impact on how Unity calls it). We now return `Task<int>` and the result is available after 10 frames.

##Switching contexts
Sometimes you'll want to perform some task on the thread pool and return to the main thread once this is completed without blocking. This could be done via `Task.ConfigureAwait` but now you can await directly on a `SynchronizationContext` which makes it super easy to swap back and forth:
```c#
using UnityAsync;

...

async void Start()
{
	// do some work on the thread pool
	await Await.BackgroundSyncContext();
	
	// ...
	
	// resume on the main thread
	await Await.UnitySyncContext();
	
	// update Transforms, etc.
}

...

```
`Await` actually contains many shortcut functions to streamline your code.

##ConfigureAwait
Just like with `Task`s, UnityAsync `Continuation<T>`s can be configured. You can set the scheduler (Update, LateUpdate, FixedUpdate) and also link its lifespan to a `UnityEngine.Object` or `CancellationToken`.
```c#
using UnityEngine;
using UnityAsync;

...

async void Start()
{
	// continuation will finish if:
	// - 10 seconds pass or
	// - this MonoBehaviour is destroyed
	// time is evaluated every fixed update
	await Await.Seconds(10).ConfigureAwait(this, FrameScheduler.FixedUpdate);
	
	// ...we are now in fixed update
}

...

```
##Yielding a task
You may run into a situation where some of your coroutine code is async, but it is called from an `IEnumerator`. In such a situation you can use `Task.AsYieldInstruction` or `Task<>.AsYieldInstruction`.
```c#
using UnityEngine;
using UnityAsync;

...

IEnumerator Start()
{
	yield return new WaitForSeconds(1);
	
	Debug.Log("Click to continue...");
	
	yield return WaitForMouse().AsYieldInstruction();
	
	Debug.Log("Click!");
}

async Task WaitForMouse()
{
	await Await.Until(() => Input.GetMouseDown(0));
}

...

```

#Await instructions / awaitables

Built-in:
* `WaitForFrames`
* `WaitForSeconds`
* `WaitForUnscaledSeconds`
* `WaitUntil`
* `WaitWhile`

Unity:
* `IEnumerator`*
* `YieldInstruction`*
* `AsyncOperation`
* `ResourceRequest`**

Others:
* `Task`
* `Task<>`
* …anything that implements `GetAwaiter()`

*Will spin up a Unity `Coroutine`, causing allocations.

** Very small delegate allocation.

Anything that implements `IAwaitInstruction` can be awaited and will be evaluated in Update, LateUpdate, or FixedUpdate. This is how the built-in await instructions are implemented. The advantage over `Task`s or `CustomYieldInstruction`s is they don't cause any allocations if implemented as structs.
# Custom await instructions
You can implement your own await instructions by implementing `IAwaitInstruction`.
```c#
public interface IAwaitInstruction
{
	bool IsCompleted();
}
```
It's dead simple, just place your behaviour in IsCompleted and return `true` when your criteria are met. `IsCompleted` will be evaluated every frame (depending on the `FrameScheduler`). Use a struct to avoid unnecessary allocations.

One more thing to note; you will want to implement `GetAwaiter()` so that the instruction can encapsulate itself in a `Continuation` when awaited. It is the `Continuation` which actually ends up being awaited - as a kind of decoration to make custom await instructions more straight-forward to implement (the alternative is inheritance which causes heap allocations).

```c#
using UnityAsync;

public struct WaitForTimeSpan : IAwaitInstruction
{
	readonly float finishTime;

	bool IAwaitInstruction.IsCompleted() => AsyncManager.currentTime >= finishTime;
	
	public WaitForFrames(TimeSpan timeSpan)
	{
		finishFrame = AsyncManager.currentTime + (float)timeSpan.TotalSeconds;
	}
	
	public Continuation<WaitForTimeSpan> GetAwaiter() => new Continuation<WaitForTimeSpan>(this);
}
```