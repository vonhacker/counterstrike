using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSL.Common;
using System.IO;
using System.Xml.Serialization;

namespace CSL.LevelEditor
{
    /// <summary>
    /// Data access layer
    /// </summary>
    public class Dal
    {
        private Dal() { }

        private static XmlSerializer _xmlSerializer = new XmlSerializer(typeof(MapDatabase));
        public static MapDatabase GetMapDatabase(String filePath4MapDescriptor)
        {
            MapDatabase mapDatabase = new MapDatabase();
            try
            {
                if (!File.Exists(filePath4MapDescriptor))
                {
                    InfoMessageBox.Show("File not exists " + filePath4MapDescriptor);
                    return mapDatabase;
                }
                byte[] buffer = File.ReadAllBytes(filePath4MapDescriptor);
                MemoryStream memoryStream = new MemoryStream(buffer);
                mapDatabase = (MapDatabase)_xmlSerializer.Deserialize(memoryStream);

                return mapDatabase;
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
                return mapDatabase;
            }
        }

        public static void SaveMapDatabase(String filePath4MapDescriptor, MapDatabase mapDatabase)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                _xmlSerializer.Serialize(memoryStream, mapDatabase);

                byte[] buffer = memoryStream.ToArray();
                File.WriteAllBytes(filePath4MapDescriptor, buffer);
            }
            catch (Exception ex)
            {
                ErrorMessageBox.Show(ex);
            }
        }
    }
}
