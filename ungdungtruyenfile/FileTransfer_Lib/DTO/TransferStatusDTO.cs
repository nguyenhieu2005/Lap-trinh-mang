namespace FileTransfer_Lib.DTO
{
    public class TransferStatusDTO
    {
        public string FileName { get; set; }
        public long BytesTransferred { get; set; }
        public long TotalBytes { get; set; }
    }
}
