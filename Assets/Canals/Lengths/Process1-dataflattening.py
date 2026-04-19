import json

# Input/Output paths
input_path = r'Assets\Canals\Lengths\Canal_And_River_Trust_Canals_By_KM_Length_View.geojson'
output_path = r'Assets\Canals\Lengths\Processed1-NoArrays.json'

with open(input_path, 'r') as f:
    data = json.load(f)

new_features = []

for feature in data['features']:
    geom = feature.get('geometry')
    if not geom:
        continue
        
    g_type = geom['type']
    props = feature.get('properties', {})
    
    # Split MultiLineString into multiple LineStrings
    if g_type == 'MultiLineString':
        for i, line in enumerate(geom['coordinates'], 1):
            new_feat = json.loads(json.dumps(feature)) # Deep copy
            new_feat['geometry']['type'] = 'LineString'
            new_feat['geometry']['coordinates'] = line
            new_feat['properties']['functionallocation'] = f"{props.get('functionallocation', 'Unknown')} Part {i}"
            new_features.append(new_feat)

    # Split MultiPoint into multiple Points
    elif g_type == 'MultiPoint':
        for i, pt in enumerate(geom['coordinates'], 1):
            new_feat = json.loads(json.dumps(feature)) # Deep copy
            new_feat['geometry']['type'] = 'Point'
            new_feat['geometry']['coordinates'] = pt
            new_feat['properties']['functionallocation'] = f"{props.get('functionallocation', 'Unknown')} Part {i}"
            new_features.append(new_feat)

    # Keep standard types as they are
    elif g_type in ['LineString', 'Point']:
        new_features.append(feature)

# Update the feature list and save
data['features'] = new_features

with open(output_path, 'w') as f:
    json.dump(data, f, indent=4)

print(f"Flattening complete. Created {len(new_features)} features.")