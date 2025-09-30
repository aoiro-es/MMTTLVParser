using System.Buffers.Binary;

namespace MMTTLVParser.PacketDefinitions.MMT.Table;

public abstract record M2SectionMessageTable : Table;

public enum StateOfService : byte
{
    InNonOperation = 1,
    ItWillStartWithinSeveralSeconds = 2,
    OutOfOperation = 3,
    InOperation = 4
}

public record EntitlementControlMessage : M2SectionMessageTable
{
    public byte TableId { get; init; }
    public bool SectionSyntaxIndicator { get; init; }
    public ushort SectionLength { get; init; }
    public ushort TableIdExtension { get; init; }
    public byte VersionNumber { get; init; }
    public bool CurrentNextIndicator { get; init; }
    public byte SectionNumber { get; init; }
    public byte LastSectionNumber { get; init; }
    public byte[] ECMData { get; init; }

    public EntitlementControlMessage(ReadOnlySpan<byte> data)
    {
        TableId = data[0];
        SectionSyntaxIndicator = Convert.ToBoolean((data[1] & 0x80) >> 7);
        SectionLength = (ushort)(((data[1] & 0x0f) << 8) | data[2]);
        TableIdExtension = BinaryPrimitives.ReadUInt16BigEndian(data[3..5]);
        VersionNumber = (byte)((data[5] & 0x3e) >> 1);
        CurrentNextIndicator = Convert.ToBoolean(data[5] & 0x01);
        SectionNumber = data[6];
        LastSectionNumber = data[7];

        ECMData = data[8..].ToArray();
    }
}

public record EntitlementManagementMessage : M2SectionMessageTable
{
    public byte TableId { get; init; }
    public bool SectionSyntaxIndicator { get; init; }
    public ushort SectionLength { get; init; }
    public ushort TableIdExtension { get; init; }
    public byte VersionNumber { get; init; }
    public bool CurrentNextIndicator { get; init; }
    public byte SectionNumber { get; init; }
    public byte LastSectionNumber { get; init; }
    public byte[] EMMData { get; init; }
    public EntitlementManagementMessage(ReadOnlySpan<byte> data)
    {
        TableId = data[0];
        SectionSyntaxIndicator = Convert.ToBoolean((data[1] & 0x80) >> 7);
        SectionLength = (ushort)(((data[1] & 0x0f) << 8) | data[2]);
        TableIdExtension = BinaryPrimitives.ReadUInt16BigEndian(data[3..5]);
        VersionNumber = (byte)((data[5] & 0x3e) >> 1);
        CurrentNextIndicator = Convert.ToBoolean(data[5] & 0x01);
        SectionNumber = data[6];
        LastSectionNumber = data[7];

        EMMData = data[8..].ToArray();
    }
}

public record DownloadControlMessage : M2SectionMessageTable
{
    public byte TableId { get; init; }
    public bool SectionSyntaxIndicator { get; init; }
    public ushort SectionLength { get; init; }
    public ushort TableIdExtension { get; init; }
    public byte VersionNumber { get; init; }
    public bool CurrentNextIndicator { get; init; }
    public byte SectionNumber { get; init; }
    public byte LastSectionNumber { get; init; }
    public byte[] DCMData { get; init; }
    public DownloadControlMessage(ReadOnlySpan<byte> data)
    {
        TableId = data[0];
        SectionSyntaxIndicator = Convert.ToBoolean((data[1] & 0x80) >> 7);
        SectionLength = (ushort)(((data[1] & 0x0f) << 8) | data[2]);
        TableIdExtension = BinaryPrimitives.ReadUInt16BigEndian(data[3..5]);
        VersionNumber = (byte)((data[5] & 0x3e) >> 1);
        CurrentNextIndicator = Convert.ToBoolean(data[5] & 0x01);
        SectionNumber = data[6];
        LastSectionNumber = data[7];

        DCMData = data[8..].ToArray();
    }
}

