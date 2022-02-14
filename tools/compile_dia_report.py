#!/usr/bin/python3
"""
Build diagnostic reports from a set of log files and output them as a HTML file. 
To be used as a build step in the TeamCity - Acceptance Tests configuration.
"""
__author__ = "Maarten Tegelaers"
__copyright__ = "Copyright 2019"
__version__ = "1.0"
__maintainer__ = "Maarten Tegelaers"
__email__ = "Maarten.Tegelaers@deltares.nl"
__status__ = "Development"

from pathlib import Path
import shutil
import argparse


DOCUMENT_TEMPLATE = """<!DOCTYPE html>
<html>
{0}
</html>
"""


SCRIPT = """  <script>
  document.addEventListener('DOMContentLoaded', function() {
      let cardToggles = document.getElementsByClassName('card-toggle');
      for (let i = 0; i < cardToggles.length; i++) {
          cardToggles[i].addEventListener('click', e => {
              e.currentTarget.parentElement.parentElement.childNodes[3].classList.toggle('is-hidden');
          });
      }
  });
  </script>
"""


HEADER = """  <head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Acceptance Test - .dia Report</title>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/bulma/0.7.5/css/bulma.min.css">
    <script defer src="https://use.fontawesome.com/releases/v5.3.1/js/all.js"></script>
    <style>
        .scrollable {
            max-height: 40pc;
            overflow: auto;
        }

        .debug-color {
            color: #6C7680;
        }

        .warning-color {
            color: #F2AE49;
        }

        .error-color {
            color: #F07171;
        }

    </style>
  </head>
"""


BODY_CONTAINER_TEMPLATE = """  <body>
{0}
  </body>
"""


SECTION_TEMPLATE = """    <section class="section">
      <div class="container">
          <h1 class="title">{0}</h1>
{1}
      </div>
    </section
"""


ELEMENT_TEMPLATE = """<div class="card is-fullwidth">
  <header class="card-header">
    <p class="card-header-title">{0}</p>
      <a class="card-header-icon card-toggle">
        <i class="fa fa-angle-down"></i>
      </a>
  </header>
  <div class="card-content is-hidden">
    <div class="content">
      <div class="scrollable">
{1}
      </div>
    </div>
  </div>
</div>
"""

ELEMENTFOLDER_TEMPLATE = """<div class="card is-fullwidth">
  <header class="card-header">
    <p class="card-header-title">{0}</p>
      <a class="card-header-icon card-toggle">
        <i class="fa fa-angle-down"></i>
      </a>
  </header>
  <div class="card-content is-hidden"> 
  {1} 
  </div>
</div>
"""


def format_dia_file(dia_content: str) -> str:
    """
    Format the specified dia file content to be placed within the diagnostic report.

    :param dia_content: The dia file content to be formatted.
    :returns: The formatted dia file content.
    """
    lines = dia_content.splitlines()

    for i in range(len(lines)):
        if lines[i][0:2] != "**":
            continue

        vals = lines[i].split(':', 1)

        if len(vals) < 1: 
            continue

        start = vals[0]

        if len(vals) < 2:
            end = ""
        else:
            end = vals[1]

        message_type = start[3:].strip().lower()

        if (message_type == "debug" or 
            message_type == "warning" or 
            message_type == "error"):
            start = '<span class="{}-color">{}:</span>'.format(message_type, start)
        else:
            start = "{}:".format(start)

        lines[i] = "{}{}".format(start, end)

    return """<pre>
{}
</pre>""".format("\n".join(lines))

def format_xml_file(xml_content: str) -> str:
    """
    Format the specified xml file content to be placed within the diagnostic report.

    :param xml_content: The xml file content to be formatted.
    :returns: The formatted xml file content.
    """
    xml_content = xml_content.replace("<", "&lt;")
    xml_content = xml_content.replace(">", "&gt;")

    return """<pre>
{}
</pre>""".format(xml_content)

def format_general_file(content: str) -> str:
    """
    Format general file content to be placed within the diagnostic report.

    :param content: The file content to be formatted.
    :returns: The formatted file content.
    """ 
    
    return """<pre>
{}
</pre>""".format(content)

