using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public static class PWWorker {

	public static int			workerCount {get; private set;}
	public static int			checkTimeout = 10; //ms

	static Queue< Todo >		todos = new Queue< Todo >();
	static List< Worker >		workers = new List< Worker >();
	static bool					working = false;

	class Worker
	{
		public Thread			thread = null;
		public AutoResetEvent	waitHandle = null;
		public bool				working = false;
		public bool				mustQuit = false;
		public int				id = -2;

		public Worker()
		{
			thread = new Thread(() => ThreadWorker(this));
			thread.Start();
			waitHandle = new AutoResetEvent(false);
		}
	}

	class Todo
	{
		public Func< object >		task;
		public Action< object >		callback;
		public int					id;

		public Todo(Func< object > task, Action< object > callback, int id)
		{
			this.task = task;
			this.callback = callback;
			this.id = id;
		}
	}

	static PWWorker()
	{
		if (!working)
		{
			SetWorkerCount(8);
			Work();
		}
	}

	public static void		SetWorkerCount(int wCount)
	{
		workerCount = wCount;

		if (workers.Count < wCount)
			for (int i = 0; i < wCount - workers.Count; i++)
				workers.Add(new Worker());
		else if (workers.Count > wCount)
		{
			for (int i = 0; i < workers.Count - wCount; i++)
				workers[i].mustQuit = true;
		}
	}

	public static void		EnqueueTask(Func< object > task, Action< object > callback, int id)
	{
		todos.Enqueue(new Todo(task, callback, id));
	}

	public static void StopAllWorkers(int id = -1)
	{
		/*lock (workers) {
			workers.RemoveAll(w => {
				if (id == -1 || w.id == id)
				{
					Debug.Log("removing thread " + id);
					w.thread.Abort();
					return true;
				}
				return false;
			});
		}*/
		SetWorkerCount(workerCount);
	}

	static void				Work()
	{
		(new Thread(() => {
			while (true)
			{
				lock (todos)
				{
					if (todos.Count > 0)
						foreach (var worker in workers)
						{
							Debug.Log("worker state: " + worker.working);
							if (worker.working == false)
							{
								Debug.Log("woke up worker !");
								worker.waitHandle.Set();
							}
						}
				}
				Thread.Sleep(checkTimeout);
			}
		})).Start();
	}

	static void ThreadWorker(Worker me)
	{
		Debug.Log("started threads !");
		while (true)
		{
			if (me.mustQuit == true)
				break ;
			me.working = false;
			me.waitHandle.WaitOne();

			Debug.Log("working !");
			Todo	todo = null;
			lock (todos) {
				if (todos.Count != 0)
					todo = todos.Dequeue();
			}
			if (todo != null)
			{
				me.id = todo.id;
				me.working = true;
				try {
					todo.callback(todo.task());
				} catch (Exception e) {
					Debug.LogError(e);
				}
			}
		}
	}
}
