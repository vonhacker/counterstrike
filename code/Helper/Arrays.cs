using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Linq;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Reflection;
using System.Collections;
using System.Xml.Schema;
using System.Collections.Specialized;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using ICSharpCode.SharpZipLib.Zip;
using System.Windows.Controls;
using System.ComponentModel;





namespace doru
{
	public class BindableList<T> : ObservableCollection<T>
	{
        public BindableList()
        {

        }
        
		public List<ICVS> _ArrayList = new List<ICVS>();
		public interface ICVS
		{
			void Add(object o);
			void Remove(object o);
		}
		//[DebuggerStepThrough]
		public class CVS<T2> : ICVS
		{
			public delegate T2 Converter(T t);
			public IList<T2> _list;
			public void Add(object o)
			{
				T2 t2 = _Converter((T)o);
				if (!_list.Contains(t2))
					_list.Add(t2);
			}
			public void Remove(object o)
			{
				T2 t2 = _Converter((T)o);
				if (_list.Contains(t2))
					_list.Remove(t2);
			}
			public CVS()
			{
				if (_Converter == null) _Converter = DefaultConverter;
			}
			public Converter _Converter;
			public T2 DefaultConverter(T t)
			{
				object o = (object)t;
				return (T2)o;
			}
		}
		public void BindTo<T2>(IList<T2> list)
		{
            //if (_ArrayList.Any(a => a == list)) Debugger.Break();
			CVS<T2> _CVS = new CVS<T2> { _list = list };
			_ArrayList.Add(_CVS);
		}

		public void BindTo<T2>(IList<T2> list, CVS<T2>.Converter cv)
		{
            //if (_ArrayList.Any(a => a == list)) Debugger.Break();
			CVS<T2> _CVS = new CVS<T2> { _Converter = cv, _list = list };
			_ArrayList.Add(_CVS);
		}
		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (T t in e.NewItems)
						foreach (ICVS o in _ArrayList)
							o.Add(t);
					break;
				case NotifyCollectionChangedAction.Remove:
					foreach (T t in e.OldItems)
						foreach (ICVS o in _ArrayList)
							o.Remove(t);
					break;
			}
			base.OnCollectionChanged(e);
		}
	}
	[Obsolete("Use clinq")]
	public class ObservableArray<T> : IEnumerable<T>, INotifyCollectionChanged
	{
		ObservableCollection<T> _List = new ObservableCollection<T>();
		T[] a;
		public ObservableArray(int count)
		{
			_List.CollectionChanged += new NotifyCollectionChangedEventHandler(List_CollectionChanged);
			a = new T[count];
		}


		void List_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (CollectionChanged != null) CollectionChanged(sender, e);
		}
		public T this[int i]
		{
			get { return a[i]; }
			set
			{
				T oldValue = a[i];
				a[i] = value;

				if (oldValue == null)
				{
					_List.Add(value);
				} else
				{
					_List.Remove(oldValue);
					if (value != null) _List.Add(value);
				}
			}
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public IEnumerator<T> GetEnumerator()
		{
			return _List.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _List.GetEnumerator();
		}
	}

}
