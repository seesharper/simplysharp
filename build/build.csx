#load "nuget:Dotnet.Build, 0.16.1"
#load "nuget:dotnet-steps, 0.0.2"

var pathToExtensionArtifactsFolder = FileUtils.CreateDirectory(BuildContext.ArtifactsFolder, "Marketplace");
var pathToExtensionArtifact = Path.Combine(pathToExtensionArtifactsFolder, $"simplysharp-{BuildContext.LatestTag}");

var MARKETPLACE_ACCESSTOKEN = Environment.GetEnvironmentVariable("MARKETPLACE_ACCESSTOKEN");

[StepDescription("Creates the extension package")]
AsyncStep pack = async () =>
{
    await Command.ExecuteAsync("docker", "build --tag vsce \"https://github.com/microsoft/vscode-vsce.git#main\"");
    await Command.ExecuteAsync("docker", $"run --rm  -v {BuildContext.RepositoryFolder}:/workspace vsce package -o build/Artifacts/Marketplace/simplysharp-{BuildContext.LatestTag}.vsix --no-git-tag-version --no-update-package-json {BuildContext.LatestTag}");
};

[DefaultStep]
[StepDescription("Deploys packages if we are on a tag commit in a secure environment.")]
AsyncStep deploy = async () =>
{
    await pack();
    if (!BuildEnvironment.IsSecure)
    {
        Logger.Log("Publishing artifacts can only be done in a secure environment");
        return;
    }

    await GitHub.CreateChangeLog();

    if (!BuildContext.IsTagCommit)
    {
        Logger.Log("Publishing artifacts can only be done if we are on a tag commit");
        return;
    }

    Git.Open(BuildContext.RepositoryFolder).RequireCleanWorkingTree();

    await GitHub.Release();

    await Command.ExecuteAsync("docker", $"run --rm  -v {BuildContext.RepositoryFolder}:/workspace vsce publish -p {MARKETPLACE_ACCESSTOKEN} -i build/Artifacts/Marketplace/simplysharp-{BuildContext.LatestTag}.vsix --no-git-tag-version --no-update-package-json");

};

await StepRunner.Execute(Args);
return 0;

