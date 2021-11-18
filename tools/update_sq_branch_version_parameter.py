__author__ = "Maarten Tegelaers"
__copyright__ = "Copyright (C) Stichting Deltares, 2020"
__version__ = "0.2.0"
__maintainer__ = "Maarten Tegelaers, Prisca van der Sluis"
__email__ = "Maarten.Tegelaers@deltares.nl"
__status__ = "Development"

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
    set_parameter_msg = "setParameter name='{}' value='{}'".format(name, value)
    service_msg = SERVICE_MSG_TEMPLATE.format(set_parameter_msg)
    print(service_msg)


RELEASE_BRANCH_PREFIX = "release/D-HYDRO1D2D-"


def is_release_branch(branch_name: str) -> bool:
    return branch_name.startswith(RELEASE_BRANCH_PREFIX)


def get_version(branch_name: str, git_hash: str) -> str:
    if is_release_branch(branch_name):
        release_version = branch_name.replace(RELEASE_BRANCH_PREFIX, "")
        return f"release_{release_version}.{git_hash}"
    
    return f"development.{git_hash}"


def parse_arguments():
    """
    Parse the arguments with which this script was called through
    argparse.ArgumentParser
    """
    parser = argparse.ArgumentParser()

    parser.add_argument("branch_name", help="The branch name currently being analyzed")
    parser.add_argument("git_hash_short", help="The short git hash used to set the version name.")
    parser.add_argument("--variable_name", default="D-HYDRO_ReleaseVersion",  help="The prefix variable name to write to.")
    return parser.parse_args()


if __name__ == "__main__":
    args = parse_arguments()

    version = get_version(args.branch_name, args.git_hash_short)
    set_build_parameter(args.variable_name, version)
