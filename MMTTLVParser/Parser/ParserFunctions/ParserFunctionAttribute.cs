namespace MMTTLVParser.Parser.ParserFunctions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ParserFunctionAttribute(Type Type) : Attribute
{
    public Type PacketType => Type;
}