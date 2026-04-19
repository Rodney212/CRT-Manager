import json
import os

# Define paths

input_path = r'Assets\Canals\Lengths\Processed4-LocalStarts-LockData-Meter-NoArrays.json'
output_path = r'Assets\Canals\Lengths\test1'
print("line7")
def split_waterway_data():
    # 1. Load the original data
    try:
        with open(input_path, 'r', encoding='utf-8') as f:
            data = json.load(f)
    except FileNotFoundError:
        print(f"Error: Could not find {input_path}")
        return

    # Check if it's a FeatureCollection or a simple list
    features = data.get('features', data) if isinstance(data, dict) else data
    print("line 19")

    # 2. Sort and group by waterway_name
    waterway_groups = {}
    
    for feature in features:
        # Navigate to the waterway_name property
        name = feature.get('properties', {}).get('name')
        
        if name:
            # Clean name for filename (remove slashes or dots)
            safe_name = "".join([c for c in name if c.isalnum() or c in (' ', '-', '_')]).strip()
            
            if safe_name not in waterway_groups:
                waterway_groups[safe_name] = []
            
            waterway_groups[safe_name].append(feature)

    # 3. Output new JSON files
    for waterway, items in waterway_groups.items():
        file_name = f"{waterway}.json"
        save_path = os.path.join(output_path, file_name)
        
        with open(save_path, 'w', encoding='utf-8') as out_file:
            # Saving as a list of features; wrap in a dict if you need FeatureCollection format
            json.dump(items, out_file, indent=4)
            
        print(f"Generated: {file_name}")



if __name__ == "__main__":
    split_waterway_data()