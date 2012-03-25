using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ReplacementFileSystem
{
    /// <summary>
    /// MockFileSystem provides a complete virtual filesystem implementing the IFileSystem interface.
    /// </summary>
    public class MockFileSystem : IFileSystem
    {
        /// <summary>
        /// Info provides a List representing all MockFileSystem objects currently represented.  To add a file or directory, add a MockFileSystemInfo object to this list.
        /// </summary>
        public List<MockFileSystemInfo> Info = new List<MockFileSystemInfo>();
        string userDataPath;

        public virtual string UserDataPath
        {
            get
            {
                if (userDataPath == null)
                {
                    userDataPath = @"C:\" + Guid.NewGuid().ToString();
                    EnsurePath(userDataPath);
                }

                return userDataPath;
            }
        }

        public virtual void AppendAllText(string path, string text)
        {
            using (Stream stream = OpenFile(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                stream.Position = stream.Length;

                StreamWriter writer = new StreamWriter(stream);

                writer.Write(text);
                writer.Flush();
            }
        }

        public virtual void CombineAttributes(string path, FileAttributes attributes)
        {
            MockFileSystemInfo info = Locate(path);
            info.Attributes |= attributes;
        }

        public virtual string CombinePath(params string[] pathElements)
        {
            // Just delegate to Path.Combine, it doesn't actually access the file system
            string result = "";

            foreach (string pathElement in pathElements)
                result = Path.Combine(result, pathElement);

            return result;
        }

        public virtual void CopyFile(string sourcePath, string targetPath)
        {
            CopyFile(sourcePath, targetPath, false);
        }

        public virtual void CopyFile(string sourcePath, string targetPath, bool overwrite)
        {
            if (!overwrite && Locate(targetPath) != null)
                throw new IOException("The target file already exists.");

            MockFileSystemInfo sourceInfo = Locate(sourcePath);
            MockFileSystemInfo originalTarget = Locate(targetPath);

            if (originalTarget != null)
                Info.Remove(originalTarget);

            MockFileSystemInfo targetInfo = new MockFileSystemInfo(sourceInfo.ItemType, targetPath, Path.GetFileName(targetPath), sourceInfo.Attributes, sourceInfo.LastWriteTime, "");

            targetInfo.FileData = sourceInfo.FileData;

            Info.Add(targetInfo);
        }

        public virtual void CreateDirectory(string path)
        {
            MockFileSystemInfo info = Locate(path);
            if (info != null)
                throw new IOException("The target directory already exists.");
            Info.Add(new MockFileSystemInfo(ItemType.Folder, path));
        }

        public virtual void DeleteDirectory(string path, bool force)
        {
            MockFileSystemInfo info = Locate(path);

            if (info == null)
                return;

            if (info.ItemType != ItemType.Folder)
                throw new DirectoryNotFoundException();

            string testPath = path.ToLowerInvariant();
            if (!testPath.EndsWith("\\"))
                testPath += "\\";

            List<MockFileSystemInfo> children = Info.FindAll(delegate(MockFileSystemInfo mockInfo)
            {
                return mockInfo.Path.ToLowerInvariant().StartsWith(testPath);
            });

            if (children.Count > 0)
            {
                if (!force)
                    throw new IOException("Directory " + path + " not empty, and force is false");

                Info.RemoveAll(delegate(MockFileSystemInfo mockInfo)
                {
                    return children.Contains(mockInfo);
                });
            }

            Info.Remove(info);
        }

        public virtual void DeleteFile(string path)
        {
            MockFileSystemInfo info = Locate(path);

            if (info == null)
                return;

            if (info.ItemType != ItemType.File)
                throw new FileNotFoundException();

            Info.Remove(info);
        }

        public virtual bool DirectoryExists(string path)
        {
            MockFileSystemInfo info = Locate(path);
            return info != null && info.ItemType == ItemType.Folder;
        }

        public virtual void EnsurePath(string path)
        {
            MockFileSystemInfo info = Locate(path);
            if (info == null)
            {
                Info.Add(new MockFileSystemInfo(ItemType.Folder, path));
            }
        }

        public virtual bool FileExists(string path)
        {
            MockFileSystemInfo info = Locate(path);
            return info != null && info.ItemType == ItemType.File;
        }

        public virtual FileAttributes GetAttributes(string path)
        {
            MockFileSystemInfo info = Locate(path);
            if (info == null)
                throw new FileNotFoundException();

            return info.Attributes;
        }

        public virtual string[] GetDirectories(string path)
        {
            if (Locate(path) == null)
                throw new DirectoryNotFoundException();

            List<string> dirs = new List<string>();

            foreach (MockFileSystemInfo info in Info)
            {
                if (info.ItemType == ItemType.Folder)
                {
                    string infoDirectoryName = Path.GetDirectoryName(info.Path);
                    if (string.Compare(path, infoDirectoryName, true) == 0)
                        dirs.Add(info.Path);
                }
            }

            return dirs.ToArray();
        }

        public virtual string GetDirectoryName(string localPath)
        {
            if (string.IsNullOrEmpty(localPath))
                return null;

            char ending = localPath[localPath.Length - 1];

            if (ending == Path.DirectorySeparatorChar || ending == Path.AltDirectorySeparatorChar)
                localPath = localPath.Substring(0, localPath.Length - 1);

            return Path.GetDirectoryName(localPath);
        }

        public virtual byte[] GetFileHash(string path)
        {
            MockFileSystemInfo info = Locate(path);
            if (info == null)
                throw new FileNotFoundException();

            using (MemoryStream stream = new MemoryStream(info.FileData))
                return EncryptionUtil.GetHash(stream);
        }

        public virtual byte[] GetFileHash(byte[] contents)
        {
            using (MemoryStream stream = new MemoryStream(contents))
                return EncryptionUtil.GetHash(stream);
        }

        public virtual string GetFileHashAsString(string path)
        {
            return Convert.ToBase64String(GetFileHash(path));
        }

        public virtual string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public virtual string[] GetFiles(string path)
        {
            return this.GetFiles(path, "*.*");
        }

        public virtual string[] GetFiles(string path, string searchPattern)
        {
            if (Locate(path) == null)
                throw new DirectoryNotFoundException();

            List<string> files = new List<string>();

            foreach (MockFileSystemInfo info in Info)
            {
                if (info.ItemType == ItemType.File)
                {
                    string infoDirectoryName = Path.GetDirectoryName(info.Path);
                    if (string.Compare(path, infoDirectoryName, true) == 0)
                    {
                        if (searchPattern == "*.*")
                        {
                            files.Add(info.Path);
                        }
                        else
                        {
                            string fileName = Path.GetFileName(info.Path);
                            if (fileName.Contains(searchPattern))
                                files.Add(info.Path);
                        }
                    }
                }
            }

            return files.ToArray();
        }

        public virtual long GetFileSize(string path)
        {
            MockFileSystemInfo info = Locate(path);
            if (info == null)
                throw new FileNotFoundException();

            return info.FileData.Length;
        }

        public virtual string GetFullPath(string path)
        {
            // Just delegate to Path.GetFullPath, it will still work if the file doesn't exist..
            return Path.GetFullPath(path);
        }

        public virtual DateTime GetLastWriteTime(string path)
        {
            MockFileSystemInfo info = Locate(path);
            if (info == null)
                throw new FileNotFoundException();

            return info.LastWriteTime;
        }

        public virtual string GetName(string path)
        {
            // Just delegate to Path.GetFileName, it will still work if the file doesn't exist.  It also works on directories to get the directory name.
            return Path.GetFileName(path);
        }

        MockFileSystemInfo Locate(string path)
        {
            return Info.Find(delegate(MockFileSystemInfo obj)
            {
                return string.Compare(path, obj.Path, true) == 0;
            });
        }

        public virtual Stream OpenFile(string path, FileMode mode, FileAccess access, FileShare share)
        {
            bool mustExist = mode == FileMode.Truncate || mode == FileMode.Open;
            bool cantExist = mode == FileMode.CreateNew;

            MockFileSystemInfo info = Locate(path);
            if (mustExist && (info == null || info.ItemType != ItemType.File))
                throw new FileNotFoundException();

            if (info != null && cantExist)
                throw new IOException("The file already exists.");

            if (info == null)
            {
                info = new MockFileSystemInfo(ItemType.File, path, DateTime.Now, "");

                Info.Add(info);
            }

            bool writable = access == FileAccess.ReadWrite || access == FileAccess.Write;

            if (writable)
                info.LastWriteTime = DateTime.Now;

            return new MockFileSystemInfoStream(info, writable);
        }

        public virtual byte[] ReadAllBytes(string path)
        {
            byte[] contents = new byte[GetFileSize(path)];

            using (Stream inputStream = OpenFile(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                inputStream.Read(contents, 0, contents.Length);

            return contents;
        }

        public virtual string[] ReadAllLines(string path)
        {
            return ReadAllText(path).Split(new string[] { "\r\n" }, StringSplitOptions.None);
        }

        public virtual string ReadAllText(string path)
        {
            using (Stream stream = OpenFile(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                StreamReader reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }

        public virtual void RemoveAttributes(string path, FileAttributes attributes)
        {
            MockFileSystemInfo info = Locate(path);
            if (info == null)
                throw new FileNotFoundException();

            info.Attributes &= ~attributes;
        }

        public virtual void SetAttributes(string path, FileAttributes attributes)
        {
            MockFileSystemInfo info = Locate(path);
            if (info == null)
                throw new FileNotFoundException();

            info.Attributes = attributes;
        }

        public virtual void SetLastWriteTime(string path, DateTime value)
        {
            MockFileSystemInfo info = Locate(path);
            if (info == null)
                throw new FileNotFoundException();

            info.LastWriteTime = value;
        }

        public virtual void WriteAllBytes(string path, byte[] contents)
        {
            using (Stream stream = OpenFile(path, FileMode.Create, FileAccess.Write, FileShare.None))
                stream.Write(contents, 0, contents.Length);
        }

        public virtual void WriteAllLines(string path, string[] contents)
        {
            WriteAllText(path, string.Join("\r\n", contents));
        }

        public virtual void WriteAllText(string path, string text)
        {
            using (Stream stream = OpenFile(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                StreamWriter writer = new StreamWriter(stream);

                writer.Write(text);
                writer.Flush();
            }
        }


        /// <summary>
        /// Adds a mock file to the MockFileSystem object.  Will create the full directory structure by default.
        /// </summary>
        /// <param name="file">The file.</param>
        public virtual void AddMockFile(MockFileSystemInfo file, Boolean createDirectoryTree = true)
        {
            var fullpath = new FileInfo(file.Path).DirectoryName;
            
            //Recurse a bit to add directories if required.
            if (createDirectoryTree)
                AddMockDirectoryStructure(fullpath);

            Info.Add(file);
        }

        /// <summary>
        /// Recursively adds the mock directory structure for each directory represented by the path.
        /// </summary>
        /// <param name="directory">The directory.</param>
        public virtual void AddMockDirectoryStructure(string directory)
        {
            var parent = new DirectoryInfo(directory).Parent;
            if (parent != null)
            {
                AddMockDirectoryStructure(parent.FullName);
            }
            if (!DirectoryExists(directory))
            {
                Info.Add(new MockFileSystemInfo(ItemType.Folder, directory)
                {
                    Attributes = FileAttributes.Directory | FileAttributes.Normal
                });
            }
        }
    }
}