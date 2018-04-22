using System;

namespace UniHttp
{
	public class Progress : IProgress<long>
	{
		public ProgressState State { get; private set; }

		public long Current { get; private set; }

		public long? Total { get; private set; }

		public float Ratio
		{
			get {
				if(State == ProgressState.Done) {
					return 1f;
				}

				if(Total.HasValue) {
					return (float)Current / (float)Total.Value;
				}

				return 0f;
			}
		}

		public Progress()
		{
			this.Reset();
		}

		internal void Start(long? total = null)
		{
			this.Total = total;
			this.State = ProgressState.InProgress;
		}

		public void Report(long value)
		{
			this.Current = value;
		}

		internal void Finialize()
		{
			this.State = ProgressState.Done;
		}

		internal void Reset()
		{
			this.Current = 0;
			this.Total = null;
			this.State = ProgressState.Pending;
		}
	}

	public enum ProgressState : byte
	{
		Pending,
		InProgress,
		Done
	};
}
