open System.Diagnostics
open System.IO

let runCommand (command: string) (args: string) =
    let startInfo = ProcessStartInfo(command, args)
    startInfo.UseShellExecute <- false
    startInfo.RedirectStandardOutput <- true
    startInfo.RedirectStandardError <- true

    use proc = Process.Start(startInfo)
    proc.WaitForExit()

    if proc.ExitCode <> 0 then
        let error = proc.StandardError.ReadToEnd()
        failwithf "Command '%s %s' failed with exit code %d. Error: %s" command args proc.ExitCode error
    else
        proc.StandardOutput.ReadToEnd() |> printfn "%s"

// Shared tasks
let restoreDotnetTools () =
    printfn "Restoring .NET tools..."
    runCommand "dotnet" "tool restore"

let restorePaketPackages () =
    printfn "Restoring Paket packages..."
    runCommand "dotnet" "paket restore"

let buildDotnetProject () =
    printfn "Building the .NET project..."
    runCommand "dotnet" "build"

let navigateToClientFolder () =
    printfn "Navigating to client folder..."
    Directory.SetCurrentDirectory("src/Client")

let installNpmDependencies () =
    printfn "Installing NPM dependencies..."
    runCommand "npm" "install"
