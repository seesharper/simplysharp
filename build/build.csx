#load "nuget:Dotnet.Build, 0.16.1"
#load "nuget:dotnet-steps, 0.0.2"


[StepDescription("Creates the extension package")]
AsyncStep pack = async () =>
{
    await Command.ExecuteAsync("docker", "build --tag vsce \"https://github.com/microsoft/vscode-vsce.git#main\"");

    await Command.ExecuteAsync("docker", $"docker run --rm -it -v {BuildContext.SourceFolder}:/workspace vsce publish");
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

