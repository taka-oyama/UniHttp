using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;
using System.IO;

namespace UniHttp.FileOperation
{
	internal class Scheduler
	{
		static object locker = new object();

		DirectoryInfo baseDirectory;
		List<Operation> list = new List<Operation>();
		SynchronizationContext contextThread;

		internal Scheduler(DirectoryInfo baseDirectory, SynchronizationContext context = null)
		{
			this.baseDirectory = baseDirectory;
			this.contextThread = context ?? SynchronizationContext.Current;
		}

		internal bool Exists(string path)
		{
			if(File.Exists(string.Format(baseDirectory.FullName + path))) {
				return true;
			}
			bool result = false;
			for(int i = 0; i < list.Count; i++) {
				if(list[0].targetPath == path) {
					if(list[0] is WriteOperation) {
						result = true;
					} else if(list[0] is DeleteOperation) {
						result = false;
					}
				}
			}
			return result;
		}

		internal void Read(string path, Action<byte[]> callback)
		{
			Enqueue(new ReadOperation(path, callback));
		}

		internal void Write(string path, byte[] data)
		{
			Enqueue(new WriteOperation(path, data));
		}

		internal void Delete(string path)
		{
			Enqueue(new DeleteOperation(path));
		}

		void Enqueue(Operation schedule)
		{
			list.Add(schedule);
		}

		void DequeueAndExecute()
		{
			Execute(list[0]);
			list.RemoveAt(0);
		}

		void Execute(Operation operation)
		{
			ThreadPool.QueueUserWorkItem(nil => {
				lock(locker) {
					try {
						if(operation.callback != null) {
							operation.callback.Invoke(operation.Execute());
						}
					}
					catch(IOException exception) {
						ThrowOnMainThread(exception);
					}
				}
			});
		}

		void ThrowOnMainThread(Exception exception)
		{
			contextThread.Post(e => { throw (Exception)e; }, exception);
		}
	}
}
