using CommandLine;
using MMTTLVParser.Sample;
using MMTTLVParser.Sample.CC;
using MMTTLVParser.Sample.Datacasting;
using MMTTLVParser.Sample.Statistics;

CommandLine.Parser.Default.ParseArguments<Options>(args)
    .WithParsed(opt =>
    {
        using var file = File.OpenRead(opt.FileName);
        using var reader = new BinaryReader(file);
        var parser = new MMTTLVParser.Parser.Parser();

        var ccFilter = new CCFilter(file.Name);
        var dcFilter = new DatacastingFilter(file.Name);
        var stasticsFilter = new StatisticsFilter();

        var buffer = new byte[1024 * 1024];
        int readLength;

        Console.WriteLine($"Processing file: {opt.FileName}");

        while ((readLength = reader.Read(buffer, 0, buffer.Length)) > 0)
        {
            try
            {
                var read = buffer.AsSpan(0, readLength);
                var packets = parser.Parse(read);
                if (packets is null)
                {
                    continue;
                }

                if (opt.EnableParseCC)
                {
                    ccFilter.ProcessPackets(packets);
                }

                if (opt.EnableParseDatacasting)
                {
                    dcFilter.ProcessPackets(packets);
                }

                stasticsFilter.ProcessPackets(packets);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }

        Console.WriteLine("================= Statistics =================");
        foreach (var type in stasticsFilter._typeCounts.Keys)
        {
            Console.WriteLine($"{type}: {stasticsFilter._typeCounts[type]}");
        }
        Console.WriteLine("==============================================");
    });