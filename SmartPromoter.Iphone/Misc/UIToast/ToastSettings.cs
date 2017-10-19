using System.Drawing;
namespace SmartPromoter.Iphone
{
		public class ToastSettings
		{
			public ToastSettings()
			{
				Duration = 500;
				Gravity = ToastGravity.Center;
			}

			public int Duration
			{
				get;
				set;
			}

			public double DurationSeconds
			{
				get { return (double)Duration / 1000; }

			}

			public ToastGravity Gravity
			{
				get;
				set;
			}

			public PointF Position
			{
				get;
				set;
			}


		}
}


