import argparse
import xml.etree.ElementTree as et
from pathlib import Path
import requests
import json

# Production Server
TEAMCITY_URL = "https://build.deltares.nl"
# Test Server
# TEAMCITY_URL = "http://tl-ts001.xtr.deltares.nl:8080"

BUILDS_ROOT = f"{TEAMCITY_URL}/httpAuth/app/rest/builds/"
JSON_RESPONSE_HEADER = {'Accept': 'application/json'}

# 0: nuget package id, 1: nuget package build configuration id
NUGET_PACKAGES = [
    ("Dimr.Libs", "DeltaShell_3rdPartyNuGetPackages_Dimr"),
    # ("RGFGRID", "DeltaShell_3rdPartyNuGetPackages_Rgfgrid"),
    # ("DIDO", "DeltaShell_3rdPartyNuGetPackages_Dido"),
    # ("PLCT.Libs", "DeltaShell_3rdPartyNuGetPackages_PlctWaq"),
    # ("Substances.Libs", "DeltaShell_3rdPartyNuGetPackages_SubstancesWaq")
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


def get_previous_build(build_config: str, tag: str) -> dict:
    """
    Get the previous build tagged with the specified tag

    Parameters
    ----------
    build_config : str
        The build configuration id of the build to be retrieved.
    tag : str
        The tag with which the build should be retrieved.
    """
    build_url = f"{BUILDS_ROOT}buildType:{build_config},tag:{tag},pinned:true,count:1"
    previous_build_response = wrapper.get(build_url)

    if previous_build_response.status_code != 200:
        return None

    return json.loads(previous_build_response.text)


def unpin_build(build_id: str) -> None:
    """
    Unpin the build with build_id.

    Parameters
    ----------
    build_id : str
        The id of the build to be unpinned.
    """
    pin_url = f"{BUILDS_ROOT}id:{build_id}/pin/"
    wrapper.delete(pin_url)


def clean_up_build(build_info: dict, tag: str) -> None:
    """
    Remove the specified tag from the specified build and unpin if necessary.

    Parameters
    ----------
    build_info : dict
        A dictionary describing the build to be modified.
    tag : str
        The tag to be removed from the build
    """
    build_id = build_info["id"]
    untag_build(build_info, tag)

    tag_info = build_info["tags"]
    if tag in get_tag_values(tag_info) and tag_info["count"] == 1:
        unpin_build(build_id)


def get_tag_values(tags):
    """
    Get the tags from the specified tags dictionary.

    Parameters
    ----------
    tags : dict
        A tags dictionary.
    """
    return list(x["name"] for x in tags["tag"])


def untag_build(build_info: dict, tag: str) -> None:
    """
    Remove the specified tag from the build specified with build_info.

    Parameters
    ----------
    build_info : dict
        A dictionary describing the build to be modified.
    tag : str
        The tag to be removed from the build
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

    for p in packages:
        packages[p] = list(packages[p])

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


def get_new_build(build_config_id: str, nuget_package_file_name: str) -> dict:
    """
    Get the build from build_config with the specified revision number.

    Parameters
    ----------
    build_config_id : str
        The id of the build configuration of which the build is part.
    nuget_package_file_name : str
        The expected nuget package file name within the build to be retrieved.
    """
    builds_url = f"{BUILDS_ROOT}?locator=buildType:{build_config_id}"

    new_builds_response = wrapper.get(builds_url)

    if new_builds_response.status_code != 200:
        return None

    builds = new_builds_response.json()
    for build in builds['build']:

        new_build_url = f"{BUILDS_ROOT}id:{build['id']}/"
        build_artifacts_url = f"{new_build_url}artifacts/"

        artifacts_response = wrapper.get(build_artifacts_url)

        if artifacts_response.status_code != 200:
            continue

        artifact_info = artifacts_response.json()
        if not artifact_info['file'][0]['name'] == nuget_package_file_name:
            continue

        new_build_info = wrapper.get(new_build_url)

        if new_build_info.status_code != 200:
            return None

        return new_build_info.json()

    return None


def pin_build(build_id: str) -> None:
    """
    Pin the build with build_id.

    Parameters
    ----------
    build_id : str
        The id of the build to be pinned.
    """
    pin_url = f"{BUILDS_ROOT}id:{build_id}/pin/"
    wrapper.put(pin_url)


def tag_build(build_info, tag: str) -> None:
    """
    Add a tag with value tag to the build specified with build_info.

    Parameters
    ----------
    build_info : dict
        A dictionary describing the build to be modified.
    tag : str
        The new tag to be added to the build
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


def bag_new_build(build_info: dict, tag: str) -> None:
    """
    Pin and tag the build specified with build_info.

    Parameters
    ----------
    build_info : dict
        A dictionary describing the build to be modified.
    tag : str
        The new tag to be added to the build
    """
    pin_build(build_info['id'])
    tag_build(build_info, tag)


def set_pins_and_tags(packages_with_versions: dict, tag: str):
    """
    Pin and tag the build specified with build_info.

    Parameters
    ----------
    packages_with_versions : dict
        A dictionary containing the nuget package ids and their current versions in the solution.
    tag : str
        The new tag to be added to the build
    """
    for p in NUGET_PACKAGES:
        p_id = p[0]
        build_config_id = p[1]

        if not len(packages_with_versions[p_id]) == 1:
            continue

        old_build_info = get_previous_build(build_config_id, tag)
        if old_build_info:
            clean_up_build(old_build_info, tag)

        nuget_package_file_name = f"{p_id}.{packages_with_versions[p_id][0]}.nupkg"
        new_build_info = get_new_build(build_config_id, nuget_package_file_name)
        if new_build_info:
            bag_new_build(new_build_info, tag)


def parse_arguments():
    parser = argparse.ArgumentParser()

    parser.add_argument("checkout_dir", help="Path of the checkout directory")
    parser.add_argument("tag_string", help="The string that is used to tag the build.")
    parser.add_argument("user", help="User to authenticate with.")
    parser.add_argument("password", help="Password to authenticate with.")

    return parser.parse_args()


if __name__ == "__main__":
    args = parse_arguments()
    wrapper = RequestWrapper(args.user, args.password)

    package_files = get_packages_files(Path(args.checkout_dir))
    all_packages = get_packages(package_files)

    set_pins_and_tags(all_packages, args.tag_string)
