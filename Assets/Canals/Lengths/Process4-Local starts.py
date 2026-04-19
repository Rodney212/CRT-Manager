import json

def process_geodata(input_file, output_file):
    try:
        with open(input_file, 'r') as f:
            data = json.load(f)

        for feature in data.get("features", []):
            geometry = feature.get("geometry", {})
            
            if geometry.get("type") == "LineString" and "coordinates" in geometry:
                coords = geometry["coordinates"]
                
                if not coords:
                    continue

                # Grab the anchor point
                start_x, start_y = coords[0]
                
                # Store the original anchor in properties
                feature["properties"]["startlocation"] = [start_x, start_y]

                # Re-map EVERY coordinate (including the first one) 
                # so that [start_x - start_x, start_y - start_y] becomes [0, 0]
                relative_coords = [
                    [round(x - start_x, 6), round(y - start_y, 6)] 
                    for x, y in coords
                ]
                
                geometry["coordinates"] = relative_coords

        with open(output_file, 'w') as f:
            json.dump(data, f, indent=4)
        
        print(f"Done! All features now start at [0, 0].")

    except Exception as e:
        print(f"Error: {e}")



if __name__ == "__main__":
    # 1. Added 'r' for raw string to handle backslashes
    # 2. Corrected input path to point to the .json file instead of the .py script
    input_path = r'Assets\Canals\Lengths\Processed3-LockData-Meter-NoArrays.json'
    output_path = r'Assets\Canals\Lengths\Processed4-LocalStarts-LockData-Meter-NoArrays.json'
    
    process_geodata(input_path, output_path)