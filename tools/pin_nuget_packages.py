"""
Pins and tags the third party NuGet packages on TeamCity
that are used in the solution folder for which this script is run.
Removes the tag, if found, from a previous build
and removes the pin when possible.
"""

import argparse
import xml.etree.ElementTree as et
from pathlib import Path
import requests
import json
import logging

# Production Server
TEAMCITY_URL = "https://dpcbuild.deltares.nl"
# Test Server
# TEAMCITY_URL = "http://tl-ts001.xtr.deltares.nl:8080"

BUILDS_ROOT = f"{TEAMCITY_URL}/httpAuth/app/rest/builds/"
JSON_RESPONSE_HEADER = {'Accept': 'application/json'}

# 0: nuget package id, 1: nuget package build configuration id
NUGET_PACKAGES = [
    ("Dimr.Libs", "DHydroUserInterface_DHydroExternalLibraries_KernelNuGetPackages_Dimr"),
    ("RGFGRID", "DHydroUserInterface_DHydroExternalLibraries_KernelNuGetPackages_Rgfgrid"),
    ("DIDO", "DHydroUserInterface_DHydroExternalLibraries_KernelNuGetPackages_Dido"),
    ("PLCT.Libs", "DHydroUserInterface_DHydroExternalLibraries_KernelNuGetPackages_Plct"),
    ("Substances.Libs", "DHydroUserInterface_DHydroExternalLibraries_KernelNuGetPackages_Substances"),
    ("DeltaShell.Framework", "DeltaShellFramework_DeltaShellFrameworkGit_NuGet_DeltaShellFrameworkSigned")
]


class RequestWrapper:

    def __init__(self, user: str, password: str):
        """
        user : str
            The user to authenticate with.
        password : str
            The password to authenticate with.
        """

        self.user = user
        self.password = password
        self.header = JSON_RESPONSE_HEADER

    def get(self, url: str):
        return requests.get(url,
                            auth=(self.user, self.password),
                            headers=JSON_RESPONSE_HEADER)

    def delete(self, url: str):
        requests.delete(url,
                        auth=(self.user, self.password),
                        headers=JSON_RESPONSE_HEADER)

    def put_json(self, url: str, new_json: dict):
        requests.put(url,
                     auth=(self.user, self.password),
                     headers=JSON_RESPONSE_HEADER,
                     json=new_json)

    def put(self, url: str):
        requests.put(url,
                     auth=(self.user, self.password),
                     headers=JSON_RESPONSE_HEADER)


def get_previous_build(build_config: str, tag: str, wrapper: RequestWrapper) -> dict:
    """
    Get the previous build tagged with the specified tag

    Parameters
    ----------
    build_config : str
        The build configuration id of the build to be retrieved.
    tag : str
        The tag with which the build should be retrieved.
    wrapper : RequestWrapper
        The request wrapper to make requests calls.
    """
    build_url = f"{BUILDS_ROOT}buildType:{build_config},tag:{tag},pinned:true,count:1"
    previous_build_response = wrapper.get(build_url)

    if previous_build_response.status_code != 200:
        return None

    return previous_build_response.json()


def unpin_build(build_id: str, wrapper: RequestWrapper) -> None:
    """
    Unpin the build with build_id.

    Parameters
    ----------
    build_id : str
        The id of the build to be unpinned.
    wrapper : RequestWrapper
        The request wrapper to make requests calls.
    """
    pin_url = f"{BUILDS_ROOT}id:{build_id}/pin/"
    wrapper.delete(pin_url)


def clean_up_build(build_info: dict, tag: str, wrapper: RequestWrapper) -> None:
    """
    Remove the specified tag from the specified build and unpin if necessary.

    Parameters
    ----------
    build_info : dict
        A dictionary describing the build to be modified.
    tag : str
        The tag to be removed from the build
    wrapper : RequestWrapper
        The request wrapper to make requests calls.
    """
    build_id = build_info["id"]
    untag_build(build_info, tag, wrapper)

    tag_info = build_info["tags"]
    if tag in get_tag_values(tag_info) and tag_info["count"] == 1:
        unpin_build(build_id, wrapper)


