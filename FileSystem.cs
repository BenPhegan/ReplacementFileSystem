using System;
using System.IO;

namespace ReplacementFileSystem
{
    public class FileSystem : IFileSystem
    {
        string userDataPath = null;

        public virtual string UserDataPath
        {
            get
            {
                if (userDataPath == null)
                {
                    userDataPath = CombinePath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "CodePlex Client");
                    EnsurePath(userDataPath);
                }
                return userDataPath;
            }
            set { userDataPath = value; }
        }

        public virtual void AppendAllText(string path, string contents)
        {
            File.AppendAllText(path, contents);
        }

        public virtual void CombineAttributes(string path, FileAttributes attributes)
        {
            FileAttributes existing = GetAttributes(path);

            existing |= attributes;

            SetAttributes(path, existing);
        }

        public virtual string CombinePath(params string[] pathElements)
        {
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
            File.Copy(sourcePath, targetPath, overwrite);
        }

        public virtual void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public virtual void DeleteDirectory(string path, bool force)
        {
            if (!Directory.Exists(path))
                return;

            if (force)
                RecursiveMakeNormal(path);

            Directory.Delete(path, force);
        }

        public virtual void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                SetAttributes(path, FileAttributes.Normal);
                File.Delete(path);
            }
        }

        public virtual bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public virtual void EnsurePath(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public virtual bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public virtual FileAttributes GetAttributes(string path)
        {
            return File.GetAttributes(path);
        }

        public virtual string[] GetDirectories(string path)
        {
            return Directory.GetDirectories(path);
        }

        public virtual string GetDirectoryName(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            char ending = path[path.Length - 1];

            if (ending == Path.DirectorySeparatorChar || ending == Path.AltDirectorySeparatorChar)
                path = path.Substring(0, path.Length - 1);

            return Path.GetDirectoryName(path);
        }

        public virtual byte[] GetFileHash(string path)
        {
            using (Stream stream = OpenFile(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                return EncryptionUtil.GetHash(stream);
        }

        public virtual byte[] GetFileHash(byte[] contents)
        {
            using (MemoryStream stream = new MemoryStream(contents, false))
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
            return Directory.GetFiles(path);
        }

        public virtual string[] GetFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern);
        }

        public virtual long GetFileSize(string path)
        {
            return new FileInfo(path).Length;
        }

        public virtual string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }

        public virtual DateTime GetLastWriteTime(string path)
        {
            return File.GetLastWriteTime(path);
        }

        public virtual string GetName(string path)
        {
            return Path.GetFileName(path);
        }

        public virtual Stream OpenFile(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return new FileStream(path, mode, access, share);
        }

        public virtual byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public virtual string[] ReadAllLines(string path)
        {
            return File.ReadAllLines(path);
        }

        public virtual string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        void RecursiveMakeNormal(string path)
        {
            foreach (string dir in GetDirectories(path))
            {
                RecursiveMakeNormal(dir);
                SetAttributes(dir, FileAttributes.Normal | FileAttributes.Directory);
            }

            foreach (string file in GetFiles(path))
                SetAttributes(file, FileAttributes.Normal);
        }

        public virtual void RemoveAttributes(string path, FileAttributes attributes)
        {
            FileAttributes existing = GetAttributes(path);

            existing &= ~attributes;

            SetAttributes(path, existing);
        }

        public virtual void SetAttributes(string path, FileAttributes attributes)
        {
            File.SetAttributes(path, attributes);
        }

        public virtual void WriteAllBytes(string path, byte[] contents)
        {
            File.WriteAllBytes(path, contents);
        }

        public virtual void WriteAllLines(string path, string[] contents)
        {
            File.WriteAllLines(path, contents);
        }

        public virtual void WriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }
    }
}