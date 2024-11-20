from pathlib import Path
import shutil
import argparse
import zipfile
import datetime
from itertools import chain


# Currently Assumes Cleaned repository folder.
def get_relevant_files(svn_root: Path):
    """
    Get all files from the src and test folders except for the test-data 
    folders and contents.

    :param svn_root: The root path of the svn.
    """
    src_path = svn_root / Path("src")
    test_path = svn_root / Path("test")

    return chain(src_path.glob("**/*"), 
                 (p for p in test_path.glob("**/*") if not "test-data" in p.parts))


def write_zipfile(content_paths, zip_path: Path) -> None:
    """
    Write the specified content paths to the specified zip.

    :param content_paths: The collection of paths which need to be written to
                         the zipfile.
    :param zip_path: The path to zip file that will be written.
    """
    with zipfile.ZipFile(str(zip_path), 'w') as w_zip:
        for p in content_paths:
            w_zip.write(str(p))


def compose_argument_parser() -> argparse.ArgumentParser:
    """
    Compose the parser to be used in the main function of this script

    :returns: An argument parser with the correct settings.
    """
    parser = argparse.ArgumentParser()
    parser.add_argument("root_path", help="Path svn repository.", type=str)
    parser.add_argument("zip_name", help="zip name, will be changed to '<zip_name>_YYYYMMDD.zip'.")

    return parser


if __name__ == "__main__":
    parser = compose_argument_parser()
    args = parser.parse_args()

    svn_src_path = Path(args.root_path)
    build_path = svn_src_path / Path("zip_contents")

    today = datetime.date.today()
    zip_file_name = "{}_{}.zip".format(args.zip_name, today.strftime("%Y%m%d"))

    write_zipfile(get_relevant_files(svn_src_path), svn_src_path / Path(zip_file_name))
