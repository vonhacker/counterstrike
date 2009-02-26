using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Xml.Serialization;
using doru;
using ICSharpCode.SharpZipLib.Zip;


namespace Cleaner2
{
	public class Database
	{
        public List<Task> _Tasks = new List<Task>();
	}
    public class Task
    {
        public string _directorya;
        public string _directoryb;
        public string _pattern;
        public string _pattern2;
    }
    public class Program 
    {
        static FastZip _Zip;
		public Database _Database;
        
        
        static void Main(string[] args)
        {
            Logging.Setup();
            _Zip = new FastZip();            
            Program _Program = new Program();                        
        }

        bool gz;
        public Program()
        {
            LoadDb();
            foreach(Task task in _Database._Tasks)
                Move(task._directorya,task._directoryb,task._pattern, task._pattern2);
        }

        private void Move(string _directorya, string _directoryb,string _pattern,string _pattern2)
        {
            
            if (Path.GetExtension(_directoryb) == ".zip")
            {
                gz = true;
                if (File.Exists(_directoryb)) File.Delete(_directoryb);
                _Zip.CreateZip(_directoryb); //.NewZip(_directoryb);
            }

            List<string> _directories = new List<string>();
            Helper.GetDirectories(_directories, new string[] { _directorya });
            if (!gz)
                Helper.CreateDirectories(_directories, _directoryb);
            else
                foreach (string dir in _directories)
                    _Zip.AppendNewDir(dir.TrimStart(_directorya));

            List<string> _files = GetFiles(_directories);

            int i = 0;
            foreach (string _File in _files)
            {
                if (Regex.IsMatch(_File.TrimStart(_directorya), _pattern, RegexOptions.IgnoreCase) && (_pattern2 == null || _pattern2.Length == 0 || !Regex.IsMatch(_File.TrimStart(_directorya), _pattern2, RegexOptions.IgnoreCase)))
                    try
                    {
                        if (!gz)
                            File.Copy(_File, _directoryb + "/" + _File.TrimStart(_directorya));
                        else
                            _Zip.AppendData(_File.TrimStart(_directorya), File.ReadAllBytes(_File));
                        Trace.WriteLine(_File);
                        i++;
                    }
                    catch (IOException e) { Trace.WriteLine("error:" + _File + " " + e.Message); }
                    catch (UnauthorizedAccessException e) { Trace.WriteLine("error:" + _File + " " + e.Message); }
            }
            if (gz)
                _Zip.WriteZipAndClose();
            Trace.WriteLine("Moved:" + i);
        }

        private void LoadDb()
        {
            XmlSerializer _XmlSerializer = Helper.CreateSchema("Cleaner2",typeof(Database));
            using (FileStream _FileStream = new FileStream("db.xml", FileMode.Open, FileAccess.Read))
                _Database = (Database)_XmlSerializer.Deserialize(_FileStream);
        }

        
		
    }
}
