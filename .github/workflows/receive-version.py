import re
import os
import glob

### reads the application-version number from the windows build and writes it into github enviornment variable "Release_Name"

# read version from csproj
filePath = glob.glob("**/Markdown2Pdf.csproj")[0]

print(f"Reading Version from '{filePath}'")

with open(filePath, "r") as f:
    data = f.read()

# parse version out of the file
def parse(data=data):
    output = re.search('<Version>(?P<Version>(.*?))</Version>', data, flags=re.X)
    return output.group('Version')

versionName = parse()

tagName = f"v{versionName}" 
releaseName = f"Version {versionName}"

print(f"Tag_Name:     {tagName}")
print(f"Release_Name: {releaseName}")
print(f"Nuget_Version: {versionName}")

# set github enviorment variable
env_file = os.getenv('GITHUB_ENV')

with open(env_file, "a") as myfile:
    myfile.write(f"Tag_Name={tagName}\r\n")
    myfile.write(f"Release_Name={releaseName}\r\n")
    myfile.write(f"Nuget_Version={versionName}")