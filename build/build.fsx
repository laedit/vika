#r "../tools/FAKE/tools/FakeLib.dll"
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
let buildResultDir = "./build_result/"
let testDir = "./test/"
let artifactsDir = "./artifacts/"

// version info
let version = if isLocalBuild then "0.0.1" else if buildServer = AppVeyor then environVar "GitVersion_NuGetVersionV2" else buildVersion
let tag = if buildServer = AppVeyor then AppVeyor.AppVeyorEnvironment.RepoTagName else "v0.0.1"

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildResultDir; testDir; artifactsDir]
)

Target "RestorePackages" (fun _ ->
    "./src/Vika.sln"
    |> RestoreMSSolutionPackages (fun p -> { p with OutputPath = "src/packages" })
)

Target "BuildApp" (fun _ ->
    !! "src/NVika/NVika.csproj"
      |> MSBuildRelease buildResultDir "Build"
      |> Log "AppBuild-Output: "
)

Target "InspectCodeAnalysis" (fun _ ->
    "resharper-clt.portable" |> Choco.Install id
    
    directExec(fun info ->
        info.FileName <- "inspectcode"
        info.Arguments <- "/o=\"" + artifactsDir + "inspectcodereport.xml\" /project=\"NVika\" \"src\Vika.sln\"" ) |> ignore
)

Target "GendarmeAnalysis" (fun _ ->
    "mono.gendarme" |> NugetInstall (fun p -> 
    { p with 
        OutputDirectory = "tools";
        ExcludeVersion = true
    })
    
    directExec(fun info ->
        info.FileName <- System.IO.Path.GetFullPath "./tools/Mono.Gendarme/tools/gendarme.exe"
        info.Arguments <- "--xml " + buildResultDir + "GendarmeReport.xml " + "--ignore " + buildDir + "gendarme.ignore " + buildResultDir + "NVika.exe" ) |> ignore
)

Target "LaunchNVika" (fun _ ->
    let reportsPath = 
        [
            artifactsDir + "inspectcodereport.xml";
            buildResultDir + "static-analysis.sarif.json";
            buildResultDir + "NVika.exe.CodeAnalysisLog.xml";
            buildResultDir + "GendarmeReport.xml";
        ]
    let existingReportsPath = reportsPath |> Seq.filter fileExists

    if Seq.isEmpty existingReportsPath then
        traceImportant "No analytics reports to parse :'("
    else
        existingReportsPath |> NVika.ParseReports (fun p -> { p with Debug = true; IncludeSource = true; ToolPath = buildResultDir @@ "NVika.exe" })
)

Target "BuildReleaseNotes" (fun _ ->
     SemanticReleaseNotesParser.Convert (fun p -> { p with 
                                                        GroupBy = SemanticReleaseNotesParser.GroupByType.Categories
                                                        Debug = true
                                                        OutputPath = buildResultDir @@ "ReleaseNotes.html"
                                                        PluralizeCategoriesTitle = true
                                                        IncludeStyle = SemanticReleaseNotesParser.IncludeStyleType.Yes
        } )

     buildResultDir @@ "ReleaseNotes.html" |> FileHelper.CopyFile artifactsDir

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
    !! (buildResultDir + "/*.dll")
    ++ (buildResultDir + "NVika.exe")
    ++ (buildResultDir + "NVika.exe.config")
    ++ (buildResultDir + "ReleaseNotes.html")
    |> Zip buildResultDir (artifactsDir + "NVika." + version + ".zip")
)

Target "PackFakeHelper" (fun _ ->
    buildDir @@ "NVikaHelper.fsx" |> FileHelper.CopyFile artifactsDir
    artifactsDir @@ "NVikaHelper.fsx" |> FileHelper.RegexReplaceInFileWithEncoding "../tools/FAKE/tools/FakeLib.dll" "FakeLib.dll" System.Text.Encoding.UTF8
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
            Tags = ["report"; "parsing"; "build"; "server"; "inspectcode"; "FxCop"; "SARIF"; "Roslyn"; "Gendarme"]
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
  ==> "GendarmeAnalysis"
  ==> "LaunchNVika"
  ==> "BuildReleaseNotes"
  ==> "BuildTest"
  ==> "Test"
  ==> "Zip"
  ==> "PackFakeHelper"
  =?> ("ChocoPack", Choco.IsAvailable)
  ==> "All"

// start build
RunTargetOrDefault "All"
