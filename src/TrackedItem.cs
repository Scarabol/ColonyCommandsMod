
namespace ColonyCommands {

	// TrackedItem for the StatisticManager
	public class TrackedItem
	{
		public ItemTypes.ItemType item;

		// interval means days, to report stats every day, every two days and so on
		public ushort interval;

		// store current and last value. no long term statistics yet
		private int[] itemAmount = new int[2];

		// Constructor
		public TrackedItem(ItemTypes.ItemType item, ushort interval)
		{
			this.item = item;
			this.interval = interval;

			for (int i = 0; i < itemAmount.Length; ++i) {
				this.itemAmount[i] = 0;
			}
		}

		public string Name
		{
			get {
				return this.item.Name;
			}
		}

		// increase, decrease and delta are all calculated based on the interval
		public bool DidIncrease
		{
			get {
				return (this.itemAmount[0] > this.itemAmount[1]);
			}
		}

		public bool DidDecrease
		{
			get {
				return (this.itemAmount[0] < this.itemAmount[1]);
			}
		}

		public string GetDeltaFormatted()
		{
			int delta = itemAmount[0] - itemAmount[1];
			if (System.Math.Abs(delta) > 10000) {
				return string.Format("{0:D2}K", delta / 1000);
			}
			return string.Format("{0:D}", delta);
		}

		// update new daily value and shift the old values
		public void Update(int newValue)
		{
			for (int i = itemAmount.Length - 1; i > 0; --i) {
				this.itemAmount[i] = this.itemAmount[i - 1];
			}
			this.itemAmount[0] = newValue;
			return;
		}
	}

}

