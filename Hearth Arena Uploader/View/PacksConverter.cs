using Hearthstone_Deck_Tracker.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HearthArenaUploader.View
{
	public class PacksConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			ArenaRewardPacks[] rewards = value as ArenaRewardPacks[];
			return rewards.Count(pack => pack != ArenaRewardPacks.None).ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
