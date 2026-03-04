import os
import re

# this function takes a standard tailwind class and returns a version that includes a dark mode variant
def intelligent_dark_mapper(match):
    prefix = match.group(1)   # e.g., 'bg-taupe-50'
    opacity = match.group(2) or "" 
    
    if prefix == 'bg-white' or prefix == 'bg-taupe-50' or prefix == 'bg-slate-50' or prefix == 'bg-gray-50':
        dark_val = 'bg-[#1A1314]'
    elif prefix == 'bg-taupe-100' or prefix == 'bg-slate-100':
        dark_val = 'bg-[#201819]'
    elif prefix in ['bg-taupe-200', 'border-taupe-50', 'border-taupe-100', 'border-taupe-200', 'border-white', 'border-slate-100', 'border-slate-200']:
        if prefix == 'border-white':
            dark_val = 'border-taupe-800'
        else:
            p_parts = prefix.split('-')
            dark_val = f"{p_parts[0]}-taupe-800"
    elif prefix in ['text-taupe-900', 'text-taupe-800', 'text-slate-900', 'text-[#0f172a]']:
        dark_val = 'text-white'
    elif prefix in ['text-taupe-700', 'text-slate-700']:
        dark_val = 'text-taupe-300'
    elif prefix in ['text-taupe-600', 'text-slate-600']:
        dark_val = 'text-taupe-400'
    elif prefix == 'bg-taupe-800':
        dark_val = 'bg-taupe-200'
    else:
        return match.group(0) # Unchanged

    return f"{prefix}{opacity} dark:{dark_val}{opacity}"

# this function scans the text and removes any old or broken dark mode classes to start fresh
def remove_existing_dark_classes_and_garbage(content):
    # Remove any existing dark: variants to prevent duplicates/mess
    c1 = re.sub(r'\s*dark:[a-zA-Z0-9\-\[\]\#\/\%]+None', '', content)
    c1 = re.sub(r'\s*dark:[a-zA-Z0-9\-\[\]\#\/\%]+', '', c1)
    # Remove literal "None" sticking to the end of classes
    c1 = re.sub(r'None(?=[\s\'"])', '', c1)
    # Remove None sticking anywhere in classes
    c1 = c1.replace('None', '')
    return c1

# this is the main function that opens a file and applies all the dark mode transformations to the content
def process_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # Step 1: Clean
    clean_content = remove_existing_dark_classes_and_garbage(content)

    def replacer(match):
        pre = match.group(1)
        classes = match.group(2)
        post = match.group(3)
        
        words = re.split(r'(\s+|(?<=\')|(?=\'))', classes)
        
        new_words = []
        for w in words:
            if not w.strip() or w == "'":
                new_words.append(w)
                continue
            
            w_mapped = re.sub(
                r'^(bg-white|bg-taupe-50|bg-slate-50|bg-gray-50|bg-taupe-100|bg-slate-100|bg-taupe-200|border-taupe-50|border-taupe-100|border-taupe-200|border-white|border-slate-100|border-slate-200|text-taupe-900|text-taupe-800|text-slate-900|text-\[\#0f172a\]|text-taupe-700|text-slate-700|text-taupe-600|text-slate-600|bg-taupe-800)(\/[0-9]+)?$',
                intelligent_dark_mapper,
                w
            )
            new_words.append(w_mapped)
            
        return pre + "".join(new_words) + post

    new_content = re.sub(r'(class=")([^"]+)(")', replacer, clean_content)
    new_content = re.sub(r"(class=')([^']+)(')", replacer, new_content)
    new_content = re.sub(r'(routerLinkActive=")([^"]+)(")', replacer, new_content)
    new_content = re.sub(r"(')(bg-[^']+|text-[^']+|border-[^']+)(')", replacer, new_content)

    if new_content != content:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(new_content)
        print(f"Updated {filepath}")

# this final block searches through all the folders for html files and runs the processing logic on each one
for root, _, files in os.walk('c:/Users/rithw/OneDrive/Desktop/Training/capstone/CommercialInsuranceFrontend/src/app'):
    for file in files:
        if file.endswith('.html'):
            process_file(os.path.join(root, file))