public record DownloadManagementMessage : M2SectionMessageTable
{
    public byte TableId { get; init; }
    public bool SectionSyntaxIndicator { get; init; }
    public ushort SectionLength { get; init; }
    public ushort TableIdExtension { get; init; }
    public byte VersionNumber { get; init; }
    public bool CurrentNextIndicator { get; init; }
    public byte SectionNumber { get; init; }
    public byte LastSectionNumber { get; init; }
    public byte[] DMMData { get; init; }
    public DownloadManagementMessage(ReadOnlySpan<byte> data)
    {
        TableId = data[0];
        SectionSyntaxIndicator = Convert.ToBoolean((data[1] & 0x80) >> 7);
        SectionLength = (ushort)(((data[1] & 0x0f) << 8) | data[2]);
        TableIdExtension = BinaryPrimitives.ReadUInt16BigEndian(data[3..5]);
        VersionNumber = (byte)((data[5] & 0x3e) >> 1);
        CurrentNextIndicator = Convert.ToBoolean(data[5] & 0x01);
        SectionNumber = data[6];
        LastSectionNumber = data[7];

        DMMData = data[8..].ToArray();
    }
}

public record MHEventInformationTable : M2SectionMessageTable
{
    public record Event(ushort EventId, DateTime StartTime, TimeSpan Duration,
        StateOfService RunningStatus, bool FreeCAMode,
        Descriptor[] Descriptors);

    public byte TableId { get; init; }
    public bool SectionSyntaxIndicator { get; init; }
    public ushort SectionLength { get; init; }
    public ushort ServiceId { get; init; }
    public byte VersionNumber { get; init; }
    public bool CurrentNextIndicator { get; init; }
    public byte SectionNumber { get; init; }
    public byte LastSectionNumber { get; init; }
    public ushort TlvStreamId { get; init; }
    public ushort OriginalNetworkId { get; init; }
    public byte SegmentLastSectionNumber { get; init; }
    public byte LastTableId { get; init; }
    public Event[] Events { get; init; }

    public MHEventInformationTable(ReadOnlySpan<byte> data)
    {
        TableId = data[0];
        SectionSyntaxIndicator = Convert.ToBoolean((data[1] & 0x80) >> 7);
        SectionLength = (ushort)(((data[1] & 0x0f) << 8) | data[2]);
        ServiceId = BinaryPrimitives.ReadUInt16BigEndian(data[3..5]);
        VersionNumber = (byte)((data[5] & 0x3e) >> 1);
        CurrentNextIndicator = Convert.ToBoolean(data[5] & 0x01);
        SectionNumber = data[6];
        LastSectionNumber = data[7];

        TlvStreamId = BinaryPrimitives.ReadUInt16BigEndian(data[8..10]);
        OriginalNetworkId = BinaryPrimitives.ReadUInt16BigEndian(data[10..12]);
        SegmentLastSectionNumber = data[12];
        LastTableId = data[13];

        var pos = 14;
        var events = new List<Event>();
        while (pos < data.Length)
        {
            var eventId = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            var startTime = Utilities.ParseJSTandMJDbytes(data[(pos + 2)..(pos + 7)]);
            var duration = Utilities.ParseBCDDuration(data[(pos + 7)..(pos + 10)]);
            var runningStatus = (StateOfService)((data[pos + 10] & 0xe0) >> 5);
            var freeCaMode = Convert.ToBoolean((data[pos + 10] & 0x10) >> 4);
            var descriptorLoopLength = (data[pos + 10] & 0x0f) << 8 | data[pos + 11];
            pos += 12;
            var descriptor = Descriptor.ParseDescriptors(data[pos..(pos + descriptorLoopLength)], typeof(MHEventInformationTable));
            pos += descriptorLoopLength;

            events.Add(new Event(eventId, startTime, duration, runningStatus, freeCaMode, descriptor));
        }
        Events = events.ToArray();
    }
}

public record MHCommonDataTable : M2SectionMessageTable
{
    public abstract record DataModuleRecord;

    public record LogoDataByte : DataModuleRecord
    {
        public enum LogoTypeEnum : byte
        {
            Large = 0x07,
            Small = 0x06,
            TwoD = 0x05
        }

        public LogoTypeEnum LogoType { get; init; }
        public ushort LogoId { get; init; }
        public ushort LogoVersion { get; init; }
        public ushort DataSize { get; init; }
        // PNG
        public byte[] LogoData { get; init; }

