using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Xml.Serialization;
using doru;


namespace Cleaner2
{
	public class Database
	{
		public string _directorya;
		public string _directoryb;
		public string _pattern;
        public string _pattern2;
	}
    public class Program 
    {
		public Database _Database;
        public string _directorya { get { return _Database._directorya; } }
        public string _directoryb { get { return _Database._directoryb; } }
        public string _pattern { get { return _Database._pattern; } }
        public string _pattern2 { get { return _Database._pattern2; } }
        public static Chilkat.Zip _Zip;
        static void Main(string[] args)
        {
            Spammer3.Setup();
            _Zip = new Chilkat.Zip();
            if (!_Zip.UnlockComponent("30-day trial")) throw new Exception("chilcat");
            Program _Program = new Program();                        
        }

        bool gz;
        public Program()
        {
            LoadDb();
            if (Path.GetExtension(_directoryb) == ".zip")
            {
                gz = true;
                if (File.Exists(_directoryb)) File.Delete(_directoryb);
                _Zip.NewZip(_directoryb);
            }
                        
			List<string> _directories = new List<string>();
            GetDirectories(_directories, new string[] { _directorya });
            if (!gz)
                CreateDirectories(_directories);
            else            
                foreach (string dir in _directories)                
                    _Zip.AppendNewDir(dir.TrimStart(_directorya));                            

			List<string> _files = GetFiles(_directories);

            int i=0;
            foreach (string _File in _files)
            {
                if (Regex.IsMatch(_File.TrimStart(_directorya), _pattern, RegexOptions.IgnoreCase) && (_pattern2 == null || _pattern2.Length == 0 || !Regex.IsMatch(_File.TrimStart(_directorya), _pattern2, RegexOptions.IgnoreCase)))
                    try
                    {
                        if (!gz)
                            File.Copy(_File, _directoryb + "/" + _File.TrimStart(_directorya));
                        else
                            _Zip.AppendData(_File.TrimStart(_directorya),File.ReadAllBytes(_File));
                        Trace.WriteLine(_File);
                        i++;
                    }
                    catch (IOException e) { Trace.WriteLine("error:"+_File+" "+e.Message); }
                    catch (UnauthorizedAccessException e) { Trace.WriteLine("error:" + _File+" "+e.Message); }
            }
            if (gz)
                _Zip.WriteZipAndClose();
            Trace.WriteLine("Moved:" + i);
        }

        private void LoadDb()
        {
            XmlSerializer _XmlSerializer = new XmlSerializer(typeof(Database));
            using (FileStream _FileStream = new FileStream("db.xml", FileMode.Open, FileAccess.Read))
                _Database = (Database)_XmlSerializer.Deserialize(_FileStream);
        }

        private void CreateDirectories(List<string> _directories)
        {
            if (!Directory.Exists(_directoryb)) Directory.CreateDirectory(_directoryb);
            foreach (string _Direcotry in _directories)
                try
                {
                    if (!Directory.Exists(_directoryb + "/" + _Direcotry)) Directory.CreateDirectory(_directoryb + "/" + _Direcotry);
                }
                catch (PathTooLongException) { }
        }
		private static List<string> GetFiles(List<string> _directories)
		{
			List<string> _list = new List<string>();
			foreach (string _string in _directories)
			{
				try
				{
					string[] _files = Directory.GetFiles(_string);
					foreach (string file in _files)
					{
						_list.Add(file);
					}
				}
				catch { }				
			}
			return _list;
		}

		private static void GetDirectories(List<string> _strings2, string[] _strings)
		{
			foreach (string _string in _strings)
			{
				_strings2.Add(_string);
				try
				{
					string[] _directories = Directory.GetDirectories(_string);
					GetDirectories(_strings2, _directories);
				}
				catch { }								
			}
		}
    }
}
