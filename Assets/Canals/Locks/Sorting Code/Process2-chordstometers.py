import json
import math

def lat_lon_to_meters(lon, lat):
    r_major = 6378137.0
    x = r_major * math.radians(lon)
    # Standard Web Mercator Y calculation
    y = math.log(math.tan((90 + lat) * math.pi / 360)) * r_major
    return x, y

# Origin point (Locks 68 Tannersfield Lowest - as per your first entry)
STANDARD_LAT = 53.503281892469303
STANDARD_LON = -2.17143763948923

input_path = r'Assets\Canals\Locks\Sorting Code\Canal_And_River_Trust_Locks.geojson'
output_path = r'Assets\Canals\Locks\Sorting Code\inMeters.json'

with open(input_path, 'r') as f:
    data = json.load(f)

origin_x, origin_y = lat_lon_to_meters(STANDARD_LON, STANDARD_LAT)

def convert_coord_pair(pt):
    """Converts [lon, lat] to Unity-friendly [x, z] meters relative to origin."""
    mx, my = lat_lon_to_meters(pt[0], pt[1])
    # Swap Y to Z for Unity ground plane
    return [round(mx - origin_x, 3), round(my - origin_y, 3)]

for feature in data['features']:
    geom = feature['geometry']
    g_type = geom['type']
    coords = geom['coordinates']

    if g_type == 'Point':
        geom['coordinates'] = convert_coord_pair(coords)

    elif g_type in ['LineString', 'MultiPoint']:
        geom['coordinates'] = [convert_coord_pair(p) for p in coords]

    elif g_type == 'MultiLineString':
        geom['coordinates'] = [[convert_coord_pair(p) for p in line] for line in coords]

with open(output_path, 'w') as f:
    json.dump(data, f, indent=4)

print(f"Done! Processed {len(data['features'])} locks.")
print(f"Origin (0,0,0) set to: {STANDARD_LON}, {STANDARD_LAT}")