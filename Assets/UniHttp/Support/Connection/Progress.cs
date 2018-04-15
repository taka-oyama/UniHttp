using System;

namespace UniHttp
{
	public class Progress : IProgress<long>
	{
		public ProgressState State { get; private set; }

		public long Read { get; private set; }

		public long? Total { get; private set; }

		public float Ratio
		{
			get {
				if(State == ProgressState.Done) {
					return 1f;
				}

				if(Total.HasValue) {
					return (float)Read / (float)Total.Value;
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
			this.Read = value;
		}

		internal void Finialize()
		{
			this.State = ProgressState.Done;
		}

		internal void Reset()
		{
			this.Read = 0;
			this.Total = null;
			this.State = ProgressState.Pending;
		}
	}

	public enum ProgressState
	{
		Pending,
		InProgress,
		Done
	};
}
