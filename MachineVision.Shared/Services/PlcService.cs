using System;
using System.Security.Principal;
using System.Text;
using S7.Net;

namespace MachineVision.Shared.Services;

public class PlcService : IPlcService
{
    private static Plc plc;

    public bool InitPLC(CpuType cpu, string ip, short rack, short slot)
    {
        try
        {
            plc = new Plc(cpu, ip, rack, slot); // 创建实例
            plc.Open();                         // 打开连接
            if (!plc.IsConnected)
            {
                Console.WriteLine("无法连接到 PLC，请检查网络或配置。");
                return false;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        return true;
    }

    public int ReadDbInt(int db, int startByteAddress)
    {
        try
        {
            var bytes = plc.ReadBytes(DataType.DataBlock, db, startByteAddress, 2);
            if (bytes.Length < 2)
            {
                Console.WriteLine("读取的数据不足 2 字节。");
                return int.MinValue;
            }

            // 解析为 Int16，并处理字节序（Big-Endian 转 Little-Endian）
            return (bytes[0] << 8) | bytes[1];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"读取 DB Int 失败: {ex.Message}");
            return int.MinValue;
        }
    }

    public float ReadDbReal(int db, int startByteAddress)
    {
        try
        {
            var bytes = plc.ReadBytes(DataType.DataBlock, db, startByteAddress, 4);
            if (bytes.Length < 4)
            {
                Console.WriteLine("读取的数据不足 4 字节。");
                return float.MinValue;
            }


            // 转换为浮点数
            return BitConverter.ToSingle(new[] { bytes[2], bytes[3], bytes[0], bytes[1] }, 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"读取 DB Real 失败: {ex.Message}");
            return float.MinValue;
        }
    }

    public bool ReadDbBool(int db, int startByteAddress, byte bitAdr)
    {
        try
        {
            var bytes = plc.ReadBytes(DataType.DataBlock, db, startByteAddress, 1);
            if (bytes.Length < 1)
            {
                Console.WriteLine("读取的数据不足 1 字节。");
                return false;
            }


            // 通过位偏移获取布尔值
            return (bytes[0] & (1 << bitAdr)) != 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"读取 DB Bool 失败: {ex.Message}");
            return false;
        }
    }

    public void WriteDbInt(int db, int startByteAddress, int value)
    {
        try
        {
            // 转换为字节数组（Big-Endian）
            byte[] bytes = { (byte)(value >> 8), (byte)(value & 0xFF) };
            plc.WriteBytes(DataType.DataBlock, db, startByteAddress, bytes);

            Console.WriteLine("写入 Int 成功！");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"写入 DB Int 失败: {ex.Message}");
        }
    }

    public void WriteDbBool(int db, int startByteAddress, byte bitAdr, bool value)
    {
        try
        {
            byte[] bytes = plc.ReadBytes(DataType.DataBlock, db, startByteAddress, 1);

            if (value)
                bytes[0] |= (byte)(1 << bitAdr);
            else
                bytes[0] &= (byte)~(1 << bitAdr);

            plc.WriteBytes(DataType.DataBlock, db, startByteAddress, bytes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"写入 DB Bool 失败: {ex.Message}");
            throw;
        }
    }

    public string ReadDbString(int db, int startByteAddress, int maxLength)
    {
        try
        {
            byte[] bytes = plc.ReadBytes(DataType.DataBlock, db, startByteAddress, maxLength + 2);

            // 第一个字节是最大长度，第二个字节是实际长度
            int actualLength = bytes[1];
            if (actualLength > maxLength)
                throw new Exception("实际字符串长度超出最大长度。");

            // 提取字符串内容
            return Encoding.ASCII.GetString(bytes, 2, actualLength);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"读取 DB String 失败: {ex.Message}");
            throw;
        }
    }

    public void WriteDbString(int db, int startByteAddress, string value, int maxLength)
    {
        try
        {
            if (value.Length > maxLength)
                throw new ArgumentException("字符串长度超过指定的最大长度。");

            byte[] bytes = new byte[maxLength + 2];

            // 第一个字节是最大长度，第二个字节是实际长度
            bytes[0] = (byte)maxLength;
            bytes[1] = (byte)value.Length;

            // 写入字符串内容
            Encoding.ASCII.GetBytes(value, 0, value.Length, bytes, 2);

            // 写入到 PLC
            plc.WriteBytes(DataType.DataBlock, db, startByteAddress, bytes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"写入 DB String 失败: {ex.Message}");
            throw;
        }
    }
}
