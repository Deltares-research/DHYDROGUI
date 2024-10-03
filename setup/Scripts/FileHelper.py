import subprocess
import os as _os
import win32api as _win32
import time as _time
import struct as _struct
import pdf as _pdf
from pdf.pdf import DocumentInformation as _DocumentInformation

from typing import List, Dict, Iterable

class _FileInfo:

    def __init__(self, product_version : str, file_version : str, date_modified : str, bitness : str):
      self._product_version = product_version
      self._file_version = file_version
      self._date_modified = date_modified
      self._bitness = bitness
    
    @property
    def product_version(self):
        return self._product_version

    @property
    def file_version(self):
        return self._file_version
    
    @property
    def date_modified(self):
        return self._date_modified

    @property
    def bitness(self):
        return self._bitness

def is_managed_dll(path:str) -> bool:
    """Checks if path is a managed (.net) dll

    Args:
        path (str): path to the dll

    Returns:
        bool: true if path points to managed (.net) dll
    """
    with open(path, "rb") as stream:
        stream.seek(0x3C,0) # PE, SizeOfHeaders starts at 0x3B, second byte is 0x80 for .NET
        i1 = stream.read(1)

        stream.seek(0x86,0) # 0x03 for managed code
        i2 = stream.read(1)

        return i1 == b'\x80' and i2 == b'\x03'

def get_file_entries(path:str, folders_to_skip : List[str] = []) -> List[_os.DirEntry]: 
    """Recursivly goes through the supplied path
    to find all DirEntries

    Args:
        path (str): path to search
        folders_to_skip (List[str]): folders to skip in the search

    Returns:
        List[_os.DirEntry]: List of found directory entries (DirEntry)
    """
    entries = []
    with _os.scandir(path) as it:
        for entry in it:
            if entry.is_file():
                entries.append(entry)
            else:
                if (entry.name in folders_to_skip):
                    continue
                entries += get_file_entries(path + "\\" + entry.name)                
    
    return entries

def get_file_versions(directory : str) -> List[str]:
    """Gets all file versions for dll, exe and pdf files in
    the provided folder (recursively)

    Args:
        directory (str): Directory to search

    Returns:
        List[str]: List of files with found versions
    """
    file_versions = []

    for entry in get_file_entries(directory):

        path = entry.path
        is_pdf = path.endswith('.pdf')

        if (not path.endswith('.dll') 
            and not path.endswith('.exe')
            and not is_pdf):
            continue
        
        file_versions.append(path)

        if (is_pdf):
            doc_info = _get_pdf_info(path)
            if (doc_info.author):
                file_versions.append(f"\t Author  : {doc_info.author}")
            if '/pdfauthor' in doc_info:
                file_versions.append(f"\t Author  : {doc_info['/pdfauthor']}")
            if '/Suite' in doc_info:
                file_versions.append(f"\t Suite   : {doc_info['/Suite']}")
            if '/Revision' in doc_info:
                file_versions.append(f"\t Revision: {doc_info['/Revision']}")
            continue
        
        
        if is_managed_dll(path):
            file_info = _get_version_attributes(entry)
            file_versions.append(f"\t File Version   : {file_info.file_version} ({file_info.bitness}) {file_info.date_modified}")
            file_versions.append(f"\t Product Version: {file_info.product_version}")

        else:
            print(f"processing : {path}")
            fileVersionsInfoUnmanagedDeltares = _get_native_deltares_dll_versions(path)
            if not fileVersionsInfoUnmanagedDeltares:
                file_info = _get_version_attributes(entry)
                file_versions.append(f"\t File Version   : {file_info.file_version} ({file_info.bitness}) {file_info.date_modified}")
                file_versions.append(f"\t Product Version: {file_info.product_version}")
            else:
                file_versions += fileVersionsInfoUnmanagedDeltares

        details = check_signature(path)
        info = parse_details(details)
        # Loop through the dictionary
        for key, value in info.items():
            file_versions.append(f"\t {key} : {value}")

    return file_versions

