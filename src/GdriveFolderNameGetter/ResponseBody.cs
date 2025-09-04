
namespace GdriveFolderNameGetter;

// JsonSerializerがデフォルトでシリアライズ（JSON化）するのは、publicなプロパティ ({ get; set; }) だけ
public class ResponseBody
{
    public string FolderName { get; set; } = "";
    public string Error { get; set; } = "";
    public string Message { get; set; } = "";
}