def get_tag_values(tags):
    """
    Get the tags from the specified tags dictionary.

    Parameters
    ----------
    tags : dict
        A tags dictionary.
    """
    return list(x["name"] for x in tags["tag"])


def untag_build(build_info: dict, tag: str, wrapper: RequestWrapper) -> None:
    """
    Remove the specified tag from the build specified with build_info.

    Parameters
    ----------
    build_info : dict
        A dictionary describing the build to be modified.
    tag : str
        The tag to be removed from the build
    wrapper : RequestWrapper
        The request wrapper to make requests calls.
    """
    build_tags = get_tag_values(build_info["tags"])
    if tag not in build_tags:
        return

    new_tag_values = list({"name": x} for x in build_tags if x != tag)
    new_tags = {
        'count': len(new_tag_values),
        'tag': new_tag_values
    }

    tag_url = f"{BUILDS_ROOT}id:{build_info['id']}/tags/"
    wrapper.put_json(tag_url, new_tags)


def get_packages(files) -> dict:
    """
    Returns a dictionary with the package ids and a set of the collected version numbers.

    Parameters
    ----------
    files :
        The paths to the packages files from which the version number should be read.
    """

    packages = {}

    for file in files:
        root = et.parse(file).getroot()
        for child in root:
            p_id = child.get('id')
            version = child.get('version')

            if p_id in packages:
                packages[p_id].add(version)
            else:
                packages[p_id] = {version}

    return packages


def get_packages_files(dir_path: Path):
    """
    Returns full file paths of the packages.config files in the directory.

    Parameters
    ----------
    dir_path :
        The path to the directory from which the files should be retrieved.
    """

    return dir_path.glob("**/packages.config")


def has_artifact_for_nuget_pkg(build_url: str, nuget_package_file_name: str, wrapper: RequestWrapper) -> bool:
    """
    Returns whether or not the specified build has the valid
    artifact for the nuget package

    Parameters
    ----------
    build_url : str
        The build url to check the artifacts for.
    nuget_package_file_name : str
        The expected nuget package file name within the build to be retrieved.
    wrapper : RequestWrapper
        The request wrapper to make requests calls.
    """
    build_artifacts_url = f"{build_url}artifacts/"
    artifacts_response = wrapper.get(build_artifacts_url)

    if artifacts_response.status_code != 200:
        return false

    artifact_info = artifacts_response.json()
    return artifact_info['file'][0]['name'] == nuget_package_file_name


def get_new_build(build_config_id: str, nuget_package_file_name: str, wrapper: RequestWrapper) -> dict:
    """
    Get the build from build_config with the specified nuget package file.

    Parameters
    ----------
    build_config_id : str
        The id of the build configuration of which the build is part.
    nuget_package_file_name : str
        The expected nuget package file name within the build to be retrieved.
    wrapper : RequestWrapper
        The request wrapper to make requests calls.
    """
    builds_url = f"{BUILDS_ROOT}?locator=buildType:{build_config_id}"

    new_builds_response = wrapper.get(builds_url)

    if new_builds_response.status_code != 200:
        return None

    builds = new_builds_response.json()
    for build in builds['build']:

        new_build_url = f"{BUILDS_ROOT}id:{build['id']}/"

        if not has_artifact_for_nuget_pkg(new_build_url, nuget_package_file_name, wrapper):
            continue

        new_build_info = wrapper.get(new_build_url)

        if new_build_info.status_code != 200:
            logging.warning(f"Request '{new_build_url}' returned {new_build_info.status_code}.")
            continue

        return new_build_info.json()

    return None


