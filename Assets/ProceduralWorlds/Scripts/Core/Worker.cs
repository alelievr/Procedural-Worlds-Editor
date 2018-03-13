using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public static class Worker
{

	public static int			workerCount {get; private set;}
	public static int			checkTimeout = 10; //ms

	static Queue< Completed >	completedQueue = new Queue< Completed >();

	private class Completed
	{
		public Action< object >		callback;
		public object				param;

		public Completed(Action< object > callback, object param)
		{
			this.param = param;
			this.callback = callback;
		}
	}

	public static void		EnqueueTask(Func< object > task, Action< object > callback)
	{
		WaitCallback	wc = (object o) => {
			object res = task();
			completedQueue.Enqueue(new Completed(callback, res));
		};

		ThreadPool.QueueUserWorkItem(wc);
	}

	public static void StopAllWorkers(int id = -1)
	{
	}

	static void				UpdateWorkCallbacks()
	{
		Completed		c;

		while (completedQueue.Count != 0)
		{
			c = completedQueue.Dequeue();

			c.callback(c.param);
		}
	}
}
