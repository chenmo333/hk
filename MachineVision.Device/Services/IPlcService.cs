using System;
using S7.Net;

namespace MachineVision.Device.Services;

public interface IPlcService
{
    public bool InitPLC(CpuType cpu, string ip, short rack, short slot);

    int    ReadDbInt(int     db, int startByteAddress);
    float  ReadDbReal(int    db, int startByteAddress);
    bool   ReadDbBool(int    db, int startByteAddress, byte   bitAdr);
    void   WriteDbInt(int    db, int startByteAddress, int    value);
    void   WriteDbBool(int   db, int startByteAddress, byte   bitAdr, bool value);
    string ReadDbString(int  db, int startByteAddress, int    maxLength);
    void   WriteDbString(int db, int startByteAddress, string value, int maxLength);
}