def pin_build(build_id: str, wrapper: RequestWrapper) -> None:
    """
    Pin the build with build_id.

    Parameters
    ----------
    build_id : str
        The id of the build to be pinned.
    wrapper : RequestWrapper
        The request wrapper to make requests calls.
    """
    pin_url = f"{BUILDS_ROOT}id:{build_id}/pin/"
    wrapper.put(pin_url)


def tag_build(build_info, tag: str, wrapper: RequestWrapper) -> None:
    """
    Add a tag with value tag to the build specified with build_info.

    Parameters
    ----------
    build_info : dict
        A dictionary describing the build to be modified.
    tag : str
        The new tag to be added to the build
    wrapper : RequestWrapper
        The request wrapper to make requests calls.
    """
    if 'tags' in build_info:
        new_tag_values = list({"name": x} for x in get_tag_values(build_info['tags']) if x != tag)
    else:
        new_tag_values = []

    new_tag_values.append({"name": tag})

    new_tags = {
        'count': len(new_tag_values),
        'tag': new_tag_values
    }

    tag_url = f"{BUILDS_ROOT}id:{build_info['id']}/tags/"
    wrapper.put_json(tag_url, new_tags)


def bag_new_build(build_info: dict, tag: str, wrapper: RequestWrapper) -> None:
    """
    Pin and tag the build specified with build_info.

    Parameters
    ----------
    build_info : dict
        A dictionary describing the build to be modified.
    tag : str
        The new tag to be added to the build
    wrapper : RequestWrapper
        The request wrapper to make requests calls.
    """
    pin_build(build_info['id'], wrapper)
    tag_build(build_info, tag, wrapper)


def set_pins_and_tags(packages_with_versions: dict, tag: str, wrapper: RequestWrapper):
    """
    Pin and tag the build specified with build_info.

    Parameters
    ----------
    packages_with_versions : dict
        A dictionary containing the nuget package ids and their current versions in the solution.
    tag : str
        The new tag to be added to the build
    wrapper : RequestWrapper
        The request wrapper to make requests calls.
    """
    for (p_id, build_config_id) in NUGET_PACKAGES:

        versions = packages_with_versions[p_id]
        if len(versions) != 1:
            logging.warning(f"Multiple versions of NuGet package '{p_id}' were found in the solution: {versions}")
            continue

        old_build_info = get_previous_build(build_config_id, tag, wrapper)
        if old_build_info:
            clean_up_build(old_build_info, tag, wrapper)

        nuget_package_file_name = f"{p_id}.{versions.pop()}.nupkg"
        new_build_info = get_new_build(build_config_id, nuget_package_file_name, wrapper)
        if new_build_info:
            bag_new_build(new_build_info, tag, wrapper)
        else:
            logging.warning(f"Could not find a build to tag NuGet package '{nuget_package_file_name}'.")


def parse_arguments():
    """
    Parse the arguments with which this script was called through
    """
    parser = argparse.ArgumentParser()

    parser.add_argument("checkout_dir", help="Path of the checkout directory")
    parser.add_argument("tag_string", help="The string that is used to tag the build.")
    parser.add_argument("user", help="User to authenticate with.")
    parser.add_argument("password", help="Password to authenticate with.")

    return parser.parse_args()


def run(dir_path: Path, tag_string: str, user: str, password: str):
    """
    Runs the script with the specified parameters.

    Parameters
    ----------
    dir_path : Path
        The path to the directory from which the used nuget packages should be retrieved.
    tag_string : str
        The tag that is used to tag the build.
    user : str
            The user to authenticate with.
    password : str
            The password to authenticate with.
    """
    package_files = get_packages_files(dir_path)
    all_packages = get_packages(package_files)
    set_pins_and_tags(all_packages, tag_string,
                      RequestWrapper(user, password))


if __name__ == "__main__":
    args = parse_arguments()

    run(Path(args.checkout_dir), args.tag_string, args.user, args.password)
