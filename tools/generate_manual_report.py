#!/usr/bin/python3
"""
Build a report of updating the manuals.
"""
__author__ = "Maarten Tegelaers"
__copyright__ = "Copyright 2019"
__version__ = "1.0"
__maintainer__ = "Maarten Tegelaers"
__email__ = "Maarten.Tegelaers@deltares.nl"
__status__ = "Development"


from pathlib import Path
import shutil


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
    <p class="card-header-title {2}">{0}</p>
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

LIST_ITEM_TEMPLATE = "<li>{0}</li>"

def _generate_list_card(title: str, 
                        manuals_list: list, 
                        background_color_str: str) -> str:
    list_items = "\n".join(LIST_ITEM_TEMPLATE.format(str(x)) for x in manuals_list)
    body = "<ul>\n{0}</ul>".format(list_items)

    return ELEMENT_TEMPLATE.format(title, body, background_color_str)


def _generate_optional_list_card(title_template: str, 
                                 manuals_list: list, 
                                 background_color_str: str) -> str:
    n_manuals = len(manuals_list)

    if n_manuals <= 0:
        return None

    return _generate_list_card(title_template.format(n_manuals), 
                               manuals_list, 
                               background_color_str)


def generate_not_updated_in_repository_element(manuals_list: list) -> str:
    background_color = "has-background-danger"
    title_template = "Not updated in the repository: {0}"
    
    return _generate_optional_list_card(title_template, 
                                       manuals_list, 
                                       background_color)


def generate_not_found_in_repository_element(manuals_list: list) -> str:
    background_color = "has-background-warning"
    title_template = "Not found in the repository: {0}"
    
    return _generate_optional_list_card(title_template, 
                                       manuals_list, 
                                       background_color)


def generate_copied_element(manuals: list) -> str:
    background_color = "" if (len(manuals)) > 0 else "has-background-danger"
    title_template = "Replaced in repository: {0}".format(len(manuals))

    return _generate_list_card(title_template, 
                               manuals, 
                               background_color)


NESTED_LIST_ITEM = "<li><h3>{0}</h3>\n<ul>\n{1}</ul></li>"


def generate_covers_element(manuals: dict) -> str:
    n_manuals = 0
    cover_elems = []

    for key in manuals:
        n_manuals += len(manuals[key])

        list_items = "\n".join(LIST_ITEM_TEMPLATE.format(x) for x in manuals[key])
        cover_elems.append(NESTED_LIST_ITEM.format(key, list_items))

    cover_str = "".join(x for x in cover_elems)

    body = "<ul>{0}</ul>".format(cover_str)

    background_color_str = "" if (len(manuals)) > 0 else "has-background-danger"
    title = "Manuals in artifact: {0}".format(n_manuals)
    
    return ELEMENT_TEMPLATE.format(title, body, background_color_str)


def generate_source_element(manuals: list) -> str:
    background_color = "" if (len(manuals)) > 0 else "has-background-danger"
    title = "Manuals in repository: {0}".format(len(manuals))
    
    return _generate_list_card(title, 
                               manuals, 
                               background_color)


def generate_report_str(cover_dict: dict, 
                        src_list: list, 
                        updated_elems: list,
                        not_found_in_repo: list,
                        not_found_in_artifact: list) -> str:
    elements = []

    not_updated_in_repo = generate_not_updated_in_repository_element(not_found_in_artifact)
    if not_updated_in_repo:
        elements.append(not_updated_in_repo)

    not_found_in_repo = generate_not_found_in_repository_element(not_found_in_repo)
    if not_found_in_repo:
        elements.append(not_found_in_repo)

    elements.append(generate_copied_element(updated_elems))
    elements.append(generate_source_element(list("{} ({})".format(x.name, x.cover) for x in src_list)))
    elements.append(generate_covers_element(cover_dict))

    section_content = "\n".join(elements)
    section = SECTION_TEMPLATE.format("Automatic Manual Update", section_content)
    body = BODY_CONTAINER_TEMPLATE.format(section)

    return DOCUMENT_TEMPLATE.format("\n".join((HEADER, body, SCRIPT)))


def generate_report(svn_root_path: Path,
                    cover_dict: dict, 
                    src_list: list,
                    updated_elems: list,
                    not_found_in_repo: list,
                    not_found_in_artifact: list) -> None:
    html_content = generate_report_str(cover_dict, 
                                       src_list, 
                                       updated_elems, 
                                       not_found_in_repo, 
                                       not_found_in_artifact)

    zip_name = "manual_report"
    report_path_dir = svn_root_path.resolve() / Path(zip_name)

    if not (report_path_dir.exists() and report_path_dir.is_dir()):
        report_path_dir.mkdir(parents=True)

    report_path = report_path_dir / Path("index.html")
    report_path.write_text(html_content)
