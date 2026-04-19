import json

# Load your data files
# Replace 'canals.json' and 'locks.json' with your actual filenames
with open(r'Assets/Canals/Lengths/Processed2-Meter-NoArrays.json', 'r') as f:
    canal_data = json.load(f)

with open(r'Assets\Canals\Locks\Sorting Code\Canal_And_River_Trust_Locks.geojson', 'r') as f:
    lock_data = json.load(f)

# 1. Create a dictionary to count locks per segment
# Key will be the 'sapcanalcode-km' (e.g., "RD-046")
lock_counts = {}

# Handle both single features or a FeatureCollection for the locks file
features = lock_data.get('features', lock_data)
if not isinstance(features, list):
    features = [lock_data] # If it's just a single object

for lock in features:
    loc_ref = lock['properties'].get('sap_func_loc', "")
    
    if loc_ref:
        # Split RD-046-003 by '-' and take the first two parts: RD-046
        parts = loc_ref.split('-')
        if len(parts) >= 2:
            segment_key = f"{parts[0]}-{parts[1]}"
            
            lock_counts[segment_key] = lock_counts.get(segment_key, 0) + 1

# 2. Inject counts into the canal segments
for canal in canal_data['features']:
    # Get the reference (e.g., "AB-001")
    canal_ref = canal['properties'].get('functionallocation', "")
    
    # Match and add the quantity (default to 0 if no locks found)
    canal['properties']['lock_quantity'] = lock_counts.get(canal_ref, 0)

# 3. Export the enriched data
with open(r'Assets\Canals\Lengths\Processed3-LockData-Meter-NoArrays.json', 'w') as f:
    json.dump(canal_data, f, indent=4)

print("Processing complete. File saved as 'enriched_canals.json'")