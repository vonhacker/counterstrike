using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using doru;
using System.Xml.Serialization;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Organizer
{
    public class Program
    {
        static void Main(string[] args)
        {
            Logging.Setup();
            new Program();
        }
        public class Database
        {
            public List<Task> _Tasks = new List<Task>() { new Task() };
        }
        public class Task
        {
            public string[] _SourcePath = new string[] { @"D:\Documents and Settings\doru\Desktop\", @"D:\Documents and Settings\doru\My Documents\" };
            public string _IgnoreFiles = @"\.!ut";
            public List<Dir> _Dirs = new List<Dir> {                                
                new Dir { Path = @"D:\Documents and Settings\doru\My Documents\AfterEffects",  FileTypes = @"\.aep"},                
                new Dir { Path = @"D:\Documents and Settings\doru\My Documents\code", FileTypes = @"\.cs|\.cpp"},
                new Dir { Path = @"D:\Documents and Settings\doru\My Documents\soft", FileTypes = @"\.exe|\.zip|\.rar|\.msi|\.iso|\.r01|\.7z|\.dmg"},
                new Dir { Path = @"D:\Documents and Settings\doru\My Documents\yiff", FileTypes = @"\.jpg|\.png|\.gif|\.swf" , match = .8f},
                new Dir { Path = @"D:\Documents and Settings\doru\My Documents\video", FileTypes = @"\.avi|\.flv|\.mov"},                                                
                new Dir { Path = @"D:\Documents and Settings\doru\My Documents\music", FileTypes = @"\.mp3" , match = .4f},                 
                new Dir { Path = @"D:\Documents and Settings\doru\My Documents\sites",FileTypes = @"\.php|\.fla|\.as|\.html"},
            };
        }
        public class Dir
        {
            public float match = 0;
            public string Path;
            public string FileTypes;
            public override string ToString()
            {
                return FileTypes;
            }
        }
        public static Database _Database = new Database();
        public IEnumerable<Dir> td { get { return _Database._Tasks.SelectMany(tt => tt._Dirs); } }
        public Program()
        {
            //XmlSerializer _XmlSerializer = Helper.CreateSchema("organizer", typeof(Database));
            //_XmlSerializer.DeserealizeOrCreate("db.xml", _Database);

            foreach (Dir _Dir in td)
                if (!Directory.Exists(_Dir.Path)) Directory.CreateDirectory(_Dir.Path);
            "Movig dirs ".Trace();
            foreach (Task task in _Database._Tasks)
            {
                foreach (string _SourcePath in task._SourcePath)
                {
                    //List<string> dirs = new List<string> { @"D:\Documents and Settings\doru\My Documents\Visual Studio 2008" }; o
                    List<string> dirs = Directory.GetDirectories(_SourcePath).ToList();

                    foreach (Dir _Dir in td)
                        dirs.Remove(_Dir.Path);

                    HandleDirs(task, _SourcePath, dirs);
                    MoveFiles(task,_SourcePath);
                }                
            }
        }

        private static void HandleDirs(Task task, string _SourcePath, List<string> dirs)
        {
            foreach (string dir in dirs)            
                try
                {
                    foreach (Dir _Dir in task._Dirs)
                    {


                        IEnumerable<string> files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);  

                        if (files.Count() == 0)
                        {
                            Directory.Delete(dir, true);
                            continue;
                        }

                        if (FilesIgnored(task, files))
                            continue;


                        IEnumerable<string> filesMatch = FilesMatch(_Dir, files);

                        if (files.Count() > 0 && (((float)filesMatch.Count()) / files.Count() > _Dir.match))
                        {                            
                            string nwdir = dir.Substring(_SourcePath.Length);
                            Directory.Move(dir, (_Dir.Path + @"\" + nwdir).Trace("moved dir"));
                            break;
                        }
                    }
                }
                catch (IOException e) { e.Message.Trace(); }           
        }

        private static IEnumerable<string> FilesMatch(Dir _Dir, IEnumerable<string> files)
        {
            return (from f in files
                    where Regex.IsMatch(f, _Dir.FileTypes)
                    select f);
        }

        private static bool FilesIgnored(Task task, IEnumerable<string> files)
        {
            return files.Any(a => Regex.IsMatch(a, task._IgnoreFiles));
        }

        

        private static void MoveFiles(Task task, string _SourcePath)
        {
            "moving files".Trace();
            foreach (Dir _Dir in task._Dirs)
            {

                var files = FilesMatch(_Dir, Directory.GetFiles(_SourcePath, "*", SearchOption.TopDirectoryOnly)).ToArray();
                foreach (string file in files)
                {
                    try
                    {
                        string nwfile = file.Substring(_SourcePath.Length);
                        while (true)
                        {
                            string nwpath = (_Dir.Path + @"\" + nwfile);
                            nwfile = Path.GetFileNameWithoutExtension(nwfile) + "1." + Path.GetExtension(nwfile);
                            if (File.Exists(nwpath)) continue;
                            File.Move(file, nwpath.Trace("file moved"));
                            break;
                        }                        
                    }
                    catch (IOException e) { e.Message.Trace(); }
                }
            }
        }
        
    }
}
