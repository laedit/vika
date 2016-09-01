#r "./tools/FAKE/tools/FakeLib.dll"
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
let testDir = "./test/"
let artifactsDir = "./artifacts/"

// version info
let version = if isLocalBuild then "0.0.1" else if buildServer = AppVeyor then environVar "GitVersion_NuGetVersionV2" else buildVersion
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
        [
            artifactsDir + "inspectcodereport.xml";
            buildDir + "static-analysis.sarif.json"
        ] 
        |> NVika.ParseReports (fun p -> { p with Debug = true; IncludeSource = true; ToolPath = buildDir @@ "NVika.exe" })
            
    else traceError "Execution of inspectcode have failed, NVika can't be executed."
)

Target "BuildReleaseNotes" (fun _ ->
     SemanticReleaseNotesParser.Convert (fun p -> { p with 
                                                        GroupBy = SemanticReleaseNotesParser.GroupByType.Categories
                                                        Debug = true
                                                        OutputPath = buildDir @@ "ReleaseNotes.html"
                                                        PluralizeCategoriesTitle = true
                                                        IncludeStyle = SemanticReleaseNotesParser.IncludeStyleType.Yes
        } )

     buildDir @@ "ReleaseNotes.html" |> FileHelper.CopyFile artifactsDir

     SemanticReleaseNotesParser.Convert (fun p -> { p with 
                                                        GroupBy = SemanticReleaseNotesParser.GroupByType.Categories
                                                        OutputType = SemanticReleaseNotesParser.OutputType.Environment
                                                        OutputFormat = SemanticReleaseNotesParser.OutputFormat.Markdown
                                                        Debug = true
                                                        PluralizeCategoriesTitle = true
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
    Choco.Pack (fun p -> 
        { p with 
            PackageId = "nvika"
            Version = version
            Title = "NVika"
            Authors = ["laedit"]
            Owners = ["laedit"]
            ProjectUrl = "https://github.com/laedit/vika"
            IconUrl = "https://cdn.rawgit.com/laedit/vika/master/icon.png"
            LicenseUrl = "https://github.com/laedit/vika/blob/master/LICENSE"
            BugTrackerUrl = "https://github.com/laedit/vika/issues"
            Description = "Parse analysis reports (InspectCode, ...) and send messages to build server or console."
            Tags = ["report"; "parsing"; "build"; "server"; "inspectcode"]
            ReleaseNotes = "https://github.com/laedit/vika/releases"
            PackageDownloadUrl = "https://github.com/laedit/vika/releases/download/" + tag + "/NVika." + version + ".zip"
            OutputDir = artifactsDir
            Checksum = Checksum.CalculateFileHash (artifactsDir + "NVika." + version + ".zip")
            ChecksumType = Choco.ChocolateyChecksumType.Sha256
        })
)

Target "All" DoNothing

// Dependencies
"Clean" ==> "ChocoPack"

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
