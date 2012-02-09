using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ReplacementFileSystem
{
    public class MockFileSystemInfo
    {
        public FileAttributes Attributes;
        public byte[] FileData;
        public string FileName;
        public ItemType ItemType;
        public DateTime LastWriteTime;
        public string Path;

        public MockFileSystemInfo() { }

        public MockFileSystemInfo(ItemType itemType, string path) : this(itemType, path, DateTime.Now, string.Empty) { }

        public MockFileSystemInfo(ItemType itemType, string path, DateTime lastWriteTime, string fileData)
            : this(itemType, path, System.IO.Path.GetFileName(path), FileAttributes.Normal, lastWriteTime, fileData)
        {
            if (ItemType == ItemType.Folder)
                Attributes |= FileAttributes.Directory;
        }

        public MockFileSystemInfo(ItemType itemType, string path, string filename, FileAttributes attributes, DateTime lastWriteTime, string fileData)
        {
            ItemType = itemType;
            Path = path;
            FileName = filename;
            Attributes = attributes;
            LastWriteTime = lastWriteTime;
            //TODO This is probably not good encoding.  Fix.
            FileData = Encoding.ASCII.GetBytes(fileData);
        }

        public static MockFileSystemInfo CreateFileObject(string path, string content = null)
        {
            var file = new MockFileSystemInfo()
            {
                Path = path,
                ItemType = ReplacementFileSystem.ItemType.File
            };
            if (content != null)
                file.FileData = Encoding.ASCII.GetBytes(content);

            return file;
        }

        public static MockFileSystemInfo CreateDirectoryObject(string path)
        {
            return new MockFileSystemInfo()
            {
                Path = path,
                ItemType = ItemType.Folder,
                Attributes = FileAttributes.Directory
            };
        }
    }
}
