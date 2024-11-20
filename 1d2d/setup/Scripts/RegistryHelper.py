from typing import List, Set
import winreg as reg

def get_dotnet_versions() -> Set[str]:
    """Returns all .net versions registered in the registry

    Returns:
        Set[str]: Unique list of version numbers
    """
    root = reg.HKEY_LOCAL_MACHINE
    folder = r"SOFTWARE\Microsoft\NET Framework Setup\NDP"
    
    return sorted(set(_get_versions(root, folder)), key=lambda x: x.translate({ord('.'):None}), reverse=True)

def get_vcruntime_versions():
    """Returns all visual c++ runtime versions registered in the registry

    Returns:
        List[str]: List of version numbers
    """
    root = reg.HKEY_LOCAL_MACHINE
    folder = r"SOFTWARE\Microsoft\VisualStudio"

    return _get_versions(root, folder, exclude_names=["debug"])

def _get_versions(root :int, folder : str,  exclude_names : List[str] = []):
    with reg.OpenKey(root, folder) as reg_key:
         return _search_folder_version_keys(reg_key, exclude_names)

def _search_folder_version_keys(folder, exclude_names : List[str]):
    versions = []

    subfolders_count, subkeys_count, modified = reg.QueryInfoKey(folder)
    
    for subfolder_index in range(subfolders_count):
        try:
            subfolder_name = reg.EnumKey(folder, subfolder_index)
            if subfolder_name in exclude_names:
                continue
            with reg.OpenKeyEx(folder, subfolder_name) as subfolder:
                versions += _search_folder_version_keys(subfolder, exclude_names)
        except (WindowsError, KeyError, ValueError):
            print("Error reading " + folder)

    for subkey_index in range(subkeys_count):
        name, value, index = reg.EnumValue(folder, subkey_index)
        if name == "Version": 
            versions.append(value);   

    return versions