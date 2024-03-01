using System;
using MortiseFrame.LitIO;

public struct LoginResMessage : IMessage<LoginResMessage> {

    public int id;
    public sbyte status; // 1 为成功, -1 为失败
    public string userToken;

    public void WriteTo(byte[] dst, ref int offset) {
        ByteWriter.Write<int>(dst, id, ref offset);
        ByteWriter.Write<sbyte>(dst, status, ref offset);
        ByteWriter.WriteUTF8String(dst, userToken, ref offset);
    }

    public void FromBytes(byte[] src, ref int offset) {
        id = ByteReader.Read<int>(src, ref offset);
        status = ByteReader.Read<sbyte>(src, ref offset);
        userToken = ByteReader.ReadUTF8String(src, ref offset);
    }

    public int GetEvaluatedSize(out bool isCertain) {
        int count = 9;
        isCertain = false;

        if (userToken != null) {
            count += userToken.Length * 4;
        }

        return count;
    }

    public byte[] ToBytes() {
        int count = GetEvaluatedSize(out bool isCertain);
        int offset = 0;
        byte[] src = new byte[count];
        WriteTo(src, ref offset);
        if (isCertain) {
            return src;
        } else {
            byte[] dst = new byte[offset];
            Buffer.BlockCopy(src, 0, dst, 0, offset);
            return dst;
        }
    }

}