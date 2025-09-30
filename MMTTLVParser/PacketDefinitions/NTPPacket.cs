namespace MMTTLVParser.PacketDefinitions;

public enum LeapIndicator : byte
{
    WithoutAlarm = 0,
    LastOneMinuteIs61Seconds = 2,
    LastOneMinuteIs59Seconds = 3,
    Alarm = 4
}

public enum WorkingMode : byte
{
    ObjectiveActiveMode = 1,
    ObjectivePassiveMode = 2,
    Client = 3,
    Server = 4,
    Broadcast = 5,
    MessageForNTPControl = 6
}

public record NTPPacket(LeapIndicator LeapIndicator, byte Version, WorkingMode Mode,
     byte Stratum, byte Poll, byte Precision, uint RootDelay, uint RootDispresion,
     uint ReferenceIdentification, ulong ReferenceTimestamp, ulong OriginTimestamp, ulong ReceiveTimestamp,
     ulong TransmitTimestamp) : Packet;
