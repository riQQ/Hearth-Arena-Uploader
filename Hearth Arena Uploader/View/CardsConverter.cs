using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Controls.Stats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HearthArenaUploader.View
{
	public class CardsConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			ArenaReward.CardReward[] rewards = value as ArenaReward.CardReward[];
			return rewards.Count(card => card != null && !card.Golden).ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
