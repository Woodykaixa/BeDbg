namespace BeDbg.Util
{
	public abstract class NeedRelease
	{
		protected bool IsReleased = false;

		public abstract void OnRelease();

		public void Release()
		{
			if (IsReleased)
			{
				return;
			}

			try
			{
				OnRelease();
			}
			finally
			{
				IsReleased = true;
			}
		}
	}
}