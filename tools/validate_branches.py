import subprocess
import re

import argparse
import requests


def retrieve_branches() -> list:
    git_cmd = 'git branch -r'

    p = subprocess.run(git_cmd, shell=True, stdout=subprocess.PIPE)
    branches = p.stdout.decode('utf-8').splitlines()

    return (b[9:] for b in branches)


project_keys = ['D3DFMIQ', 'DSFRAME']
issue_group_key = 'issue'


def get_issue_regex():
    integer_regex = r'(0|([1-9]\d*))'
    project_regexes = (f'({proj_key}-{integer_regex})' for proj_key in project_keys)
    issue_regex = "|".join(project_regexes)

    regex_str = fr"^.*(?P<{issue_group_key}>{issue_regex}).*$"

    return re.compile(regex_str)


def service_message(msg: str):
    print(f'##teamcity[{msg}]')


def start_test_suite():
    service_message(f"testSuiteStarted name='Branch merge validation'")


def end_test_suite():
    service_message(f"testSuiteFinished name='Branch merge validation'")


def start_test(branch: str):
    service_message(f"testStarted name='{branch}'")


def fail_test(branch: str, reason: str):
    service_message(f"testFailed name='{branch}' message='{reason}")


def end_test(branch: str):
    service_message(f"testFinished name='{branch}'")


def ignore_test(branch: str):
    service_message(f"testIgnored name='{branch}' message='No issue associated with {branch}.'")


payload = {'fields': 'status'}
issue_url = 'https://issuetracker.deltares.nl/rest/api/2/issue/'

def validate_branch(issue: str, branch: str, user: str, password: str):
    r = requests.get(f"{issue_url}{issue}", auth=(user, password), params=payload)

    if not (r.status_code == 200):
        fail_test(branch, f'Could not retrieve issue {issue} from JIRA.')
        return

    val = r.json()
    status = val['fields']['status']['name']

    if status == "Closed":
        fail_test(branch, f"Issue {issue} is 'Closed' but {branch} still exists.")


def validate_branches(branches: list, username: str, password: str) -> tuple:
    start_test_suite()

    regex = get_issue_regex()

    for b in branches:
        m = regex.match(b)

        if (m):
            start_test(b)
            issue = m.group(issue_group_key)
            validate_branch(issue, b, username, password)
            end_test(b)
        else:
            ignore_test(b)

    end_test_suite()


def parse_arguments():
    """
    Parse the arguments with which this script was called through
    argparse.ArgumentParser
    """
    parser = argparse.ArgumentParser()

    parser.add_argument("username", help="JIRA Username")
    parser.add_argument("password", help="JIRA password")

    return parser.parse_args()


if __name__ == "__main__":
    args = parse_arguments()

    branches = retrieve_branches()
    validate_branches(branches, args.username, args.password)