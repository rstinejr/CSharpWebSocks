using System;

namespace waltonstine.demo.csharp.websockets.upload
{
    public class UploadChunk
    {
        public const UInt64 CHUNK_SIZE = 10240; 

        public int UploadID { get; }
        public UInt64 ChunkNumber { get; }
        public byte[] ChunkData { get; }

        public UploadChunk(int uploadID, UInt64 chunkNumber, byte[] chunkData)
        {
            UploadID    = uploadID;
            ChunkNumber = chunkNumber;
            ChunkData   = chunkData;
        }
    }
}
