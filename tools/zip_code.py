from pathlib import Path
import shutil
import argparse


# Currently Assumes Cleaned repository folder.

def copy(svn_root, contents):
    contents.mkdir()

    path_src = svn_root / Path("src").resolve()
    print(path_src)

    shutil.copytree(str(path_src), 
                    str(contents / Path("src"))) # we want the whole src folder
    
    # we want everything but the test-data.
    folders_in_test = (svn_root / Path("test")).glob("*")

    for p_folder in folders_in_test:
        target_path = (contents / Path("test") / Path(p_folder.name))
        target_path.mkdir(parents=True, exist_ok=True)

        for p_relevant_file in list(p for p in p_folder.glob("*") if p.name != "test-data"):
            target = target_path / Path(p_relevant_file.name)
            if p_relevant_file.is_dir():
                shutil.copytree(str(p_relevant_file), str(target))
            else:
                shutil.copyfile(str(p_relevant_file), str(target))


def compose_argument_parser() -> argparse.ArgumentParser:
    """
    Compose the parser to be used in the main function of this script

    :returns: An argument parser with the correct settings.
    """
    parser = argparse.ArgumentParser()
    parser.add_argument("root_path", 
                        help="Path svn repository.",
                        type=str)

    return parser


if __name__ == "__main__":
    # Parse the arguments
    parser = compose_argument_parser()
    args = parser.parse_args()

    # Collect all relevant files
    svn_src_path = Path(args.root_path)
    build_path = svn_src_path / Path("zip_contents")

    copy(svn_src_path, build_path)

    #shutil.make_archive(str(build_path), 'zip', str(svn_src_path)) need to debug this