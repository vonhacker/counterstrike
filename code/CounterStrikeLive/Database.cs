using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows.Controls;
using System.IO;
using System.Xml.Serialization;
using FarseerGames.FarseerPhysics.Mathematics;
using System.Windows.Media.Imaging;

namespace CounterStrikeLive
{
    
	public class Database
	{
		public interface ILoad
		{
			void Load();
		}
		
		public List<AnimatedBitmap> _Sparks = new List<AnimatedBitmap>();
		public List<AnimatedBitmap> _Blood = new List<AnimatedBitmap>();
		[XmlIgnore]
		public static List<ILoad> _ILoads = new List<ILoad>();
		public List<Player> _PlayerModels = new List<Player>();
		public AnimatedBitmap _Gun;
		public enum PlayerType : byte { unknown = 23, TPlayer = 84, CPlayer = 85 }
		public Player GetPlayer(PlayerType _PlayerType)
		{
			return _PlayerModels.FirstOrDefault(p => p._PlayerType == _PlayerType);
		}
		public class Player
		{
			public PlayerType _PlayerType;
			public AnimatedBitmap _PlayerRun;
			public AnimatedBitmap _PlayerDie;
			public AnimatedBitmap _PlayerStay;
		}
		
		public class AnimatedBitmap : ILoad
		{
			public float _Width;
			public float _Height;
			public float _y;
			public float _x;
			public AnimatedBitmap()
			{
				if (!Database._ILoads.Contains(this))
					Database._ILoads.Add(this);
			}
			public Image GetImage()
			{
				Image _Image = new Image();
				Canvas.SetLeft(_Image, -_Width / 2 + _x);
				Canvas.SetTop(_Image, -_Height / 2 + _y);
				_Image.Width = _Width;
				_Image.Height = _Height;
				_Image.Source = _BitmapImages[0];
				return _Image;
			}
			public List<string> _Bitmaps = new List<string>();
			[XmlIgnore]
			public List<BitmapImage> _BitmapImages = new List<BitmapImage>();
			bool _Loaded;
			public void Load()
			{
				if (_Loaded) throw new Exception("Break");
				_Loaded = true;
				foreach (string _path in _Bitmaps)
				{
                    BitmapImage _BitmapImage = new BitmapImage();
                    _BitmapImage.SetSource(App.GetResourceStream(new Uri(_path, UriKind.Relative)).Stream);                                                            
					_BitmapImages.Add(_BitmapImage);
				}
			}			
		}
	}
}
