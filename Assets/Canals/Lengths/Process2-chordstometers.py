
# 1. Load the original data


import json
import math

def lat_lon_to_meters(lon, lat):
    r_major = 6378137.0
    x = r_major * math.radians(lon)
    # Standard Web Mercator Y calculation
    y = math.log(math.tan((90 + lat) * math.pi / 360)) * r_major
    return x, y

STANDARD_LAT = 53.503281892469303
STANDARD_LON = -2.17143763948923

input_path = r'Assets\Canals\Lengths\Processed1-NoArrays.json'
output_path = r'Assets\Canals\Lengths\Processed2-Meter-NoArrays.json'

with open(input_path, 'r') as f:
    data = json.load(f)

origin_x, origin_y = lat_lon_to_meters(STANDARD_LON, STANDARD_LAT)

def convert_coord_pair(pt):
    """Applies the math to a single [lon, lat] list."""
    mx, my = lat_lon_to_meters(pt[0], pt[1])
    return [round(mx - origin_x, 3), round(my - origin_y, 3)]

for feature in data['features']:
    geom = feature['geometry']
    g_type = geom['type']
    coords = geom['coordinates']

    if g_type == 'Point':
        geom['coordinates'] = convert_coord_pair(coords)

    elif g_type == 'LineString' or g_type == 'MultiPoint':
        # Single level of nesting: [pt, pt, pt]
        geom['coordinates'] = [convert_coord_pair(p) for p in coords]

    elif g_type == 'MultiLineString' or g_type == 'MultiPolygon':
        # Two levels of nesting: [[pt, pt], [pt, pt]]
        geom['coordinates'] = [[convert_coord_pair(p) for p in line] for line in coords]

    elif g_type == 'MultiPolygon':
        # Three levels of nesting (if needed for complex shapes)
        geom['coordinates'] = [[[convert_coord_pair(p) for p in ring] for ring in poly] for poly in coords]

with open(output_path, 'w') as f:
    json.dump(data, f, indent=4)

print("Conversion complete. Coordinates transformed to meters; structure preserved.")