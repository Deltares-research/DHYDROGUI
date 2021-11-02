import requests
import argparse
import json


# Production Server
TEAMCITY_URL = "https://dpcbuild.deltares.nl"             
# Test Server
#TEAMCITY_URL = "http://tl-ts001.xtr.deltares.nl:8080" 

BUILDS_ROOT = f"{TEAMCITY_URL}/httpAuth/app/rest/builds/"
JSON_RESPONSE_HEADER = {'Accept': 'application/json'}


def get_previous_build(user: str, password: str, build_config: str, tag: str) -> dict:
    """
    Get the previous build tagged with the specified tag

    Parameters
    ----------
    user : str
        The user to authenticate with.
    password : str
        The password to authenticate with.
    build_config : str
        The build configuration id of the build to be retrieved.
    tag : str
        The tag with which the build should be retrieved.
    """
    build_url = f"{BUILDS_ROOT}buildType:{build_config},tag:{tag},pinned:true,count:1"

    previous_build_response = requests.get(build_url, 
                                           auth=(user, password),
                                           headers=JSON_RESPONSE_HEADER,
                                           verify=False)

    if previous_build_response.status_code != 200:
        return None

    return json.loads(previous_build_response.text)


def get_tag_values(tags):
    """
    Get the tags from the specified tags dictionary.

    Parameters
    ----------
    tags : dict
        A tags dictionary.
    """
    return list(x["name"] for x in tags["tag"])


def untag_build(user: str, password: str, build_info: str, tag: str) -> None:
    """
    Remove the specified tag from the build specified with build_info.

    Parameters
    ----------
    user : str
        The user to authenticate with.
    password : str
        The password to authenticate with.
    build_info : dict
        A dictionary describing the build to be modified.
    tag : str
        The tag to be removed from the build
    """
    if not tag in get_tag_values(build_info["tags"]):
        return

    new_tag_values = list({ "name": x } for x in get_tag_values(build_info["tags"]) if x != tag)
    new_tags = { 'count': len(new_tag_values)
               , 'tag': new_tag_values
               }

    tag_url = f"{BUILDS_ROOT}id:{build_info['id']}/tags/"
    requests.put(tag_url, 
                 auth=(user, password), 
                 headers=JSON_RESPONSE_HEADER, 
                 json=new_tags,
                 verify=False)
    

def unpin_build(user: str, password: str, build_id: str) -> None:
    """
    Unpin the build with build_id.

    Parameters
    ----------
    user : str
        The user to authenticate with.
    password : str
        The password to authenticate with.
    build_id : str
        The id of the build to be unpinned.
    """
    pin_url = f"{BUILDS_ROOT}id:{build_id}/pin/"
    requests.delete(pin_url, 
                    auth=(user, password), 
                    headers=JSON_RESPONSE_HEADER, 
                    verify=False)
    

def clean_up_build(user: str, password: str, build_info: dict, tag: str) -> None:
    """
    Remove the specified tag from the specified build and unpin if necessary.

    Parameters
    ----------
    user : str
        The user to authenticate with.
    password : str
        The password to authenticate with.
    build_info : dict
        A dictionary describing the build to be modified.
    tag : str
        The tag to be removed from the build
    """
    build_id = build_info["id"]
    untag_build(user, password, build_info, tag)
    
    if tag in get_tag_values(build_info["tags"]) and build_info["tags"]["count"] == 1:
        unpin_build(user, password, build_id)


def get_new_build(user: str, password: str, build_config: str, git_hash: str) -> None:
    """
    Get the build from build_config with the specified git hash number.

    Parameters
    ----------
    user : str
        The user to authenticate with.
    password : str
        The password to authenticate with.
    build_config : str
        The name of the build configuration of which the build is part.
    git_hash : str
        The git hash of the build to be retrieved.
    """
    build_url = f"{BUILDS_ROOT}buildType:{build_config},number:{git_hash},count:1"
    new_build_response = requests.get(build_url, 
                                      auth=(user, password), 
                                      headers=JSON_RESPONSE_HEADER,
                                      verify=False)
    if new_build_response.status_code != 200:
        return None

    return json.loads(new_build_response.text)
    

def pin_build(user: str, password: str, build_id: str) -> None:
    """
    Pin the build with build_id.

    Parameters
    ----------
    user : str
        The user to authenticate with.
    password : str
        The password to authenticate with.
    build_id : str
        The id of the build to be pinned.
    """
    pin_url = f"{BUILDS_ROOT}id:{build_id}/pin/"
    requests.put(pin_url, auth=(user, password), headers=JSON_RESPONSE_HEADER, verify=False)


def tag_build(user: str, password: str, build_info, tag: str) -> None:
    """
    Add a tag with value tag to the build specified with build_info.

    Parameters
    ----------
    user : str
        The user to authenticate with.
    password : str
        The password to authenticate with.
    build_info : dict
        A dictionary describing the build to be modified.
    tag : str
        The new tag to be added to the build
    """
    if "tags" in build_info:
        new_tag_values = list({ "name": x } for x in get_tag_values(build_info["tags"]) if x != tag)
    else:
        new_tag_values = []
    
    new_tag_values.append({ "name": tag })

    new_tags = { 'count': len(new_tag_values),
                 'tag': new_tag_values
    }

    tag_url = f"{BUILDS_ROOT}id:{build_info['id']}/tags/"
    requests.put(tag_url, auth=(user, password), headers=JSON_RESPONSE_HEADER, json=new_tags, verify=False)


def bag_new_build(user: str, password: str, build_info: dict, tag: str) -> None:
    """
    Pin and tag the build specified with build_info.

    Parameters
    ----------
    user : str
        The user to authenticate with.
    password : str
        The password to authenticate with.
    build_info : dict
        A dictionary describing the build to be modified.
    tag : str
        The new tag to be added to the build
    """
    pin_build(user, password, build_info["id"])
    tag_build(user, password, build_info, tag)


def parse_arguments():
    """
    Parse the arguments with which this script was called through
    argparse.ArgumentParser
    """
    parser = argparse.ArgumentParser()

    parser.add_argument("build_configuration_id", help="Build configuration id of the build configuration which need to be pinned.")
    parser.add_argument("user", help="User to authenticate with.")
    parser.add_argument("password", help="Password to authenticate with.")
    parser.add_argument("tag_string", help="The string that is used to tag the build.")
    parser.add_argument("git_hash", help="The short git hash of the build to be pinned and tagged.")

    return parser.parse_args()


if __name__ == "__main__":
    args = parse_arguments()

    old_build_info = get_previous_build(args.user, args.password, args.build_configuration_id, args.tag_string)

    if old_build_info:
        clean_up_build(args.user, args.password, old_build_info, args.tag_string)

    new_build_info = get_new_build(args.user, args.password, args.build_configuration_id, args.git_hash)

    if new_build_info:
        bag_new_build(args.user, args.password, new_build_info, args.tag_string)
