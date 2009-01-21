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
		public Sound _ShootSound;
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
		public class Sound : ILoad
		{
			[XmlIgnore]
			Stream _Stream;
			public string _Path;
			public Sound()
			{
				if (!Database._ILoads.Contains(this))
					Database._ILoads.Add(this);
			}
			public void Load()
			{
				WebClient _WebClient = new WebClient();
				_WebClient.OpenReadAsync(new Uri(_Path, UriKind.Relative));
				_WebClient.OpenReadCompleted += new OpenReadCompletedEventHandler(WebClientOpenReadCompleted);
			}

			void WebClientOpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
			{
				_Stream = e.Result;
			}
			public void Play(Vector2 v1, Vector2 v2)
			{
				if (_Stream != null)
				{
					float dist = Vector2.Distance(v1, v2);
					float a = 1000;
					float dist1 = (a - dist) / a;
					if (dist1 > 0)
					{
						MediaElement _MediaElement = new MediaElement();
						_MediaElement.Volume = dist1;
						_MediaElement.Source = new Uri(_Path, UriKind.Relative);
					}

				}
			}


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
					BitmapImage _BitmapImage = new BitmapImage(new Uri(_path,UriKind.Relative));                                        

					_BitmapImages.Add(_BitmapImage);
				}
			}			
		}
	}
}
