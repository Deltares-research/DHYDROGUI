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
__version__ = "0.1.0"
__maintainer__ = "Maarten Tegelaers, Prisca van der Sluis"
__email__ = "Maarten.Tegelaers@deltares.nl"
__status__ = "Development"


#import requests
import argparse
import json
from pathlib import Path
import shutil
import subprocess
from win32api import GetFileVersionInfo, LOWORD, HIWORD
import re


# Confluence
# ----------
# TODO: Add support for adding the release row through the REST API instaed of
# #     manually.
RELEASE_PAGE_URL = r"https://publicwiki.deltares.nl/display/TOOLS/Release+Overview"

def print_response(resp):
    print(json.dumps(resp, indent=4, sort_keys=True))


def get_overview_page_content():
    #TODO
    pass


# SVN
# ---
# Package Information

framework  = "framework"
dimr       = "dimr"
rgfgrid    = "rgfgrid"
dido       = "dido"
plct       = "plct"
substances = "substances"


ORDERED_PKGS = [ framework
               , dimr
               , rgfgrid
               , dido
               , plct
               , substances
               ]


# Mapping to determine the version numbers from the package folder
PKG_NAME_MAPPING = { framework : "DeltaShell.Framework"
                   , dido      : "DIDO"
                   , dimr      : "Dimr.Libs"
                   , plct      : "PLCT.Libs"
                   , rgfgrid   : "RGFGRID"
                   , substances: "Substances.Libs"
                   }


def obtain_version_numbers(svn_repo_path: Path):
    """
    Obtain the version numbers from the packages.config files from the specified svn_repo_path.

    :param svn_root_path: The root path of the svn directory
    
    :returns: The package version numbers (as specified by PKG_NAME_MAPPING)
    """

    src_folder = svn_repo_path / Path("src")

    pkg_version_numbers = {}

    search_pattern = 'version=\"(.*)\" targetFramework'

    for folder in PKG_FOLDER_MAPPING:

        package_config_file_path = src_folder / Path(folder) / Path("packages.config")

        with package_config_file_path.open() as file:
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


def get_revision_numbers(file_path: Path) -> list:
    """
    Get all revision numbers of the specified file.

    :param file_path: The path to the file of which the revision should be returned.

    :returns: All revisions in which changes were made to the specified file.
    """
    # https://superuser.com/questions/160054/svn-getting-all-revisions-of-a-file
    # Executing the following command to obtain all rev numbers for the specified file_path
    # Assumption is no spaces in file paths, if you are using file paths, shame on you.
    #p_cmd = "svn log {} | grep ^r[0-9] | cut -c 2- | cut -d ' ' -f 1".format(file_path)
    p_cmd = "svn log {} -q".format(file_path)
    p = subprocess.run(p_cmd, shell=True, capture_output=True, text=True)
    
    # Select the revisions, these are always on the even line within
    revisions = list(x.split(' | ', 1)[0][1:] for x in p.stdout.splitlines()[1::2])
    return revisions


def get_revision_log_diff(file_path: Path, rev):
    """
    Get the revision log associated withe the specified rev for the specified file at file_path

    :param file_path: Path to the file of which the revision log diff should be retrieved
    :param rev: the specific revision of which the revision log diff should be retrieved

    :returns: The revision log diff of the specified file_path and rev.
    """
    p_cmd = "svn log {} -r {} --diff".format(file_path, rev)
    p = subprocess.run(p_cmd, shell=True, capture_output=True, text=True)

    return p.stdout.splitlines()


PKG_ADDITION = '+  <package id="'

def verify_rev(rev, expected_pkgs):
    """
    Given the specified revision log diff, verify if the specified expected_pkgs
    are set in it.

    :param rev: The revision log diff to be checked
    :param expected_pkgs: The packages to be checked to exist in rev

    :returns: A dictionary mapping the expected_pkgs to whether they are contained
              in the specified rev.
    """
    expected_pkgs = list(expected_pkgs)
    result = {}
    for pkg in expected_pkgs:
        result[pkg] = False

    for line in rev:
        if not line.startswith(PKG_ADDITION):
            continue

        line_relevant_part = line[(len(PKG_ADDITION)):]


        for i in range(len(expected_pkgs)):
            if not line_relevant_part.startswith(PKG_NAME_MAPPING[pkg]):
                continue

            result[pkg] = True
            del expected_pkgs[i]
            break
        
        if not expected_pkgs:
            break

    return result


PKG_FOLDER_MAPPING = { "DeltaShell.Dimr" : [ framework, dimr ]
                     , "DeltaShell.Plugins.FMSuite.Common.Gui" : [ rgfgrid ]
                     , "DeltaShell.Plugins.DelftModels.WaterQualityModel" : [ dido , plct, substances ]
                     }
                   

def obtain_svn_commit_number(svn_root_path):
    """
    Obtain all svn_commit_numbers for the packages

    :param svn_root_path: The root path of the svn directory
    
    :returns: A dictionary mapping packages to the revision they were committed
    """
    src_folder = svn_root_path / Path("src")
    
    result = {}

    for p in PKG_FOLDER_MAPPING:

        package_path = src_folder / Path(p) / Path("packages.config")

        revisions = get_revision_numbers(package_path)

        relevant_pkgs = list(PKG_FOLDER_MAPPING[p])

        for pkg in relevant_pkgs:
            result[pkg] = ""

        while relevant_pkgs and revisions:
            rev_log = get_revision_log_diff(package_path, revisions[0])
            has_pkgs = verify_rev(rev_log, relevant_pkgs)

            next_pkgs = list(relevant_pkgs)

            for i in range(len(relevant_pkgs)):
                if not (has_pkgs[relevant_pkgs[i]]):
                    continue

                result[relevant_pkgs[i]] = revisions[0]
                del next_pkgs[i]

            relevant_pkgs = next_pkgs
            revisions = revisions[1:] # drop head

    return result


