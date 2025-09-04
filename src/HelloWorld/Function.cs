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
            var driveService = await GetDriveService();
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

    private async Task<DriveService> GetDriveService()
    {
        // Secrets ManagerからGCPの認証情報を取得
        var secretsManagerClient = new AmazonSecretsManagerClient();
        var secretRequest = new GetSecretValueRequest
        {
            SecretId = "gcp/credentials" // ステップ1で付けた名前
        };
        var secretResponse = await secretsManagerClient.GetSecretValueAsync(secretRequest);

        // 取得した認証情報（JSON文字列）を使って認証
        string[] scopes = { DriveService.Scope.DriveReadonly };
        var credential = GoogleCredential.FromJson(secretResponse.SecretString).CreateScoped(scopes);

        return new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "GdriveFolderNameGetterLambda"
        });
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