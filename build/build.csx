#load "nuget:Dotnet.Build, 0.16.1"
#load "nuget:dotnet-steps, 0.0.2"


var pathToExtensionArtifact = FileUtils.CreateDirectory(BuildContext.ArtifactsFolder, "Marketplace");

[StepDescription("Creates the extension package")]
AsyncStep pack = async () =>
{

    await Command.ExecuteAsync("docker", "build --tag vsce \"https://github.com/microsoft/vscode-vsce.git#main\"");
    WriteLine(BuildContext.SourceFolder);
    await Command.ExecuteAsync("docker", $"run --rm  -v {BuildContext.RepositoryFolder}:/workspace vsce package -o build/Artifacts/Marketplace/test.vsix");
};
//docker run --rm -it -v "$(pwd)":/workspace vsce publish
[DefaultStep]
[StepDescription("Deploys packages if we are on a tag commit in a secure environment.")]
AsyncStep deploy = async () =>
{
    await pack();
    //await Artifacts.Deploy();
};

await StepRunner.Execute(Args);
return 0;

