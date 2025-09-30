using System.Buffers.Binary;
using System.Text;

namespace MMTTLVParser.Sample.Datacasting;

public record IndexItem
{
    public record Item(uint ItemId, uint ItemSize, byte ItemVersion, string FileName, byte[]? ItemChecksum, string ItemType, CompressionType CompressionType, uint? OriginalSize);

    public enum CompressionType : byte
    {
        Zlib = 0,
        Uncompressed = 0xff
    }

    public Item[] Items { get; init; }

    public IndexItem(ReadOnlySpan<byte> data)
    {
        var numOfItems = BinaryPrimitives.ReadUInt16BigEndian(data[..2]);

        Items = new Item[numOfItems];
        var pos = 2;
        for (var i = 0; i < numOfItems; i++)
        {
            var itemId = BinaryPrimitives.ReadUInt32BigEndian(data[pos..(pos + 4)]);
            var itemSize = BinaryPrimitives.ReadUInt32BigEndian(data[(pos + 4)..(pos + 8)]);
            var itemVersion = data[pos + 8];
            var fileNameLength = data[pos + 9];
            var fileName = Encoding.UTF8.GetString(data[(pos + 10)..(pos + 10 + fileNameLength)]);
            pos += 10 + fileNameLength;

            var checksumFlag = Convert.ToBoolean((data[pos] & 0x80) >> 7);
            var itemChecksum = checksumFlag ? data[(pos + 1)..(pos + 5)].ToArray() : null;
            pos += checksumFlag ? 5 : 1;

            var itemTypeLength = data[pos];
            var itemType = Encoding.UTF8.GetString(data[(pos + 1)..(pos + 1 + itemTypeLength)]);
            pos += 1 + itemTypeLength;

            var compressionType = data[pos];
            uint? originalSize = compressionType == 0xff ? null : BinaryPrimitives.ReadUInt32BigEndian(data[(pos + 1)..(pos + 5)]);
            pos += 5;

            Items[i] = new Item(itemId, itemSize, itemVersion, fileName, itemChecksum, itemType, (CompressionType)compressionType, originalSize);
        }
    }
}
