using System;
using System.Net;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GdriveFolderNameGetter;

public class FunctionInput
{
    public string? FolderId { get; set; }
}

public class Function
{
    public async Task<APIGatewayProxyResponse> FunctionHandler(FunctionInput input, ILambdaContext context)
    {
        ResponseBody responseBody = new();

        if (string.IsNullOrEmpty(input.FolderId))
        {
            responseBody.Error = "FolderId is required.";
            return CreateResponse(HttpStatusCode.BadRequest, responseBody);
        }

        try
        {
            var driveServiceHelper = new DriveServiceHelper(); // 新しいクラスのインスタンスを作成
            var driveService = await driveServiceHelper.GetDriveService();
            var request = driveService.Files.Get(input.FolderId);
            request.Fields = "name";
            var file = await request.ExecuteAsync();


            responseBody.FolderName = file.Name;
            return CreateResponse(HttpStatusCode.OK, responseBody);
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error: {ex.ToString()}");
            responseBody.Error = ex.Message;
            return CreateResponse(HttpStatusCode.InternalServerError, responseBody);
        }
    }

    private APIGatewayProxyResponse CreateResponse(HttpStatusCode statusCode, ResponseBody responseBody)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)statusCode,
            Body = JsonSerializer.Serialize(responseBody),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
}