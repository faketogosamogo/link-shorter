namespace LinkShorter.Core.Entities;

public class BarcodeInfo : BaseEntity
{
    public BarcodeInfo(string path, long shortLinkId)
    {
        Path = path;
        ShortLinkId = shortLinkId;
    }
    
    // ef
    private BarcodeInfo()
    {
    }

    public string Path { get; } = null!;

    public long ShortLinkId { get; }
}