#!/usr/bin/python3
"""
Get the version numbering (both NuGet package and actual version)
of the different dependencies used by D-Hydro. These values can
either be mapped to a human readable table, or to html code to
be pasted directly into the source editor of 
https://publicwiki.deltares.nl/display/TOOLS/Release+Overview
"""
__author__ = "Maarten Tegelaers"
__copyright__ = "Copyright (C) Stichting Deltares, 2019"
__version__ = "0.2.0"
__maintainer__ = "Maarten Tegelaers, Prisca van der Sluis"
__email__ = "Maarten.Tegelaers@deltares.nl"
__status__ = "Development"


import argparse
from pathlib import Path
from win32api import GetFileVersionInfo, LOWORD, HIWORD
import re


# Package Information
framework  = "framework"
dimr       = "dimr"
rgfgrid    = "rgfgrid"
dido       = "dido"
plct       = "plct"
substances = "substances"


ORDERED_PKGS = [ 
    framework, 
    dimr, 
    rgfgrid,
    dido,
    plct,
    substances
]


# Mapping to determine the version numbers from the package folder
PKG_NAME_MAPPING = { 
    framework : "DeltaShell.ApplicationPlugin", 
    dido      : "DIDO", 
    dimr      : "Dimr.Libs", 
    plct      : "PLCT.Libs", 
    rgfgrid   : "RGFGRID", 
    substances: "Substances.Libs"
}


def obtain_version_numbers(repo_path: Path):
    """
    Obtain the version numbers from the packages.config files from the specified repo_path.

    :param root_path: The root path of the repository
    
    :returns: The package version numbers (as specified by PKG_NAME_MAPPING)
    """

    src_folder = repo_path / "src"

    pkg_version_numbers = {}

    search_pattern = 'Version=\"((\\d+\\.?)+)\"'

    for folder in PKG_FOLDER_MAPPING:
        csproj_file_path = src_folder / folder / f"{folder}.csproj"

        with csproj_file_path.open() as file:
            lines = file.readlines()

        packages = list(PKG_FOLDER_MAPPING[folder])
        for line in lines:
            for package in packages:
                if PKG_NAME_MAPPING[package] in line:
                    search_result = re.search(search_pattern, line)
                    
                    if search_result:
                        version_number = search_result.group(1)
                        pkg_version_numbers[package] = version_number
                        packages.remove(package)
                        break
                        
            if not packages:
                break

    return pkg_version_numbers


PKG_FOLDER_MAPPING = { 
    "DeltaShell.Dimr" : [ framework, dimr ], 
    "DeltaShell.Plugins.FMSuite.Common.Gui" : [ rgfgrid ], 
    "DeltaShell.Plugins.DelftModels.WaterQualityModel" : [ dido , plct, substances ]
}


# A single file to act as representation for the whole package is selected to obtain the version from
PKG_TO_DLL_MAPPING = { 
    framework : "DeltaShell.Gui.exe", 
    dido      : "dido.dll",
    # dimr : is ignored, cause we will upload the content txt
    plct      : "plct.exe", 
    rgfgrid   : "rgfgrid.dll"
    # substances: is ignored cause we do not have 
}


def get_file_version_number (file_path):
    """
    Get the file version number of the file at the specified path.
    This uses the win32 api.

    :param file_path: The path of which the file version should be obtained.

    :returns: A string describing the file version of the specified path.
    """
    info = GetFileVersionInfo(str(file_path), "\\")

    ms = info['FileVersionMS']
    ls = info['FileVersionLS']

    return ".".join(str(val) for val in [HIWORD (ms), LOWORD (ms), HIWORD (ls), LOWORD (ls)])


def obtain_dll_version_numbers(root_path):
    """
    Gather the file versions of the files specified in PKG_TO_DLL_MAPPING
    from within the "packages" folder.

    :param root_path: The root path of the repository

    :returns: A dictionary mapping packages to their corresponding versions
    """
    pkg_folder = root_path / Path("packages")
    result = {}

    # get file version
    for pkg in PKG_TO_DLL_MAPPING:
        path = next(pkg_folder.glob("**/{}".format(PKG_TO_DLL_MAPPING[pkg])), None)

        if not path:
            continue

        result[pkg] = get_file_version_number(path)

    return result


# HTML strings
HTML_CELL = """      <td colspan="1">
{}
      </td>"""



def get_package_cell_html(pkg_version="", svn_version="") -> str:
    """
    Generate the html table cell for a package with the given pkg_version and
    svn_version.

    :param pkg_version: The package version

    :returns: A string containing the html code describing a table cell with 
              the given pkg_version and svn_version.
    """
    if not pkg_version and not svn_version:
        content = "<br />"
    else:
        content = "        <p>{}</p>".format(pkg_version)        

        if svn_version:
            content += "\n        <p>SVN rev: {}</p>".format(svn_version)
        
    return HTML_CELL.format(content)


def get_distribution_cell(distributions: list):
    """
    Generate the html table cell of the distributions given the distributions
    as a unordered list.

    :param distributions: The list of distributions to put in the cell.

    :returns: A string containing the html code describing a table cell with
              the distributions information.
    """
    distribution_content_elems = "\n".join("          <li>{}</li>".format(elem) for elem in distributions)
    distribution_content = """        <ul>
{}
        </ul>""".format(distribution_content_elems)

    return HTML_CELL.format(distribution_content)



def generate_html_table_row( distributions : list
                           , ordered_pkgs  : list
                           , pkg_versions  : dict
                           , svn_versions  : dict
                           ) -> str:
    """
    Generate a HTML table row from the given parameters. This can be
    directly pasted into confluence source editor.

    The following table is constructed currently

    | <empty> | <distributions> | <ordered_pkgs[0]> | ... | <ordered_pkgs[n]> | <additional_remarks> | <empty> |

    :param distributions: A List of strings describing the distributions
                          of the current row.
    :param ordered_pkgs: An ordered list of packages that should be put
                         in the table row.
    :param pkg_versions: A mapping of pkg to their corresponding version.
    :param svn_versions: A mapping of pkg to their corresponding svn commit 
                         rev
    :param additional_remarks: the content of the additional remarks.

    :returns: A string containing the html code describing a row with the 
              information specified.
    """
    cells = [ HTML_CELL.format("        <br/>")         # Releases cell
            , get_distribution_cell(distributions) 
            ]

    for pkg in ordered_pkgs:
        cells.append( get_package_cell_html( pkg_versions[pkg] if pkg in pkg_versions else ""
                                           , svn_versions[pkg] if pkg in svn_versions else ""))
    return """    <tr>
{}
    </tr>""".format("\n".join(cells))


def parse_arguments():
    """
    Parse the arguments with which this script was called through
    argparse.ArgumentParser
    """
    parser = argparse.ArgumentParser()

    parser.add_argument("repo_root_path", help="Path to the root of the repository.")
    parser.add_argument("--distributions", nargs="*", default=[""], help="The distributions to be added to the table.")

    return parser.parse_args()


def run() -> None:
    """
    Obtain all relevant package versions and their version in which they
    were commit, given the configuration passed through this script.
    """
    args = parse_arguments()
    repo_root_path = Path(args.repo_root_path)

    pkg_version_numbers = obtain_version_numbers(repo_root_path)
    dll_pkg_version_numbers = obtain_dll_version_numbers(repo_root_path)

    print(generate_html_table_row( 
        args.distributions, 
        ORDERED_PKGS, 
        pkg_version_numbers, 
        dll_pkg_version_numbers, 
))


if __name__ == "__main__":
    run()