#r "./tools/FAKE/tools/FakeLib.dll"
#load "Choco.fsx"
#load "NVikaHelper.fsx"
#load "SemanticReleaseNotesParserHelper.fsx"

open Fake
open Fake.Testing.XUnit2
open Fake.NuGet.Install
open OpenCoverHelper
open NVikaHelper
open SemanticReleaseNotesParserHelper

// Properties
let buildDir = "./build/"
let testDir  = "./test/"
let artifactsDir = "./artifacts/"

// version info
let version = if isLocalBuild then "0.0.1" else buildVersion
let tag = if buildServer = AppVeyor then AppVeyor.AppVeyorEnvironment.RepoTagName else "v0.0.1"

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; testDir; artifactsDir]
)

Target "RestorePackages" (fun _ ->
    "./src/Vika.sln"
    |> RestoreMSSolutionPackages (fun p -> { p with OutputPath = "src/packages" })
)

Target "BuildApp" (fun _ ->
    !! "src/NVika/NVika.csproj"
      |> MSBuildRelease buildDir "Build"
      |> Log "AppBuild-Output: "
)

Target "InspectCodeAnalysis" (fun _ ->
    "resharper-clt.portable" |> Choco.Install id
    
    if directExec(fun info ->
        info.FileName <- "inspectcode"
        info.Arguments <- "/o=\"" + artifactsDir + "inspectcodereport.xml\" /project=\"NVika\" \"src\Vika.sln\"" )
    then
        artifactsDir + "inspectcodereport.xml" |> NVika.ParseReport (fun p -> { p with Debug = true; IncludeSource = true; ToolPath = buildDir @@ "NVika.exe" })
            
    else traceError "Execution of inspectcode have failed, NVika can't be executed."
)

Target "BuildReleaseNotes" (fun _ ->
     SemanticReleaseNotesParser.Convert (fun p -> { p with 
                                                        GroupBy = SemanticReleaseNotesParser.GroupByType.Categories
                                                        Debug = true
                                                        OutputPath = buildDir @@ "ReleaseNotes.html"
        } )

     buildDir @@ "ReleaseNotes.html" |> FileHelper.CopyFile artifactsDir

     SemanticReleaseNotesParser.Convert (fun p -> { p with 
                                                        GroupBy = SemanticReleaseNotesParser.GroupByType.Categories
                                                        OutputType = SemanticReleaseNotesParser.OutputType.Environment
                                                        OutputFormat = SemanticReleaseNotesParser.OutputFormat.Markdown
                                                        Debug = true
        } )
)

Target "BuildTest" (fun _ ->
    !! "src/NVika.Tests/NVika.Tests.csproj"
      |> MSBuildRelease testDir "Build"
      |> Log "AppBuild-Output: "
)

Target "Test" (fun _ ->
    "xunit.runner.console" |> NugetInstall (fun p -> 
    { p with 
        OutputDirectory = "tools";
        ExcludeVersion = true
    })
    
    if isUnix then
        !! (testDir @@ "NVika.Tests.dll")
        |> xUnit2 (fun p -> { p with 
                                HtmlOutputPath = Some (artifactsDir @@ "xunit.html") 
                                ShadowCopy = false
                            })
    else

        "OpenCover" |> NugetInstall (fun p -> 
        { p with 
            OutputDirectory = "tools";
            ExcludeVersion = true
        })
    
        testDir + "NVika.Tests.dll -noshadow" |> OpenCover (fun p -> 
        { p with
            ExePath = "./tools/OpenCover/tools/OpenCover.Console.exe"
            TestRunnerExePath = "./tools/xunit.runner.console/tools/xunit.console.exe";
            Output = artifactsDir @@ "coverage.xml";
            Register = RegisterUser;
            Filter = "+[NVika]*";
            OptionalArguments = "-excludebyattribute:*.ExcludeFromCodeCoverage* -returntargetcode";
        })
    
        if isLocalBuild then
            "ReportGenerator" |> NugetInstall (fun p -> 
            { p with 
                OutputDirectory = "tools";
                ExcludeVersion = true
            })
            [artifactsDir @@ "coverage.xml"] |> ReportGeneratorHelper.ReportGenerator (fun p -> 
            { p with 
                TargetDir = artifactsDir @@ "reports" 
                ExePath = @"tools\ReportGenerator\tools\ReportGenerator.exe"
                LogVerbosity = ReportGeneratorHelper.ReportGeneratorLogVerbosity.Error
            })
        else
            "coveralls.io" |> NugetInstall (fun p -> 
            { p with 
                OutputDirectory = "tools";
                ExcludeVersion = true
            })
            if not (directExec(fun info ->
                info.FileName <- @"tools\coveralls.io\tools\coveralls.net.exe"
                info.Arguments <- "--opencover " + artifactsDir + "coverage.xml" ))
            then
                failwith "Execution of coveralls.net have failed."
)

Target "Zip" (fun _ ->
    !! (buildDir + "/*.dll")
    ++ (buildDir + "NVika.exe")
    ++ (buildDir + "NVika.exe.config")
    ++ (buildDir + "ReleaseNotes.html")
    |> Zip buildDir (artifactsDir + "NVika." + version + ".zip")
)

Target "PackFakeHelper" (fun _ ->
    "NVikaHelper.fsx" |> FileHelper.CopyFile artifactsDir
    artifactsDir @@ "NVikaHelper.fsx" |> FileHelper.RegexReplaceInFileWithEncoding "./tools/FAKE/tools/FakeLib.dll" "FakeLib.dll" System.Text.Encoding.UTF8
)

Target "ChocoPack" (fun _ ->
    FileUtils.cp_r "PackagingAssets\chocolatey" (buildDir @@ "chocolatey")
    buildDir + @"chocolatey\tools\chocolateyInstall.ps1" |> FileHelper.RegexReplaceInFileWithEncoding "{{version}}" version System.Text.Encoding.UTF8
    buildDir + @"chocolatey\tools\chocolateyInstall.ps1" |> FileHelper.RegexReplaceInFileWithEncoding "{{tag}}" tag System.Text.Encoding.UTF8
    buildDir + "chocolatey\NVika.nuspec" |> Choco.Pack (fun p -> { p with Version = version })
    "nvika." + version + ".nupkg" |> FileHelper.MoveFile artifactsDir
)

Target "All" DoNothing

// Dependencies
"Clean"
  ==> "RestorePackages"
  ==> "BuildApp"
  =?> ("InspectCodeAnalysis", Choco.IsAvailable)
  ==> "BuildReleaseNotes"
  ==> "BuildTest"
  ==> "Test"
  ==> "Zip"
  ==> "PackFakeHelper"
  =?> ("ChocoPack", Choco.IsAvailable)
  ==> "All"

// start build
RunTargetOrDefault "All"
