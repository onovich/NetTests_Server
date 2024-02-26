using System;
using MortiseFrame.LitIO;

public struct LoginReqMessage : IMessage<LoginReqMessage> {

    public int id;
    public string userToken;

    public void WriteTo(byte[] dst, ref int offset) {
        ByteWritter.Write<int>(dst, id, ref offset);
        ByteWritter.WriteString(dst, userToken, ref offset);
    }

    public void FromBytes(byte[] src, ref int offset) {
        id = ByteReader.Read<int>(src, ref offset);
        userToken = ByteReader.ReadString(src, ref offset);
    }

    public int GetEvaluatedSize(out bool isCertain) {
        int count = 8;
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