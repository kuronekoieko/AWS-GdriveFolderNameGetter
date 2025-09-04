using System;
using System.Net;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

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
        if (string.IsNullOrEmpty(input.FolderId))
        {
            return CreateResponse(HttpStatusCode.BadRequest, "Error: FolderId is required.");
        }

        try
        {
            var driveServiceHelper = new DriveServiceHelper(); // 新しいクラスのインスタンスを作成
            var driveService = await driveServiceHelper.GetDriveService();
            var request = driveService.Files.Get(input.FolderId);
            request.Fields = "name";
            var file = await request.ExecuteAsync();

            return CreateResponse(HttpStatusCode.OK, $"Folder Name: {file.Name}");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error: {ex.ToString()}");
            return CreateResponse(HttpStatusCode.InternalServerError, $"Error: {ex.Message}");
        }
    }

    private APIGatewayProxyResponse CreateResponse(HttpStatusCode statusCode, string body)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)statusCode,
            Body = JsonSerializer.Serialize(new { message = body }),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
}