        public LogoDataByte(ReadOnlySpan<byte> data)
        {
            LogoType = (LogoTypeEnum)data[0];
            LogoId = (ushort)(((data[1] & 0x01) << 8) | data[2]);
            LogoVersion = (ushort)(((data[3] & 0x0f) << 8) | data[4]);
            DataSize = BinaryPrimitives.ReadUInt16BigEndian(data[5..7]);
            LogoData = data[7..(DataSize + 7)].ToArray();
        }
    }

    public enum DataTypeEnum : byte
    {
        LogoData = 0x01
    }

    public byte TableId { get; init; }
    public bool SectionSyntaxIndicator { get; init; }
    public ushort SectionLength { get; init; }
    public ushort DownloadDataId { get; init; }
    public byte VersionNumber { get; init; }
    public bool CurrentNextIndicator { get; init; }
    public byte SectionNumber { get; init; }
    public byte LastSectionNumber { get; init; }
    public ushort OriginalNetworkId { get; init; }
    public DataTypeEnum DataType { get; init; }
    public byte[] Descriptor { get; init; }
    public DataModuleRecord DataModule { get; init; }
    public MHCommonDataTable(ReadOnlySpan<byte> data)
    {
        TableId = data[0];
        SectionSyntaxIndicator = Convert.ToBoolean((data[1] & 0x80) >> 7);
        SectionLength = (ushort)(((data[1] & 0x0f) << 8) | data[2]);
        DownloadDataId = BinaryPrimitives.ReadUInt16BigEndian(data[3..5]);
        VersionNumber = (byte)((data[5] & 0x3e) >> 1);
        CurrentNextIndicator = Convert.ToBoolean(data[5] & 0x01);
        SectionNumber = data[6];
        LastSectionNumber = data[7];

        OriginalNetworkId = BinaryPrimitives.ReadUInt16BigEndian(data[8..10]);
        DataType = (DataTypeEnum)data[10];
        var descriptorsLoopLength = ((data[11] & 0x0f) << 8) | data[12];
        Descriptor = data[13..(13 + descriptorsLoopLength)].ToArray();
        DataModule = DataType switch
        {
            DataTypeEnum.LogoData => new LogoDataByte(data[(13 + descriptorsLoopLength)..]),
            _ => throw new InvalidDataException()
        };
    }
}

public record MHBroadcasterInformationTable : M2SectionMessageTable
{
    public record BroadcasterIdAndDescriptor(byte BroadcasterId, Descriptor[] Descriptor);

    public byte TableId { get; init; }
    public bool SectionSyntaxIndicator { get; init; }
    public ushort SectionLength { get; init; }
    public ushort OriginalNetworkId { get; init; }
    public byte VersionNumber { get; init; }
    public bool CurrentNextIndicator { get; init; }
    public byte SectionNumber { get; init; }
    public byte LastSectionNumber { get; init; }
    public bool BroadcastViewPropriety { get; init; }
    public Descriptor[] Descriptors { get; init; }
    public BroadcasterIdAndDescriptor[] BroadcasterIdInfo { get; init; }

    public MHBroadcasterInformationTable(ReadOnlySpan<byte> data)
    {
        TableId = data[0];
        SectionSyntaxIndicator = Convert.ToBoolean((data[1] & 0x80) >> 7);
        SectionLength = (ushort)(((data[1] & 0x0f) << 8) | data[2]);
        OriginalNetworkId = BinaryPrimitives.ReadUInt16BigEndian(data[3..5]);
        VersionNumber = (byte)((data[5] & 0x3e) >> 1);
        CurrentNextIndicator = Convert.ToBoolean(data[5] & 0x01);
        SectionNumber = data[6];
        LastSectionNumber = data[7];

        BroadcastViewPropriety = Convert.ToBoolean((data[8] & 0x10) >> 4);
        var firstDescrriptorsLength = ((data[8] & 0x0f) << 8) | data[9];
        Descriptors = Descriptor.ParseMHBITDescriptors(data[10..(10 + firstDescrriptorsLength)], isFirstLoop: true);

        var pos = 10 + firstDescrriptorsLength;
        var descriptors = new List<BroadcasterIdAndDescriptor>();
        while (pos < data.Length)
        {
            var broadcasterId = data[pos];
            var length = ((data[pos + 1] & 0x0f) << 8) | data[pos + 2];
            var descriptorsInfo = Descriptor.ParseMHBITDescriptors(data[(pos + 3)..(pos + 3 + length)], isFirstLoop: false);
            var idAndDesc = new BroadcasterIdAndDescriptor(broadcasterId, descriptorsInfo);
            descriptors.Add(idAndDesc);
            pos += 3 + length;
        }
        BroadcasterIdInfo = descriptors.ToArray();
    }
}

