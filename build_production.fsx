#load "setup.fsx"
open Common

// Build for production
printfn "Building for production..."
runCommand "npm" "run build"
printfn "Output is in the folder 'dist'."

