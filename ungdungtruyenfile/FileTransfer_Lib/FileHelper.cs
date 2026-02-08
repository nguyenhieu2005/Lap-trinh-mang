using System.IO;

namespace FileTransfer_Lib
{
    public static class FileHelper
    {
        public static FileStream OpenRead(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public static FileStream OpenWrite(string path, long offset)
        {
            var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            fs.Seek(offset, SeekOrigin.Begin);
            return fs;
        }
    }
}