public record MHSoftwareDownloadTriggerTable : M2SectionMessageTable
{
    public record TimeSchedule(DateTime StartTime, TimeSpan Duration);

    public record Content(byte Group, ushort TargetVersion, ushort NewVersion, byte DownloadLevel,
        byte VersionIndicator, ushort ContentDescriptionLength, byte ScheduleTimeshiftInformation,
        TimeSchedule[] Schedules, Descriptor[] Descriptors);


    public byte TableId { get; init; }
    public bool SectionSyntaxIndicator { get; init; }
    public ushort SectionLength { get; init; }
    public ushort TableIdExtension { get; init; }
    public byte VersionNumber { get; init; }
    public bool CurrentNextIndicator { get; init; }
    public byte SectionNumber { get; init; }
    public byte LastSectionNumber { get; init; }
    public ushort TlvStreamId { get; init; }
    public ushort OriginalNetworkId { get; init; }
    public ushort ServiceId { get; init; }
    public Content[] Contents { get; init; }

    public MHSoftwareDownloadTriggerTable(ReadOnlySpan<byte> data)
    {
        TableId = data[0];
        SectionSyntaxIndicator = Convert.ToBoolean((data[1] & 0x80) >> 7);
        SectionLength = (ushort)(((data[1] & 0x0f) << 8) | data[2]);
        TableIdExtension = BinaryPrimitives.ReadUInt16BigEndian(data[3..5]);
        VersionNumber = (byte)((data[5] & 0x3e) >> 1);
        CurrentNextIndicator = Convert.ToBoolean(data[5] & 0x01);
        SectionNumber = data[6];
        LastSectionNumber = data[7];

        TlvStreamId = BinaryPrimitives.ReadUInt16BigEndian(data[8..10]);
        OriginalNetworkId = BinaryPrimitives.ReadUInt16BigEndian(data[10..12]);
        ServiceId = BinaryPrimitives.ReadUInt16BigEndian(data[12..14]);
        var numOfContents = data[14];

        var contents = new Content[numOfContents];
        var pos = 15;
        for (var i = 0; i < numOfContents; i++)
        {
            var group = (byte)((data[pos] & 0xf0) >> 4);
            var targetVersion = (ushort)(((data[pos] & 0x0f) << 8) | data[pos + 1]);
            var newVersion = (ushort)((data[pos + 1] << 4) | ((data[pos + 2] & 0xf0) >> 4));
            var downloadLevel = (byte)((data[pos + 2] & 0x0c) >> 2);
            var versionIndicator = (byte)(data[pos + 3] & 0x03);
            var contentDescriptionLength = (ushort)((data[pos + 4] << 4) | ((data[pos + 5] & 0xf0) >> 4));
            var scheduleDescriptionLength = (byte)((data[pos + 6] << 4) | ((data[pos + 7] & 0xf0) >> 4));
            var scheduleTimeshiftInformation = (byte)(data[pos + 8] & 0x0f);

            var schedules = new List<TimeSchedule>();
            foreach (var scheduleBytes in data[(pos + 9)..(pos + 9 + scheduleDescriptionLength)].ToArray().Chunk(8))
            {
                var startTime = Utilities.ParseJSTandMJDbytes(scheduleBytes[..5]);
                var duration = Utilities.ParseBCDDuration(scheduleBytes[5..8]);
                schedules.Add(new TimeSchedule(startTime, duration));
            }
            pos += 9 + scheduleDescriptionLength;

            var descriptorsLength = contentDescriptionLength - scheduleDescriptionLength;
            var descriptors = Descriptor.ParseDescriptors(data[pos..(pos + descriptorsLength)], typeof(MHSoftwareDownloadTriggerTable));
            contents[i] = new Content(group, targetVersion, newVersion, downloadLevel, versionIndicator,
                contentDescriptionLength, scheduleTimeshiftInformation, schedules.ToArray(), descriptors);

            pos += descriptorsLength;
        }

        Contents = contents;
    }
}

