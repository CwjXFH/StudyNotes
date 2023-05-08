using System.Buffers;
using System.Reflection;
using System.Text;
using MessagePack;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;

namespace StockFundamentals.Service.Helper;

public class ObjectSizeHelper
{
    private static readonly IDictionary<string, int> _typeSize;
    private static ILoggerFactory _loggerFactory = new SerilogLoggerFactory();
    private static ILogger _logger = _loggerFactory.CreateLogger<ObjectSizeHelper>();

    public static int ObjCount = 0;
    public static long ObjSize = 0;
    public static int LargeObjCount = 0;
    public static long LargeObjSize = 0;
    public static int LargeLargeObjCount = 0;
    public static long LargeLargeObjSize = 0;

    public static long ObjTotalSize = 0;
    public static int ObjTotalCount = 0;
    public static int ObjMaxSize = 0;
    public static int ObjMinSize = 0;

    static ObjectSizeHelper()
    {
        unsafe
        {
            _typeSize = new Dictionary<string, int>
            {
                [typeof(long).FullName] = sizeof(long),
                [typeof(int).FullName] = sizeof(int),
                [typeof(ulong).FullName] = sizeof(ulong),
                [typeof(uint).FullName] = sizeof(uint),
                [typeof(byte).FullName] = sizeof(byte),
                [typeof(double).FullName] = sizeof(double),
                [typeof(decimal).FullName] = sizeof(decimal),
                [typeof(DateTime).FullName] = sizeof(DateTime),
                [typeof(bool).FullName] = sizeof(DateTime),
                [typeof(long?).FullName] = sizeof(long),
                [typeof(int?).FullName] = sizeof(int),
                [typeof(ulong?).FullName] = sizeof(ulong),
                [typeof(uint?).FullName] = sizeof(uint),
                [typeof(byte?).FullName] = sizeof(byte),
                [typeof(double?).FullName] = sizeof(double),
                [typeof(decimal?).FullName] = sizeof(decimal),
                [typeof(DateTime?).FullName] = sizeof(DateTime),
                [typeof(bool?).FullName] = sizeof(DateTime),
            };
        }
    }

    // 可以获取对象在内存中大小及序列化之后的大小
    public static int GetBytes(object obj)
    {
        var objSize = 0;

        // 序列化后，要考虑字段名等空间占用
        // 使用MessagePack序列化之后的大小
        var msg = MessagePackSerializer.Serialize(obj);
        objSize = msg.Length;

        if (objSize > 1024 * 1024)
        {
            _logger.LogError("超大对象出现，大小为：{size}", objSize);
            LargeLargeObjCount += 1;
            LargeLargeObjSize += objSize;
        }
        else if (objSize > 1024 * 512)
        {
            _logger.LogWarning("大对象出现，大小为：{size}", objSize);
            LargeObjCount += 1;
            LargeObjSize += objSize;
        }
        else if (objSize > 1024 * 84)
        {
            _logger.LogInformation("较大对象出现，大小为：{size}", objSize);
            ObjCount += 1;
            ObjSize += objSize;
        }

        ObjTotalSize += objSize;
        ObjTotalCount += 1;
        if (objSize < ObjMinSize || ObjMinSize == 0)
        {
            ObjMinSize = objSize;
        }

        if (objSize > ObjMaxSize)
        {
            ObjMaxSize = objSize;
        }

        return objSize;
    }


    // 此处为考虑object header, method pointer等大小
    private static int GetBytesCore(object obj)
    {
        var type = obj.GetType();
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var objSize = 0;
        foreach (var pro in properties)
        {
            object? pval = null; //pro.GetValue(obj);
            try
            {
                pval = pro.GetValue(obj);
            }
            catch (Exception ex)
            {
            }

            if (pval is null)
            {
                continue;
            }

            if (pval is string str)
            {
                objSize += Encoding.UTF8.GetByteCount(str);
                continue;
            }
            else if (pval is Array arr)
            {
                objSize += arr.Length * 8;
                continue;
            }

            var pType = pval.GetType().FullName!;
            if (_typeSize.TryGetValue(pType, out int size))
            {
                objSize += size;
                continue;
            }
            else
            {
                throw new ArgumentException(pType);
            }
        }

        return objSize;
    }
}
