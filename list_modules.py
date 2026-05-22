import json
import os

path = os.path.expanduser('~/Documents/GeneralsEditor/Project/Schema/infantry.schema.json')
with open(path, 'r') as f:
    data = json.load(f)
    modules = data.get('modules', {})
    print("Modules found:")
    for m in sorted(modules.keys()):
        print(f"- {m}")
