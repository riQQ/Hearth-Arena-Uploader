namespace HearthArenaUploader.Data
{
	public class ArenaRewards
	{
		public ArenaRewards(int gold, int dust, int packs, int standardCards, int goldenCards)
		{
			this.gold = gold;
			this.dust = dust;
			this.packs = packs;
			this.standardCards = standardCards;
			this.goldenCards = goldenCards;
		}

		private int gold;
		public int Gold
		{
			get { return gold; }
			set { gold = value; }
		}

		private int dust;

		public int Dust
		{
			get { return dust; }
			set { dust = value; }
		}


		private int packs;

		public int Packs
		{
			get { return packs; }
			set { packs = value; }
		}


		private int standardCards;

		public int StandardCards
		{
			get { return standardCards; }
			set { standardCards = value; }
		}


		private int goldenCards;

		public int GoldenCards
		{
			get { return goldenCards; }
			set { goldenCards = value; }
		}


	}
}
