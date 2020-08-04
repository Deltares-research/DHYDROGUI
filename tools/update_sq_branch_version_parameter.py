__author__ = "Maarten Tegelaers"
__copyright__ = "Copyright (C) Stichting Deltares, 2020"
__version__ = "0.1.0"
__maintainer__ = "Maarten Tegelaers, Prisca van der Sluis"
__email__ = "Maarten.Tegelaers@deltares.nl"
__status__ = "Development"

import argparse
import subprocess


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


def get_branch_name() -> str:
    p_cmd = "git symbolic-ref --short HEAD"
    p = subprocess.run(p_cmd, shell=True, capture_output=True, text=True)

    return p.stdout.strip()


def verify_branch(branch_name: str) -> bool:
    return branch_name.startswith("release/D-HYDRO-")


def get_version(branch_name: str) -> str:
    return branch_name[16:]


def set_version_parameter(variable_name: str, version: str):
    set_build_parameter(variable_name, version)


def report_failure(branch_name: str):
    failure_description = f"The provided branch, {branch_name}, does not match the expected release convention: release/D-HYDRO-<version>"
    failure_msg = f"buildProblem description='{failure_description}'"

    service_msg = SERVICE_MSG_TEMPLATE.format(failure_msg)
    print(service_msg)


def parse_arguments():
    """
    Parse the arguments with which this script was called through
    argparse.ArgumentParser
    """
    parser = argparse.ArgumentParser()

    parser.add_argument("--variable_name", default="D-HYDRO_ReleaseVersion",  help="The prefix variable name to write to.")
    parser.add_argument("--skip_verification", action="store_true", help="Flag to skip verification.")
    return parser.parse_args()


if __name__ == "__main__":
    args = parse_arguments()
    branch_name = get_branch_name()

    # Set the variable directly to the variable name.
    if args.skip_verification:
        set_version_parameter(args.variable_name, branch_name)
    # Verify the variable to be a release branch.
    else:
        if verify_branch(branch_name):
            version = get_version(branch_name)
            set_version_parameter(args.variable_name, version)
        else:
            report_failure(branch_name)
