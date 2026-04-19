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

#For Locks
#input_path = r'Assets\Canals\Locks\Sorting Code\Canal_And_River_Trust_Locks.geojson'
#output_path = r'Assets\Canals\Locks\Sorting Code\inMeters.json'
#For Canals
input_path = r'Assets\Canals\Lengths\Canal_And_River_Trust_Canals_By_KM_Length_View.geojson'
output_path = r'Assets\Canals\Lengths\By KM-Meters-NoArrays.json'

with open(input_path, 'r') as f:
    data = json.load(f)

origin_x, origin_y = lat_lon_to_meters(STANDARD_LON, STANDARD_LAT)

def convert_coord_pair(pt):
    """Applies the math to a single [lon, lat] list."""
    mx, my = lat_lon_to_meters(pt[0], pt[1])
    return [round(mx - origin_x, 3), round(my - origin_y, 3)]

new_features = []

for feature in data['features']:
    geom = feature['geometry']
    g_type = geom['type']
    props = feature.get('properties', {})
    
    # Check if it's a 'Multi' type that needs splitting
    if g_type == 'MultiLineString':
        # geom['coordinates'] is [[[lon, lat], ...], [[lon, lat], ...]]
        for i, line in enumerate(geom['coordinates'], 1):
            new_feat = json.loads(json.dumps(feature))
            new_feat['geometry']['type'] = 'LineString'
            # Map the math across the nested list of points
           
            new_feat['properties']['name'] = f"{props.get('name', 'Unknown')} Part {i}"
            new_feat['geometry']['coordinates'] = [convert_coord_pair(p) for p in line]
            new_features.append(new_feat)
            

    elif g_type == 'MultiPoint':
        # geom['coordinates'] is [[lon, lat], [lon, lat]]
        for i, pt in enumerate(geom['coordinates'], 1):
            new_feat = json.loads(json.dumps(feature))
            new_feat['geometry']['type'] = 'Point'
            new_feat['geometry']['coordinates'] = convert_coord_pair(pt)
            new_feat['properties']['name'] = f"{props.get('name', 'Unknown')} Part {i}"
            new_features.append(new_feat)

    elif g_type == 'LineString':
        geom['coordinates'] = [convert_coord_pair(p) for p in geom['coordinates']]
        new_features.append(feature)

    elif g_type == 'Point':
        geom['coordinates'] = convert_coord_pair(geom['coordinates'])
        new_features.append(feature)

data['features'] = new_features

with open(output_path, 'w') as f:
    json.dump(data, f, indent=4)

print("Conversion and splitting complete.")