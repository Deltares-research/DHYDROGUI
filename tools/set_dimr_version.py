"""
Set the DIMR version build parameter, as part of the DIMR nuget package build 
process. Should be run as a first step of the nuget process.
"""
__author__ = "Maarten Tegelaers"
__copyright__ = "Copyright (C) Stichting Deltares, 2019"
__version__ = "0.3.0"
__maintainer__ = "Maarten Tegelaers, Prisca van der Sluis"
__email__ = "Maarten.Tegelaers@deltares.nl"
__status__ = "Development"


from pathlib import Path
import re
import subprocess
import xml.etree.ElementTree as ET
import argparse


SERVICE_MSG_TEMPLATE = "##teamcity[{0}]"


def set_build_parameter(name: str, value: str) -> None:
    """
    Set the build parameter with the specified name and value. This build 
    parameter will be available in subsequent build steps.

    Parameters:
        name (str):  The name of the build parameter.
        value (str): The value of the build parameter.

    Remarks:
        Keep in mind the prefix of the build parameter:
        * system for system properties
        * env for environment variables
        * no prefix for configuration parameters.
    """
    # See: https://www.jetbrains.com/help/teamcity/build-script-interaction-with-teamcity.html#BuildScriptInteractionwithTeamCity-AddingorChangingaBuildParameter
    set_parameter_msg = "setParameter name='{}' value='{}'".format(name, 
                                                                   value)
    service_msg = SERVICE_MSG_TEMPLATE.format(set_parameter_msg)
    print(service_msg)


def extract_version_number_from_svn_log_msg(log_msg: str, verbose: bool) -> str:
    """
    Extract the DIMR version number from the svn log message.

    Parameters:
        log_msg (str): The log message from which to obtain the DIMR version number.

    Returns:
        (str) The DIMR version number defined in the svn log message.
    """
    if verbose:
        print("  Extract version number from svn log message:")

    regex_str = r"DIMRset (\d+\.\d+\.\d+_patch_?\d+)"
    matches = re.search(regex_str, log_msg)

    if not matches:
        regex_str = r"DIMRset (\d+\.\d+\.\d+)"
        matches = re.search(regex_str, log_msg)

        if not matches:
            return None

    version = matches[1].replace("_", "-")

    if verbose:
        print(f"    Found dimr version: {version}")

    return version


def is_last_revision(file_path: Path, 
                     rev_number: int,
                     username: str,
                     password: str) -> bool:
    """
    Verify whether the provided revision number is the last revision.

    Parameters:
        file_path (Path): The path to get the revisions of.
        rev_number (int): The revision number to check.
        username (str)  : Optional username that will be passed into the svn command
        password (str)  : Optional password that will be passed into the svn command

    Returns:
        True if rev_number == last revision number
    """
    svn_cmd = f"svn log {str(file_path)} --limit 1 --xml -q"

    if username and password:
        svn_cmd += f" --username {username} --password {password}"

    p = subprocess.run(svn_cmd, shell=True, stdout=subprocess.PIPE)
    xml_log_msg = ET.fromstring(p.stdout)

    last_rev_number = int(xml_log_msg[0].attrib["revision"])

    return last_rev_number == rev_number


def find_previous_revision(file_path: Path, 
                           rev_number: int,
                           username: str,
                           password: str) -> int:
    """
    Find the revision before the specified rev_number. 

    Parameters:
        file_path (Path): The path to get the revisions of.
        rev_number (int): The revision number to check.
        username (str)  : Optional username that will be passed into the svn command
        password (str)  : Optional password that will be passed into the svn command
    
    Returns:
        (int) The revision number before the specified rev_number.

    Exceptions:
        Thrown when no previous revision could be located.
    """
    svn_cmd  = f"svn log {str(file_path)} --xml -q"

    if username and password:
        svn_cmd += f" --username {username} --password {password}"

    p = subprocess.run(svn_cmd, shell=True, stdout=subprocess.PIPE)

    xml_content = ET.fromstring(p.stdout)

    for i in range(len(xml_content) - 1):
        if int(xml_content[i].attrib["revision"]) == rev_number:
            return int(xml_content[i+1].attrib["revision"])
    
    raise Exception(f"Could not locate the revision before {rev_number}.")


def get_relevant_log_msgs(file_path: Path, 
                          rev_number: int, 
                          username: str, 
                          password: str) -> tuple:
    """
    Get log messages of the specified revision, as well as the revision
    before this revision.

    Parameters:
        file_path (Path): The path to get the revisions of.
        rev_number (int): The revision number to check.
        username (str)  : Optional username that will be passed into the svn command
        password (str)  : Optional password that will be passed into the svn command
    
    Returns:
        (tuple) A tuple containing the specified revision number's message, and
        the revision number before the specified revision number's message.

    Exceptions:
        Thrown when no previous revision could be located.
    """
    if is_last_revision(file_path, rev_number, username, password):
        svn_cmd = f"svn log {file_path} --limit 2 --xml"

    else:
        prev_rev_number = find_previous_revision(file_path, 
                                                 rev_number,
                                                 username,
                                                 password)

        svn_cmd = f"svn log {str(file_path)} -r {rev_number}:{prev_rev_number} --xml"

    if username and password:
        svn_cmd += f" --username {username} --password {password}"
    
    p = subprocess.run(svn_cmd, shell=True, stdout=subprocess.PIPE)

    xml_content = ET.fromstring(p.stdout)

    curr_msg = xml_content[0].find("msg").text
    prev_msg = xml_content[1].find("msg").text

    return (curr_msg, prev_msg)


def run(rev_number: int,
        working_directory: Path,
        build_param: str,
        username: str,
        password: str,
        verbose=False) -> None:
    """
    Execute this script.

    Parameters:
        rev_number (int): Revision numer of the commit for which this script is run.
        working_directory (Path): The working directory of this script.
        build_param (str): The build parameter name which should be updated.
        username (str)  : Optional username that will be passed into the svn command
        password (str)  : Optional password that will be passed into the svn command
        verbose (bool): Whether to display additional information.
    """
    if verbose:
        print(f"Setting the '{build_param}' build parameter")

    curr_log_msg, prev_log_msg = get_relevant_log_msgs(working_directory, rev_number, username, password)
    
    curr_version_number = extract_version_number_from_svn_log_msg(curr_log_msg, verbose)

    if not curr_version_number:
        curr_version_number = f"2.0.0.{rev_number}"

    prev_version_number = extract_version_number_from_svn_log_msg(prev_log_msg, verbose)

    if not prev_version_number:
        prev_version_number = f"2.0.0.{rev_number}"

    if curr_version_number == prev_version_number:
        raise Exception(f"The DIMR version is the same between revision {rev_number} and the revision before it, no new DIMR NuGet will be generated.")

    set_build_parameter(build_param, curr_version_number)


def parse_arguments():
    """
    Parse the arguments with which this script was called through
    argparse.ArgumentParser
    """
    parser = argparse.ArgumentParser()

    parser.add_argument("build_param", help="The build parameter to update with the DIMR version.")
    parser.add_argument("working_directory", help="The working directory.")
    parser.add_argument("revision_number", help="The revision number with which this build was triggered.")
    parser.add_argument("--verbose", action="store_true", help="Whether to print additional information or not.")
    parser.add_argument("--username", default=None, help="SVN Username")
    parser.add_argument("--password", default=None, help="SVN password")

    return parser.parse_args()


if __name__ == "__main__":
    args = parse_arguments()
    run(int(args.revision_number),
        Path(args.working_directory),
        args.build_param,
        args.username,
        args.password,
        args.verbose)
