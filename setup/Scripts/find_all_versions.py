import argparse
import sys
import os
import platform
import psutil

from typing import List
from datetime import datetime

import FileHelper as fh
import RegistryHelper as rh

'''
Author: Jan Mooiman
E-Mail: jan.mooiman@deltares.nl
Date  : 10 sep 2017

This script list all what strings, starting with @(#)Deltares, in all subdirectories of a given root directory
The root directory is specified by the argument --srcdir .....

'''

'''
Author: Ralph Peelen
E-Mail: ralph.peelen@deltares.nl
Date  : 28 feb 2022

This script list all versions of exe and dlls, in all subdirectories of a given root directory
The root directory is specified by the argument --srcdir .....

'''

'''
Author: Ralph Peelen
E-Mail: ralph.peelen@deltares.nl
Date  : 23 mrt 2022

generate in html

'''
'''
Author: Hidde Elzinga
E-Mail: hidde.elzinga@deltares.nl
Date  : 30 mrt 2022

refactoring + speedup

'''
def parse_args():
    parser = argparse.ArgumentParser(description='Batch process to list all what-strings', argument_default=argparse.SUPPRESS, conflict_handler='resolve')

    parser.add_argument('-s', '--srcdir',
                        help="Root directory from the what-strings are listed",
                        dest='src_dir',
                        default=os.getcwd())
    parser.add_argument('-o', '--output',
                        help="Output filename.",
                        dest='out_put',
                        default='dimr_version.txt')
    parser.add_argument('-w', '--useHTML',
                        help="Output in html format.",
                        dest='use_html',
                        action='store_true',                        
                        default=False
                        )
    args = parser.parse_args()

    if not os.path.exists(args.src_dir.strip()):
        print (f"Given directory does not exists: {args.src_dir.strip()}")
        exit()

    return args
    
def get_as_html_file(data: List[str]):
    header = ["<!DOCTYPE html>",
            "<html lang=\"en-us\">",            
            "<head>",
            "\t<title>Version information</title>",            
            "</head>",
            "",
            "<body>",
            "<pre>"]
            
    footer = [
        "</>",
        "</body>",
        "</html>"]

    file_content = header + data + footer
    
    return "\n".join(file_content)

def get_platform_data() -> List[str]:
    return [
        f"Python system version: {sys.version}",
        f"Platform: {platform.system()}",
        f"Platform release: {platform.release()}",
        f"Platform version: {platform.version()}",
        f"Architecture: {platform.machine()}",
        f"Processor: {platform.processor()}",
        f"Ram: {str(round(psutil.virtual_memory().total / (1024.0 **3)))} GB"
    ]

def get_header_data(header : str, html : bool) -> List[str]:
    if (html):
        return [f"<h1>{header}</h1>"]

    length = len(header) + 2
    return ["",
        "-"*length,
        f"|{header}|",
        "-"*length,
        ""]

def write_to_file(path:str, text: str):
    print(f'Listing is written to: {path}')

    if os.path.exists(path):
        os.remove(path)

    log_file = open(path, "a")
    log_file.write(text)
    log_file.close()

if __name__ == "__main__":
    
    args = parse_args()
    use_html = args.use_html

    log_data = []

    start_time = datetime.now()  
    
    print(f"Start: {start_time}")
    log_data.append(f"Start: {start_time}")

    print(f"{sys.version}")
    log_data += get_header_data("Platform", use_html)
    log_data += get_platform_data()

    log_data += get_header_data("Dot Net Versions installed", use_html)
    log_data += rh.get_dotnet_versions()

    log_data += get_header_data("VC Runtimes installed", use_html)
    log_data += rh.get_vcruntime_versions()
    
    log_data += get_header_data("Dll and exe versions + Pdf Suite & Revision information", use_html)

    print(f"Root Directory: {args.src_dir}")
    log_data += fh.get_file_versions(args.src_dir.strip())

    print("Processing done")
    
    end_time = datetime.now()

    log_data.append("")   
    log_data.append(f"End  : {end_time}")
    log_data.append('Done')
    
    text = get_as_html_file(log_data) if use_html else "\n".join(log_data)
   
    write_to_file(args.out_put, text)

    print(f"End  : {end_time}")
    print(f'Done (in {str(end_time - start_time)})')
