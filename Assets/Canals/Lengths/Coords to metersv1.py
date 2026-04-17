import json
import math

def lat_lon_to_meters(lon, lat):
    """Converts Lat/Lon to Web Mercator meters (EPSG:3857)"""
    r_major = 6378137.0
    x = r_major * math.radians(lon)
    scale = x / lon if lon != 0 else r_major
    y = 180.0 / math.pi * math.log(math.tan(math.pi / 4.0 + lat * (math.pi / 180.0) / 2.0)) * scale
    return x, y

# 1. Load the original data
input_path = r'Assets\Canals\Lengths\Canal_And_River_Trust_Canals_By_KM_Length_View.geojson'
output_path = r'Assets\Canals\Lengths\meterlengths.json'

with open(input_path, 'r') as f:
    data = json.load(f)

# Optional: Set the first point of the first lock as (0,0,0) for Unity stability
first_coords = data['features'][0]['geometry']['coordinates'][0]
origin_x, origin_y = lat_lon_to_meters(first_coords[0], first_coords[1])

# 2. Process every feature
for feature in data['features']:
    geometry = feature['geometry']
    
    if geometry['type'] == 'LineString':
        new_coords = []
        for lon, lat in geometry['coordinates']:
            # Convert to global meters
            mx, my = lat_lon_to_meters(lon, lat)
            
            # Convert to local Unity meters (relative to origin)
            # We map Y to Z because in Unity, Z is the ground plane depth
            local_x = round(mx - origin_x, 3)
            local_z = round(my - origin_y, 3)
            
            new_coords.append([local_x, local_z])
        
        # Update the geometry in the dictionary
        geometry['coordinates'] = new_coords

# 3. Write to the new file
with open(output_path, 'w') as f:
    json.dump(data, f, indent=4)

print(f"Done! Created {output_path}")
print(f"All locations are now in meters relative to the first lock.")