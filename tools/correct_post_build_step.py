from pathlib import Path
import re
import argparse

def get_args(): 
    parser = argparse.ArgumentParser()
    parser.add_argument("svn_root_path", help="Path to the root of the SVN directory")
    parser.add_argument("version_number", help="Version number of the Framework NuGet package")
    return parser.parse_args()


if __name__ == "__main__":
    args = get_args()
    root_path = Path(args.svn_root_path)
    version_number = args.version_number
    
    if root_path.exists() and root_path.is_dir():
        project_path = root_path / Path("test/DeltaShell.Plugins.DelftModels.HydroModel.Tests/DeltaShell.Plugins.DelftModels.HydroModel.Tests.csproj")
        
        with project_path.open() as f:
            lines = f.readlines()
                    
        for i in range(len(lines)):
            lines[i] = re.sub(r'DeltaShell\.TestProject\.1\.4\.0\.\d{5}',"DeltaShell.TestProject.1.4.0." + version_number, lines[i])

        with project_path.open(mode='w') as f:
            f.writelines(lines)