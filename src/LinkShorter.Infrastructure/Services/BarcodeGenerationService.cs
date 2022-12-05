using IronBarCode;
using LinkShorter.Application.Services;
namespace LinkShorter.Infrastructure.Services;

public class BarcodeGenerationService : IBarcodeGenerationService
{
    public Stream GenerateAsImageStream(string url)
    {
       var barcode = BarcodeWriter.CreateBarcode(url, BarcodeWriterEncoding.QRCode);
       // TODO: toPngStream() inside uses using, so returned stream was closed
       
       return new MemoryStream(barcode.ToPngBinaryData(), false);
    }
       
}