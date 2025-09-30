using MMTTLVParser.PacketDefinitions;
using MMTTLVParser.Parser.ParserFunctions;
using System.Reflection;

namespace MMTTLVParser.Parser;

public class ParserFunctionController
{
    public delegate ParserResultModel ParserFunctionDelegate(ReadOnlySpan<byte> data);
    private Dictionary<Type, ParserFunctionDelegate> _parserFunctionDict = [];

    public ParserFunctionController()
    {
        var methods = typeof(ParserFunctionCollection).GetMethods(BindingFlags.Public | BindingFlags.Static);
        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<ParserFunctionAttribute>();

            if (attr is null)
                continue;

            if (method.ReturnType != typeof(ParserResultModel))
                continue;

            var parameters = method.GetParameters();
            if (parameters.Length != 1 || parameters[0].ParameterType != typeof(ReadOnlySpan<byte>))
                continue;

            if (attr.PacketType.IsSubclassOf(typeof(Packet)) == false)
                continue;

            var del = (ParserFunctionDelegate)Delegate.CreateDelegate(typeof(ParserFunctionDelegate), method);
            _parserFunctionDict.Add(attr.PacketType, del);
        }
    }

    public ParserFunctionDelegate? GetParserFunctionDelegate(Type packetType)
    {
        return _parserFunctionDict.TryGetValue(packetType, out var del) ? del : null;
    }
}
