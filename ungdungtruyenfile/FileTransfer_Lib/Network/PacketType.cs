namespace FileTransfer_Lib.Network
{
    public enum PacketType
    {
        LoginRequest,
        LoginResponse,
        RegisterRequest,
        RegisterResponse,

        FileMeta,
        FileChunk,
        FileChunkAck,
        FileComplete,

        ResumeRequest,
        ResumeResponse,

        ProgressUpdate,
        Error,
        Heartbeat
    }
}
