namespace UniHttp
{
	public class Progress
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
			this.Read = 0;
			this.Total = null;
			this.State = ProgressState.Pending;
		}

		internal void Start(long? total = null)
		{
			this.Total = total;
			this.State = ProgressState.InProgress;
		}

		internal void Report(long amount)
		{
			this.Read = amount;
		}

		internal void Finialize()
		{
			this.State = ProgressState.Done;
		}
	}

	public enum ProgressState
	{
		Pending,
		InProgress,
		Done
	};

}
