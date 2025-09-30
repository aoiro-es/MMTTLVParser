using MMTTLVParser.PacketDefinitions.MMT.Table;

namespace MMTTLVParser.PacketDefinitions.MMT;

public abstract record Message(ushort MessageId, byte Version, uint Length) : Packet;

public record PAMessage(ushort MessageId, byte Version, uint Length, PAMessageTable[] TableInfos) : Message(MessageId, Version, Length);

public record M2SectionMessage(ushort MessageId, byte Version, uint Length, M2SectionMessageTable? Table, byte[] CRC32) : Message(MessageId, Version, Length);

public record CAMessage(ushort MessageId, byte Version, uint Length, CATable? Table) : Message(MessageId, Version, Length);

public record M2ShortSectionMessage(ushort MessageId, byte Version, uint Length, byte TableId, bool SectionSyntaxIndicator, ushort SectionLength, M2ShortSectionMessageTable? Table) : Message(MessageId, Version, Length);

public record DataTransmissionMessage(ushort MessageId, byte Version, uint Length, DataTransmissionMessageTable? Table, byte[] CRC32) : Message(MessageId, Version, Length);