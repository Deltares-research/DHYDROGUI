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
    ("RGFGRID", "DeltaShell_3rdPartyNuGetPackages_Rgfgrid"),
    ("DIDO", "DeltaShell_3rdPartyNuGetPackages_Dido"),
    ("PLCT.Libs", "DeltaShell_3rdPartyNuGetPackages_PlctWaq"),
    ("Substances.Libs", "DeltaShell_3rdPartyNuGetPackages_SubstancesWaq")
]


def get_previous_build(build_config: str, tag: str, user: str, password: str) -> dict:
    """
    Get the previous build tagged with the specified tag

    Parameters
    ----------
    build_config : str
        The build configuration id of the build to be retrieved.
    tag : str
        The tag with which the build should be retrieved.
    user : str
        The user to authenticate with.
    password : str
        The password to authenticate with.
    """
    build_url = f"{BUILDS_ROOT}buildType:{build_config},tag:{tag},pinned:true,count:1"
    previous_build_response = requests.get(build_url,
                                           auth=(user, password),
                                           headers=JSON_RESPONSE_HEADER)

    if previous_build_response.status_code != 200:
        return None

    return json.loads(previous_build_response.text)


def unpin_build(build_id: str, user: str, password: str) -> None:
    """
    Unpin the build with build_id.

    Parameters
    ----------
    build_id : str
        The id of the build to be unpinned.
    user : str
        The user to authenticate with.
    password : str
        The password to authenticate with.
    """
    pin_url = f"{BUILDS_ROOT}id:{build_id}/pin/"
    requests.delete(pin_url, auth=(user, password), headers=JSON_RESPONSE_HEADER)


def clean_up_build(build_info: dict, tag: str, user: str, password: str) -> None:
    """
    Remove the specified tag from the specified build and unpin if necessary.

    Parameters
    ----------
    build_info : dict
        A dictionary describing the build to be modified.
    tag : str
        The tag to be removed from the build
    user : str
        The user to authenticate with.
    password : str
        The password to authenticate with.
    """
    build_id = build_info["id"]
    untag_build(build_info, tag,
                user, password)

    tag_info = build_info["tags"]
    if tag in get_tag_values(tag_info) and tag_info["count"] == 1:
        unpin_build(build_id,
                    user, password)


def get_tag_values(tags):
    """
    Get the tags from the specified tags dictionary.

    Parameters
    ----------
    tags : dict
        A tags dictionary.
    """
    return list(x["name"] for x in tags["tag"])


def untag_build(build_info: dict, tag: str, user: str, password: str) -> None:
    """
    Remove the specified tag from the build specified with build_info.

    Parameters
    ----------
    build_info : dict
        A dictionary describing the build to be modified.
    tag : str
        The tag to be removed from the build
    user : str
        The user to authenticate with.
    password : str
        The password to authenticate with.
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
    requests.put(tag_url, auth=(user, password), headers=JSON_RESPONSE_HEADER, json=new_tags)


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


def get_new_build(build_config_id: str, nuget_package_file_name: str, user: str, password: str, ) -> dict:
    """
    Get the build from build_config with the specified revision number.

    Parameters
    ----------
    build_config_id : str
        The id of the build configuration of which the build is part.
    nuget_package_file_name : str
        The expected nuget package file name within the build to be retrieved.
    user : str
        The user to authenticate with.
    password : str
        The password to authenticate with.
    """
    builds_url = f"{BUILDS_ROOT}?locator=buildType:{build_config_id}"

    new_builds_response = requests.get(builds_url,
                                       auth=(user, password),
                                       headers=JSON_RESPONSE_HEADER)
    if new_builds_response.status_code != 200:
        return None

    builds = new_builds_response.json()
    for build in builds['build']:

        new_build_url = f"{BUILDS_ROOT}id:{build['id']}/"
        build_artifacts_url = f"{new_build_url}artifacts/"

        artifacts_response = requests.get(build_artifacts_url,
                                          auth=(user, password),
                                          headers=JSON_RESPONSE_HEADER)

        if artifacts_response.status_code != 200:
            continue

        artifact_info = artifacts_response.json()
        if not artifact_info['file'][0]['name'] == nuget_package_file_name:
            continue

        new_build_info = requests.get(new_build_url,
                                      auth=(user, password),
                                      headers=JSON_RESPONSE_HEADER)

        if new_build_info.status_code != 200:
            return None

        return new_build_info.json()

    return None


def pin_build(build_id: str, user: str, password: str) -> None:
    """
    Pin the build with build_id.

    Parameters
    ----------
    build_id : str
        The id of the build to be pinned.
    user : str
        The user to authenticate with.
    password : str
        The password to authenticate with.
    """
    pin_url = f"{BUILDS_ROOT}id:{build_id}/pin/"
    requests.put(pin_url, auth=(user, password), headers=JSON_RESPONSE_HEADER)


def tag_build(build_info, tag: str, user: str, password: str) -> None:
    """
    Add a tag with value tag to the build specified with build_info.

    Parameters
    ----------
    build_info : dict
        A dictionary describing the build to be modified.
    tag : str
        The new tag to be added to the build
    user : str
        The user to authenticate with.
    password : str
        The password to authenticate with.
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
    requests.put(tag_url, auth=(user, password), headers=JSON_RESPONSE_HEADER, json=new_tags)


def bag_new_build(build_info: dict, tag: str, user: str, password: str) -> None:
    """
    Pin and tag the build specified with build_info.

    Parameters
    ----------
    build_info : dict
        A dictionary describing the build to be modified.
    tag : str
        The new tag to be added to the build
    user : str
        The user to authenticate with.
    password : str
        The password to authenticate with.
    """
    pin_build(build_info['id'],
              user, password)
    tag_build(build_info, tag,
              user, password, )


def set_pins_and_tags(packages_with_versions: dict, tag: str, user: str, password: str):
    """
    Pin and tag the build specified with build_info.

    Parameters
    ----------
    packages_with_versions : dict
        A dictionary containing the nuget package ids and their current versions in the solution.
    tag : str
        The new tag to be added to the build
    user : str
        The user to authenticate with.
    password : str
        The password to authenticate with.
    """
    for p in NUGET_PACKAGES:
        p_id = p[0]
        build_config_id = p[1]

        if not len(packages_with_versions[p_id]) == 1:
            continue

        old_build_info = get_previous_build(build_config_id, tag,
                                            user, password)
        if old_build_info:
            clean_up_build(old_build_info, tag,
                           user, password)

        nuget_package_file_name = f"{p_id}.{packages_with_versions[p_id][0]}.nupkg"
        new_build_info = get_new_build(build_config_id, nuget_package_file_name,
                                       user, password)
        if new_build_info:
            bag_new_build(new_build_info, tag,
                          user, password)


def parse_arguments():
    parser = argparse.ArgumentParser()

    parser.add_argument("checkout_dir", help="Path of the checkout directory")
    parser.add_argument("tag_string", help="The string that is used to tag the build.")
    parser.add_argument("user", help="User to authenticate with.")
    parser.add_argument("password", help="Password to authenticate with.")

    return parser.parse_args()


if __name__ == "__main__":
    args = parse_arguments()

    package_files = get_packages_files(Path(args.checkout_dir))
    all_packages = get_packages(package_files)

    set_pins_and_tags(all_packages, args.tag_string,
                      args.user, args.password, )
