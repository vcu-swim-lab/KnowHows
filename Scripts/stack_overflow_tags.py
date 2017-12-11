#!/usr/bin/python3
# coding: utf-8

# Usage: read_posts.py [document]
# Creates language dictionaries based on tags and inline one-word code blocks on Stack Overflow.
# To be used with documents from the Stack Exchange data dump (https://archive.org/details/stackexchange)

from lxml import etree
import collections
import re
import sys

LANGUAGES = ["java", "android", "python", "python-3.x", "go", "javascript", "angularjs", "node.js", "reactjs", "typescript", "jquery" "c", "c++", "c#", ".net", "swift", "ios", "perl"]
TAGS_RE = re.compile("\<([a-z0-9\.\#\-]+)\>")
CODE_RE = re.compile("\<code\>(\w+)\<\/code\>")
THRESHOLD = 1

terms = collections.defaultdict(list)


def add_terms(language, code_block):
    '''Add terms to the specified language in the dictionary based on code blocks.'''

    if not language in terms:
        terms[language] = []

    for term in re.findall(CODE_RE, code_block):
        terms[language].append(term)


def count_terms(terms, language):
    '''Count collected terms for the specified language.'''

    return collections.Counter(terms[language])


def tag_in_list(tags):
    '''Returns the first tag found in the languages list.'''

    for tag in re.findall(TAGS_RE, tags):
        if tag in LANGUAGES:
            return tag

    return None


def dict_to_file(filename, filtered_terms):
    '''Write language dictionary to file.'''

    with open(filename, "w", encoding="utf-8") as file:
        for term in filtered_terms:
            file.write("{}\n".format(term))


if(len(sys.argv) < 2):
    sys.exit("Error: Must provide a document for parsing")

for event, element in etree.iterparse(sys.argv[1], events=["end"]):
    tag = tag_in_list(str(element.get("Tags")))
    if tag:
        add_terms(tag, str(element.get("Body")))
    element.clear()
    while element.getprevious() is not None:
        del element.getparent()[0]

for language in terms:
    counted = count_terms(terms, language)
    filtered = {module : counted[module] for module in counted if counted[module] >= THRESHOLD }
    dict_to_file("{}.txt".format(language), filtered)
