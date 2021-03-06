#addin nuget:?package=SharpZipLib&version=0.86.0
#addin nuget:?package=Cake.Compression&version=0.1.1
#addin "Cake.Docker"


var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var publishDir = Directory("../../tmp/mongomessage/");
var dockerDir = Directory("./dockerfiles/");


Task("Clean")
    .Does(() =>
{
    CleanDirectory(publishDir);
});

Task("Restore")
.IsDependentOn("Clean")
  .Does(() =>
{
    DotNetCoreRestore("./src/MongoMessage/");
});

Task("Build")
    .IsDependentOn("Restore")
  .Does(() =>
{
   var settings = new DotNetCoreBuildSettings
     {
         Framework = "netcoreapp1.0",
         Configuration = configuration,
         OutputDirectory = "./artifacts/"
     };
       DotNetCoreBuild("./src/MongoMessage/", settings);
  
});

Task("Publish")
    .IsDependentOn("Build")
  .Does(() =>
{
      var settings = new DotNetCorePublishSettings
     {
         Framework = "netcoreapp1.0",
         Configuration = configuration,
         OutputDirectory = publishDir
     };

     DotNetCorePublish("./src/MongoMessage/", settings);

});

Task("Compression")
    .IsDependentOn("Publish")
  .Does(() =>
{
    GZipCompress(publishDir, dockerDir + Directory("app") + File( "mongomessage.tar.gz"), 9);
});

Task("DockerBuild")
    .IsDependentOn("Compression")
  .Does(() =>
{
    var settings = new DockerBuildSettings
     {
         File = dockerDir + File("Dockerfile"),
         Tag = new [] {"fpommerening/dotnetinthebox:mongomessage"}
     };
    DockerBuild(settings, dockerDir);
});


//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("DockerBuild");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);