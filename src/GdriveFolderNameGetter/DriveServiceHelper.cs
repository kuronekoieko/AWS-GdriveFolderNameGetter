using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace GdriveFolderNameGetter;

public class DriveServiceHelper
{
    public async Task<DriveService> GetDriveService()
    {
        var secretsManagerClient = new AmazonSecretsManagerClient();
        var secretRequest = new GetSecretValueRequest { SecretId = "gcp/credentials" };
        var secretResponse = await secretsManagerClient.GetSecretValueAsync(secretRequest);

        string[] scopes = [DriveService.Scope.DriveReadonly];
        var credential = GoogleCredential.FromJson(secretResponse.SecretString).CreateScoped(scopes);

        return new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "GdriveFolderNameGetterLambda"
        });
    }
}