using MMTTLVParser.PacketDefinitions.MMT;
using MMTTLVParser.Parser.Reassemblers;

namespace MMTTLVParser.Parser;

public class ReassemblerController
{
    private Dictionary<Type, IReassembler> _reassemblerDict = [];

    public ReassemblerController()
    {
        _reassemblerDict.Add(typeof(MPUPayload), new MPUPayloadReassembler());
        _reassemblerDict.Add(typeof(ControlMessages), new ControlMessagesReassembler());
    }

    public IReassembler? GetReassembler(Type packetType)
    {
        if (packetType == typeof(MPUPayload) || packetType == typeof(ControlMessages))
        {
            return _reassemblerDict[packetType];
        }
        else
        {
            return null;
        }
    }
}