def check_signature(dll_path):
    """ Run sigcheck.exe on the given file (DLL or EXE) and parse the output for selected details. """
    result = subprocess.run(['sigcheck.exe', '-nobanner', '-a', '-i', dll_path], capture_output=True, text=True)
    output = result.stdout
    return output.split('\n')

def parse_details(details):
    """ Parse the details to extract only the required fields for CSV output, replacing any semicolons in values with periods. """
    info = {
        'Verified': '',
        'Signing date': '',
        'Company': '',
        'Description': '',
        'Product': '',
        'Prod version': '',
        'File version': '',
        'Machine type': '',
        'Binary version': '',
        'Copyright': '',
        'Comments': ''
    }
    for line in details:
        if ':' in line:
            key, value = line.split(':', 1)
            key = key.strip()
            value = value.strip().replace(';', '.')
            if key in info:
                info[key] = value
    return info


def _get_version_attributes(entry: _os.DirEntry) -> _FileInfo:
    
    file_stats = entry.stat()
    date_modified = _time.strftime("%b %d %Y, %H:%M:%S", _time.localtime(file_stats.st_mtime))
    bitness = _get_dll_bitness(entry.path)

    try:
        info = _win32.GetFileVersionInfo(entry.path, "\\")
    except:
        return _FileInfo("-", "-", date_modified, bitness)    

    file_version = _get_version('FileVersionMS', 'FileVersionLS', info)
    product_version = _get_version('ProductVersionMS', 'ProductVersionLS', info)

    return _FileInfo(product_version, file_version, date_modified, bitness)

def _get_version(major_version_variable_name : str, minor_version_variable_name : str, info: Dict[str, str]) -> str:
    version = "-"
    
    if (major_version_variable_name in info):
        ms = info[major_version_variable_name]
        version = f"{_win32.HIWORD(ms)}.{_win32.LOWORD (ms)}"
    
    if (minor_version_variable_name in info):
        ls = info[minor_version_variable_name]
        version += f".{_win32.HIWORD (ls)}.{_win32.LOWORD (ls)}"

    return version

def _get_dll_bitness(path: str) -> str:
    with open(path, "rb") as f:
        doshdr = f.read(64)
        magic, padding, offset = _struct.unpack('2s58si', doshdr)
        
        if magic != b'MZ':
            return 'unknown'

        f.seek(offset, _os.SEEK_SET)
        pehdr = f.read(6)
        
        magic, padding, machine = _struct.unpack('2s2sH', pehdr)
        
        if magic != b'PE':
            return 'unknown'
        if machine == 0x014c:
            return 'x86_x64'
        if machine == 0x0200:
            return 'IA64'
        if machine == 0x8664:
            return 'x64'
        return 'unknown'

def _get_pdf_info(path:str) -> _DocumentInformation:
    with open(path, 'rb') as f:
        pdf = _pdf.PdfFileReader(f)
        info = pdf.getDocumentInfo()
    return info

def _get_native_deltares_dll_versions(path: str) -> List[str]:
    data = []
    tags = ['@(#)Deltares, ', '@(#) $HeadURL:']
    for version_line in _get_lines_with_tags(path, tags):
        header = "Url : " if version_line.startswith("http") else "Version : Deltares, "
        data.append(f"\t{header}{version_line}")

    return data

def _get_lines_with_tags(path: str, tags : List[str]) -> Iterable[str]:
    lines_with_tags = _get_lines_with_tags_basic(path, tags)

    for line in lines_with_tags:
        split_line_list = [l for l in line.split('\x00') if l]
        for split_line in split_line_list:
            for tag in tags:
                if (not (tag in split_line)):
                    continue
                yield split_line.replace(tag, "").strip()

def _get_lines_with_tags_basic(path: str, tags : List[str]):
    lines_with_tags = []
    with open(path, "r", errors='ignore') as f:
        for line in f:
            for tag in tags:
                if (not (tag in line)):
                    continue
                lines_with_tags.append(line)

    return lines_with_tags