public record MHServiceDescriptionTable : M2SectionMessageTable
{
    public record ServiceInfo(ushort ServiceId, byte EITUserDefinedFlags, bool EITScheduleFlag, bool EITPresentFollowingFlag, StateOfService RunningStatus, bool FreeCAMode, Descriptor[] Descriptor);

    public byte TableId { get; init; }
    public bool SectionSyntaxIndicator { get; init; }
    public ushort SectionLength { get; init; }
    public ushort TlvStreamId { get; init; }
    public byte VersionNumber { get; init; }
    public bool CurrentNextIndicator { get; init; }
    public byte SectionNumber { get; init; }
    public byte LastSectionNumber { get; init; }
    public ushort OriginalNetworkId { get; init; }
    public ServiceInfo[] ServiceInfos { get; init; }

    public MHServiceDescriptionTable(ReadOnlySpan<byte> data)
    {
        TableId = data[0];
        SectionSyntaxIndicator = Convert.ToBoolean((data[1] & 0x80) >> 7);
        SectionLength = (ushort)(((data[1] & 0x0f) << 8) | data[2]);
        TlvStreamId = BinaryPrimitives.ReadUInt16BigEndian(data[3..5]);
        VersionNumber = (byte)((data[5] & 0x3e) >> 1);
        CurrentNextIndicator = Convert.ToBoolean(data[5] & 0x01);
        SectionNumber = data[6];
        LastSectionNumber = data[7];

        OriginalNetworkId = BinaryPrimitives.ReadUInt16BigEndian(data[8..10]);

        var pos = 11;
        var infos = new List<ServiceInfo>();
        while (pos < data.Length)
        {
            var serviceId = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            var eitUserDefinedFlags = (byte)((data[pos + 2] & 0x1c) >> 2);
            var eitScheduleFlag = Convert.ToBoolean((data[pos + 2] & 0x02) >> 1);
            var eitPresentFollowingFlag = Convert.ToBoolean(data[pos + 2] & 0x01);
            var runningStatus = (StateOfService)((data[pos + 3] & 0xe0) >> 5);
            var freeCaMode = Convert.ToBoolean((data[pos + 3] & 0x10) >> 4);
            var descriptorsLoopLength = ((data[pos + 3] & 0x0f) << 8) | data[pos + 4];
            var descriptor = Descriptor.ParseDescriptors(data[(pos + 5)..(pos + 5 + descriptorsLoopLength)], typeof(MHServiceDescriptionTable));
            pos += 5 + descriptorsLoopLength;

            infos.Add(new ServiceInfo(serviceId, eitUserDefinedFlags, eitScheduleFlag, eitPresentFollowingFlag,
                    runningStatus, freeCaMode, descriptor));
        }
        ServiceInfos = infos.ToArray();
    }
}

public record MHSelectionInformationTable : M2SectionMessageTable
{
    public record SelectionInformation(ushort ServiceId, StateOfService RunningStatus, byte[] Descriptor);

    public byte TableId { get; init; }
    public bool SectionSyntaxIndicator { get; init; }
    public ushort SectionLength { get; init; }
    public byte VersionNumber { get; init; }
    public bool CurrentNextIndicator { get; init; }
    public byte SectionNumber { get; init; }
    public byte LastSectionNumber { get; init; }
    Descriptor[] Descriptors { get; init; }
    public SelectionInformation[] SelectionInformations { get; init; }

