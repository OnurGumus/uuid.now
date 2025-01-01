#load "./common.fsx"
open Common

restoreDotnetTools()
restorePaketPackages()
buildDotnetProject()
navigateToClientFolder()
installNpmDependencies()