# A single file to act as representation for the whole package is selected to obtain the version from
PKG_TO_DLL_MAPPING = { framework : "DeltaShell.Gui.exe"
                     , dido      : "dido.dll"
                     # dimr : is ignored, cause we will upload the content txt
                     , plct      : "plct.exe"
                     , rgfgrid   : "rgfgrid.dll"
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


def obtain_dll_version_numbers(svn_root_path):
    """
    Gather the file versions of the files specified in PKG_TO_DLL_MAPPING
    from within the "packages" folder.

    :param svn_root_path: The root path of the svn directory

    :returns: A dictionary mapping packages to their corresponding versions
    """
    pkg_folder = svn_root_path / Path("packages")
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
                           , additional_remarks : str
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

    cells.append(HTML_CELL.format(additional_remarks))
    cells.append(HTML_CELL.format("        <br/>"))     # Test report cell

    return """    <tr>
{}
    </tr>""".format("\n".join(cells))


def generate_simple_table( ordered_pkgs        : list
                         , pkg_versions        : dict
                         , svn_dll_versions    : dict
                         , svn_commit_versions : dict
                         ) -> str:
    """
    Generate a simple table from the given parameters in
    a human readable form.

    :param ordered_pkgs: An ordered list of packages that should be put
                         in the table row.
    :param pkg_versions: A mapping of pkg to their corresponding version.
    :param svn_versions: A mapping of pkg to their corresponding svn commit 
                         rev

    :returns: A string describing a simple table with the information
              specified.
    """
    column_widths = []

    for pkg in ordered_pkgs:
        col_elems = [len(PKG_NAME_MAPPING[pkg])]

        if pkg in pkg_versions:
            col_elems.append(len(pkg_versions[pkg]))
        if pkg in svn_commit_versions:
            col_elems.append(len(svn_commit_versions[pkg]))
        if pkg in svn_dll_versions:
            col_elems.append(len(svn_dll_versions[pkg]))
        
        column_widths.append(max(col_elems))

    pkg_version = "pkg version"
    svn_dll     = "svn dll version"
    svn_commit  = "svn commit version"

    first_column_len = len(svn_commit)

    # Name Row
    def get_formatted_elem(dict_in, i):
        if ordered_pkgs[i] in dict_in:
            elem = dict_in[ordered_pkgs[i]]
        else:
            elem = ""

        return elem + (" " * (column_widths[i] - len(elem)))

    row_1 = ("{} | ".format(" " * first_column_len) +
             " | ".join(get_formatted_elem(PKG_NAME_MAPPING, i) for i in range(len(ordered_pkgs))) +
             " |")
    row_2 = ("{}-+-".format("-" * first_column_len) +
             "-+-".join("-" * column_widths[i] for i in range(len(ordered_pkgs))) +
             "-|")
    row_3 = ("{} | ".format(pkg_version + (" " * (first_column_len - len(pkg_version)))) +
            (" | ".join(get_formatted_elem(pkg_versions, i) for i in range(len(ordered_pkgs)))) +
             " |")

    row_4 = ("{} | ".format(svn_dll + (" " * (first_column_len - len(svn_dll)))) +
            (" | ".join(get_formatted_elem(svn_dll_versions, i) for i in range(len(ordered_pkgs)))) +
             " |")

    row_5 = ("{} | ".format(svn_commit + (" " * (first_column_len - len(svn_commit)))) +
            (" | ".join(get_formatted_elem(svn_commit_versions, i) for i in range(len(ordered_pkgs)))) +
             " |")
    return "\n".join([row_1, row_2, row_3, row_4, row_5])


def parse_arguments():
    """
    Parse the arguments with which this script was called through
    argparse.ArgumentParser
    """
    parser = argparse.ArgumentParser()

    parser.add_argument("svn_root_path", help="Path to the root of the SVN directory.")
    parser.add_argument("--distributions", nargs="*", default=[""], help="The distributions to be added to the table.")
    parser.add_argument("--additional_remarks", default="", help="Additional remarks that will be added to the table.")
    parser.add_argument("--output_html", action="store_true", help="Output a html table row, otherwise human readable output will be generated.")

    return parser.parse_args()


def run() -> None:
    """
    Obtain all relevant package versions and their version in which they
    were commit, given the configuration passed through this script.
    """
    args = parse_arguments()
    svn_root_path = Path(args.svn_root_path)

    pkg_version_numbers = obtain_version_numbers(svn_root_path)
    svn_pkg_version_numbers = obtain_dll_version_numbers(svn_root_path)

    if args.output_html:
        print(generate_html_table_row( args.distributions
                                     , ORDERED_PKGS
                                     , pkg_version_numbers
                                     , svn_pkg_version_numbers
                                     , args.additional_remarks))
    else:
        svn_commit_version_numbers = obtain_svn_commit_number(svn_root_path)

        print(generate_simple_table( ORDERED_PKGS
                                   , pkg_version_numbers
                                   , svn_pkg_version_numbers
                                   , svn_commit_version_numbers
                                   ))


if __name__ == "__main__":
    run()
 