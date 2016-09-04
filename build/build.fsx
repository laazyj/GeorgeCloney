#r @"packages\FAKE\tools\FakeLib.dll"
open Fake
open System

(* Properties *)
let projectName = getBuildParamOrDefault "project" ""
let versionNumber = getBuildParamOrDefault "version" "1.0.0.0"

(* Directories *)
let buildDir = __SOURCE_DIRECTORY__
let projectDir = buildDir @@ ".."
let sourceDir = projectDir @@ "src"
let sourcePackagesDir = sourceDir @@ "packages"
let buildPackagesDir = buildDir @@ "packages"
let buildOutputDir = buildDir @@ "output"
let artifactsDir = projectDir @@ "artifacts"
let testOutputDir = artifactsDir @@ "tests"
let nugetOutputDir = artifactsDir @@ "nuget"
let nUnitRunnerDir = buildPackagesDir @@ "NUnit.Runners.Net4" @@ "tools"

(* Files *)


(* Debug *)
printf "\n##########################################################\n"
printf "# Build Script (build.fsx)\n"
printf "#   Project Name:      %s\n" projectName
printf "#   Version Number:    %s\n" versionNumber
printf "#   Project Dir:       %s\n" projectDir
printf "#   Source Dir:        %s\n" sourceDir
printf "#   Source Package Dir %s\n" sourcePackagesDir
printf "#   Build Dir:         %s\n" buildDir
printf "#   Build Package Dir: %s\n" buildPackagesDir
printf "#   Build Temp Dir:    %s\n" buildOutputDir
printf "#   Artifacts Dir:     %s\n" artifactsDir
printf "#   Test Output Dir:   %s\n" testOutputDir
printf "#   NuGet Output Dir:  %s\n" nugetOutputDir
printf "#   NUnit Runner Dir:  %s\n" nUnitRunnerDir
printf "#\n\n"

(* Targets *)
Target "Clean" (fun _ ->
    tracefn " --- Cleaning artifacts & build directories --- "
    CleanDirs [buildOutputDir; artifactsDir]
    CleanDirs [testOutputDir; nugetOutputDir]
)

Target "RestorePackages" (fun _ -> 
    tracefn " --- Restoring NuGet packages for solution --- "
    (sourceDir @@ projectName + ".sln")
    |> RestoreMSSolutionPackages (fun p ->
        {p with
            OutputPath = sourcePackagesDir
            Retries = 4
        }
    )
)

Target "Build" (fun _ ->
    tracefn " --- Building solution --- "
    !! (sourceDir @@ projectName + ".sln")
        |> MSBuildRelease buildOutputDir "clean,build"
        |> Log "Build-Output: "
)

Target "Test" (fun _ ->
    !! (buildOutputDir @@ projectName + "*.Test*.dll")
        |> NUnit (fun p ->
            {p with
                ToolPath = nUnitRunnerDir
                DisableShadowCopy = true
                OutputFile = testOutputDir @@ "TestResult.xml"
            }
        )
)

Target "CreatePackage" (fun _ ->
    tracefn " --- Creating NuGet package --- "
    NuGet (fun p ->
        {p with
            OutputPath = nugetOutputDir
            WorkingDir = buildOutputDir
            Version = versionNumber
            AccessKey = "none"
            Publish = false
        })
        (buildDir @@ projectName + ".nuspec")
)

Target "PostClean" (fun _ ->
    tracefn " --- Cleaning build directories --- "
    DeleteDir buildOutputDir
)

Target "Default" DoNothing

(* Build Order *)
"Clean"
    ==> "RestorePackages"
    ==> "Build"
    ==> "Test"
    ==> "CreatePackage"
    ==> "Default"

(* Entry Point *)
RunTargetOrDefault "Default"