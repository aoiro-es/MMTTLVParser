using MMTTLVParser.PacketDefinitions.MMT.Table;
using static MMTTLVParser.PacketDefinitions.MMT.Table.MHApplicationInformationTable;

namespace MMTTLVParser.Sample.Datacasting.Model;

/// <summary>
/// MT-AITのApplicationInfoとその記述子を保持するモデル
/// </summary>
public record ApplicationInfoModel
{
    // AIT
    public MHApplicationInformationTable MHAIT { get; init; }

    // descriptors
    public ApplicationInfo ApplicationInfo { get; init; }
    public MHApplicationDescriptor? MHApplicationDescriptor { get; init; }
    public MHTransportProtocolDescriptor? MHTransportProtocolDescriptor { get; init; }
    public MHSimpleApplicationLocationDescriptor? MHSimpleApplicationLocationDescriptor { get; init; }
    public MHApplicationBoundaryAndPermissionDescriptor? MHApplicationBoundaryAndPermissionDescriptor { get; init; }

    public ApplicationInfoModel(MHApplicationInformationTable mhAit, ApplicationInfo ai)
    {
        MHAIT = mhAit;
        ApplicationInfo = ai;
        MHApplicationDescriptor = ai.Descriptors.OfType<MHApplicationDescriptor>().FirstOrDefault();
        MHTransportProtocolDescriptor = ai.Descriptors.OfType<MHTransportProtocolDescriptor>().FirstOrDefault();
        MHSimpleApplicationLocationDescriptor = ai.Descriptors.OfType<MHSimpleApplicationLocationDescriptor>().FirstOrDefault();
        MHApplicationBoundaryAndPermissionDescriptor = ai.Descriptors.OfType<MHApplicationBoundaryAndPermissionDescriptor>().FirstOrDefault();
    }
}