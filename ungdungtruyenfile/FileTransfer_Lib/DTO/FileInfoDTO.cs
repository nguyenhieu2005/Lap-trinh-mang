namespace FileTransfer_Lib.DTO
{
    public class FileInfoDTO
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public long Offset { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public bool Encrypted { get; set; }
    }
}