def construct_dia_element(header: str, dia_content: str) -> str:
    """
    Construct a dia element to be placed within the body of the report with 
    the given header and content.

    :param header: The header to be placed into the collapsable menu.
    :param dia_content: The actual content to place in the element
    :returns: A formatted element that can be placed inside the body.
    """
    return ELEMENT_TEMPLATE.format(header, format_dia_file(dia_content))    

def construct_xml_element(header: str, xml_content: str) -> str:
    """
    Construct a xml element to be placed within the body of the report with 
    the given header and content.

    :param header: The header to be placed into the collapsable menu.
    :param xml_content: The actual content to place in the element
    :returns: A formatted element that can be placed inside the body.
    """
    return ELEMENT_TEMPLATE.format(header, format_xml_file(xml_content)) 

def construct_general_element(header: str, content: str) -> str:
    """
    Construct a general element to be placed within the body of the report with 
    the given header and content.

    :param header: The header to be placed into the collapsable menu.
    :param content: The actual content to place in the element
    :returns: A formatted element that can be placed inside the body.
    """
    return ELEMENT_TEMPLATE.format(header, format_general_file(content))    

def construct_element(header: str, content: str) -> str:
    """
    Based on the header suffix construct a formatted element to be 
    placed within the body of the report with the given header and content.

    :param header: The header to be placed into the collapsable menu.
    :param xml_content: The actual content to place in the element
    :returns: A formatted element that can be placed inside the body.
    """
    if header.endswith('.dia') or header == "sobek_3b.log":
        return construct_dia_element(header, content)
    elif header.endswith('.xml'):
        return construct_xml_element(header, content)
    else:
        return construct_general_element(header, content)
         
def construct_folders(path: Path) -> str:
    """
    Construct a formatted element for integrated model folders in which
    diagnostic files are grouped 
    """
    content = []

    for p in path.glob("*"):        
        if p.is_dir():
            raise ValueError('In {} it is not possible to have a subdirectory, called {}'.format(path, p.name))
        else:
            content.append(construct_element(p.name, p.read_text()))        
            
    return ELEMENTFOLDER_TEMPLATE.format(path.name, "\n".join(content))

    
def build_section(path: Path) -> str:
    """
    Build a single section from the specified folder containing a set of log files.

    :param path: Path to the folder containing a set of log files.
    :returns: A formatted html section describing the files within the path.
    """
    content = []
   
    for p in path.glob("*"):        
        if p.is_dir():
            content.append(construct_folders(p))
        else:            
            content.append(construct_element(p.name, p.read_text()))
     
    return SECTION_TEMPLATE.format(path.name, "\n".join(content))


def build_report_page(src_path: Path) -> str:
    """
    Build the report page from the specified folder containing diagnostic files.

    :param src_path: The base path containing the folders with actual content
    :returns: A formatted html document describing the actual report.
    """

    section_content = (build_section(p) for p in src_path.glob("*"))    
    body = BODY_CONTAINER_TEMPLATE.format("\n".join(section_content))

    document = DOCUMENT_TEMPLATE.format("\n".join([HEADER, body, SCRIPT]))

    return document


def compose_argument_parser() -> argparse.ArgumentParser:
    """
    Compose the parser to be used in the main function of this compile_dia_report.py script

    :returns: An argument parser with the correct settings.
    """
    parser = argparse.ArgumentParser()
    parser.add_argument("root_path", 
                        help="Path diagnostics report folder.",
                        type=str)
    parser.add_argument("output_dir_path", 
                        help="Output path for the generated dia_report.zip.",
                        type=str)

    parser.add_argument("-c", "--clean",
                        action="store_true",
                        help="clean diagnostics_path after compiling the report")

    return parser


if __name__ == "__main__":
    # Parse the arguments
    parser = compose_argument_parser()
    args = parser.parse_args()

    # Build the report
    report_src_path = Path(args.root_path)

    html_doc = build_report_page(report_src_path)

    report_path = report_src_path / Path("index.html")
    report_path.write_text(html_doc)

    # Create the archive which will be published as an artifact
    output_path = Path(args.output_dir_path)

    if not (output_path.exists() and output_path.is_dir()):
        output_path.mkdir(parents=True)

    shutil.make_archive(str(output_path / Path("dia_report")), 'zip', str(report_src_path))

    # Remove the source folder, since we just put it in an archive.
    if args.clean:
        shutil.rmtree(str(report_src_path))
