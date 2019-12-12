"""
Set the DIMR version build parameter, as part of the DIMR nuget package build 
process. Should be run as a first step of the nuget process.
"""
__author__ = "Maarten Tegelaers"
__copyright__ = "Copyright (C) Stichting Deltares, 2019"
__version__ = "0.1.0"
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

    regex_str = r"DIMRset \d+\.\d+\.\d+"
    version_raw = re.findall(regex_str, log_msg)[0]
    version = version_raw.replace("DIMRset ", "")

    if verbose:
        print("    Found dimr version: {}".format(version))

    return version


def get_relevant_log_msg(file_path: Path, rev_number: int, verbose: bool) -> str:
    """
    Get the log message corresponding with the provided revision number.

    Parameters:
        file_path (Path): The path of which to retrieve the svn log.
        rev_number (int): Revision numer of the commit for which the log message
                          should be retrieved.
        verbose (bool): Whether to display additional information.

    Returns:
        (str) The log message corresponding with the provided revision number.
    """
    if verbose:
        print("  Get the relevant log message:")

    svn_cmd = "svn log {} -r {} --xml".format(str(file_path), 
                                                  rev_number)
    
    if verbose:
        print("    Running command: {}".format(svn_cmd))

    p = subprocess.run(svn_cmd, shell=True, capture_output=True, text=True)

    if verbose:
        print("    Result:\n{}".format(p.stdout))


    xml_log_msg = ET.fromstring(p.stdout)
    msg_content = xml_log_msg[0].find("msg").text

    if verbose:
        print("    message content:\n{}".format(msg_content))

    return msg_content


def run(rev_number: int,
        working_directory: Path,
        build_param: str,
        verbose=False) -> None:
    """
    Execute this script.

    Parameters:
        rev_number (int): Revision numer of the commit for which this script is run.
        working_directory (Path): The working directory of this script.
        build_param (str): The build parameter name which should be updated.
        verbose (bool): Whether to display additional information.
    """
    if verbose:
        print("Setting the '{}' build parameter".format(build_param))

    log_msg = get_relevant_log_msg(working_directory, rev_number, verbose)
    version_number = extract_version_number_from_svn_log_msg(log_msg, verbose)

    set_build_parameter(build_param, version_number)


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

    return parser.parse_args()


if __name__ == "__main__":
    args = parse_arguments()
    run(int(args.revision_number),
        Path(args.working_directory),
        args.build_param,
        args.verbose)