    public MHSelectionInformationTable(ReadOnlySpan<byte> data)
    {
        TableId = data[0];
        SectionSyntaxIndicator = Convert.ToBoolean((data[1] & 0x80) >> 7);
        SectionLength = (ushort)(((data[1] & 0x0f) << 8) | data[2]);
        VersionNumber = (byte)((data[5] & 0x3e) >> 1);
        CurrentNextIndicator = Convert.ToBoolean(data[5] & 0x01);
        SectionNumber = data[6];
        LastSectionNumber = data[7];

        var transmissionInfoLoopLength = ((data[8] & 0x0f) << 8) | data[9];
        Descriptors = Descriptor.ParseDescriptors(data[10..(10 + transmissionInfoLoopLength)], typeof(MHSelectionInformationTable));

        var selectionInfos = new List<SelectionInformation>();
        var pos = 10 + transmissionInfoLoopLength;
        while (pos < data.Length)
        {
            var serviceId = BinaryPrimitives.ReadUInt16BigEndian(data[pos..(pos + 2)]);
            var runningStatus = (StateOfService)((data[pos + 2] & 0x70) >> 4);
            var loopLength = ((data[pos + 2] & 0x0f) << 8) | data[pos + 3];
            var descriptor = data[(pos + 4)..(pos + 4 + loopLength)].ToArray();

            selectionInfos.Add(new SelectionInformation(serviceId, runningStatus, descriptor));
            pos += 4 + loopLength;
        }

        SelectionInformations = selectionInfos.ToArray();
    }
}

public record MHApplicationInformationTable : M2SectionMessageTable
{
    public enum ApplicationTypeEnum : ushort
    {
        ARIBJ = 0x0001,
        LiaisonHTML5 = 0x0010,
        HTML5 = 0x0011
    }

    public enum ApplicationControlCodeEnum : byte
    {
        Autostart = 0x01,
        Present = 0x02,
        Kill = 0x04,
        Prefetch = 0x05
    }

    public record ApplicationInfo(ApplicationIdentifier ApplicationIdentifier, ApplicationControlCodeEnum ApplicationControlCode, Descriptor[] Descriptors);

    public byte TableId { get; init; }
    public bool SectionSyntaxIndicator { get; init; }
    public ushort SectionLength { get; init; }
    public ApplicationTypeEnum ApplicationType { get; init; }
    public byte VersionNumber { get; init; }
    public bool CurrentNextIndicator { get; init; }
    public byte SectionNumber { get; init; }
    public byte LastSectionNumber { get; init; }
    public Descriptor[] Descriptors { get; init; }
    public ApplicationInfo[] ApplicationInfos { get; init; }

    public MHApplicationInformationTable(ReadOnlySpan<byte> data)
    {
        TableId = data[0];
        SectionSyntaxIndicator = Convert.ToBoolean((data[1] & 0x80) >> 7);
        SectionLength = (ushort)(((data[1] & 0x0f) << 8) | data[2]);
        ApplicationType = (ApplicationTypeEnum)BinaryPrimitives.ReadUInt16BigEndian(data[3..5]);
        VersionNumber = (byte)((data[5] & 0x3e) >> 1);
        CurrentNextIndicator = Convert.ToBoolean(data[5] & 0x01);
        SectionNumber = data[6];
        LastSectionNumber = data[7];

        var commonDescriptorLength = ((data[8] & 0x0f) << 8) | data[9];
        Descriptors = Descriptor.ParseDescriptors(data[10..(10 + commonDescriptorLength)], typeof(MHApplicationInformationTable));

        var pos = 10 + commonDescriptorLength;
        var applicationLoopLength = ((data[pos] & 0x0f) << 8) | data[pos + 1];
        var applicationLoopMaxPos = pos + 2 + applicationLoopLength;
        pos += 2;

        var applicationInfos = new List<ApplicationInfo>();
        while (pos < applicationLoopMaxPos)
        {
            var applicationIdentifier = new ApplicationIdentifier(data[pos..(pos + 6)]);
            var applicationControlCode = (ApplicationControlCodeEnum)data[pos + 6];
            var applicationDescriptorLoopLength = ((data[pos + 7] & 0x0f) << 8) | data[pos + 8];
            var descriptor = Descriptor.ParseDescriptors(data[(pos + 9)..(pos + 9 + applicationDescriptorLoopLength)], typeof(MHApplicationInformationTable));
            applicationInfos.Add(new ApplicationInfo(applicationIdentifier, applicationControlCode, descriptor));
            pos += 9 + applicationDescriptorLoopLength;
        }

        ApplicationInfos = applicationInfos.ToArray();
    }
}

public record ApplicationIdentifier
{
    public ushort OrganizationId { get; init; }
    public uint ApplicationId { get; init; }
    public ApplicationIdentifier(ReadOnlySpan<byte> data)
    {
        OrganizationId = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);
        ApplicationId = BinaryPrimitives.ReadUInt32BigEndian(data[2..6]);
    }
}