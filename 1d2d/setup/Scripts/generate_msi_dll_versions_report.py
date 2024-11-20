import argparse
from typing import Optional, Sequence, Tuple
from pathlib import Path
import re

DOCUMENT_TEMPLATE = """<!DOCTYPE html>
<html>
{0}
</html>
"""

SCRIPT = """  <script>
    let tabsWithContent = (function () {
        let tabs = document.querySelectorAll('.tabs li');
        let tabsContent = document.querySelectorAll('.tab-content');

        let deactivateAllTabs = function () {
            tabs.forEach(function (tab) {
                tab.classList.remove('is-active');
            });
        };

        let hideTabsContent = function () {
            tabsContent.forEach(function (tabContent) {
                tabContent.classList.remove('is-active');
            });
        };

        let activateTabsContent = function (tab) {
            tabsContent[getIndex(tab)].classList.add('is-active');
        };

        let getIndex = function (el) {
            return [...el.parentElement.children].indexOf(el);
        };

        tabs.forEach(function (tab) {
            tab.addEventListener('click', function () {
                deactivateAllTabs();
                hideTabsContent();
                tab.classList.add('is-active');
                activateTabsContent(tab);
            });
        })

        tabs[0].click();
    })();
  </script>
"""

HEADER = """<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>Acceptance Test - .dia Report</title>
  <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/bulma/0.7.5/css/bulma.min.css">
  <script defer src="https://use.fontawesome.com/releases/v5.3.1/js/all.js"></script>
  <style>
    #tabs-with-content .tabs:not(:last-child) {
      margin-bottom: 0;
    }

    #tabs-with-content .tab-content {
      padding: 1rem;
      display: none;
    }

    #tabs-with-content .tab-content.is-active {
      display: block;
    }
  </style>
</head>
"""


BODY_CONTAINER_TEMPLATE = """<body>
  <section class="section">
{0}
  </section>
</body>
"""

TABS_TEMPLATE = """    <div id="tabs-with-content">
      <div class="tabs is-centered">
        <ul>
{0}
        </ul>
      </div>
      <div>
{1}
      </div>
    </div>
"""

TABS_HEADER = """        <li><a>{0}</a></li>"""

TABS_CONTENT = """        <section class="tab-content">
          <div class="content">
            <div>
              <pre>
{0}
              </pre>
            </div>
          </div>
        </section>
"""


def format_tabs_header(name: str) -> str:
    return TABS_HEADER.format(name)


def format_tabs_content(content: str) -> str:
    return TABS_CONTENT.format(content)


def format_tabs(data: Sequence[Tuple[str, str]]) -> str:
    header_data = "\n".join((format_tabs_header(elem[0]) for elem in data))
    content_data = "\n".join((format_tabs_content(elem[1]) for elem in data))

    return TABS_TEMPLATE.format(header_data, content_data)


def format_document(data: Sequence[Tuple[str, str]]):
    tabs = format_tabs(data)
    body = BODY_CONTAINER_TEMPLATE.format(tabs)

    return DOCUMENT_TEMPLATE.format("\n".join((HEADER, body, SCRIPT)))


CONTENT_HEADER_TEMPLATE = """<h1>{0}</h1>"""


regex_log = re.compile(r"<\!-- fileInfo -->(?P<log>.*)<\!-- fileInfo -->", re.DOTALL)


def extract_log(file_content: str) -> Optional[str]:
    m = regex_log.search(file_content)

    if m is not None:
        return m.group("log")
    else:
        return None


def to_elem(elem: str) -> Tuple[str, Path]:
    tab_name, path_str = elem[1:-1].split(",")

    return tab_name.strip(), Path(path_str.strip())


def to_content(elem: Tuple[str, Path]) -> Optional[Tuple[str, str]]:
    name, path_dir = elem

    path = next(path_dir.glob("*.html"), None)

    if path is None:
        return None

    with path.open("r") as f:
        content = f.read()

    log = extract_log(content)
    if log is not None:
        return name, log
    else:
        return None


def retrieve_content(arg_elems: Sequence[str]):
    elems = (to_elem(e) for e in arg_elems)
    contents = (content for e in elems if (content := to_content(e)) is not None)

    return list(contents)


def parse_arguments() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument("output", help="Output file")
    parser.add_argument(
        "tabs", help="File to add as a tab, formatted as (name, directory)", nargs="+"
    )
    return parser.parse_args()


def run():
    args = parse_arguments()
    content = retrieve_content(args.tabs)
    document = format_document(content)

    output_path = Path(args.output)
    output_path.parent.mkdir(exist_ok=True, parents=True)

    with output_path.open("w") as f:
        f.write(document)


if __name__ == "__main__":
    run()
