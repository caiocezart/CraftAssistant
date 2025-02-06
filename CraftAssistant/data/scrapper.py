import json
import requests

custom_base_groups = {
    "Gloves (DEX)": "glove_dex_armour",
    "Gloves (DEX/INT)": "glove_dex_int_armour",
    "Gloves (INT)": "glove_int_armour",
    "Gloves (STR)": "glove_str_armour",
    "Gloves (STR/DEX)": "glove_str_dex_armour",
    "Gloves (STR/INT)": "glove_str_int_armour",
    "Helmet (DEX)": "helmet_dex_armour",
    "Helmet (DEX/INT)": "helmet_dex_int_armour",
    "Helmet (INT)": "helmet_int_armour",
    "Helmet (STR)": "helmet_str_armour",
    "Helmet (STR/DEX)": "helmet_str_dex_armour",
    "Helmet (STR/INT)": "helmet_str_int_armour",
    "Shield (DEX)": "shield_dex_armour",
    "Shield (STR)": "shield_str_armour",
    "Shield (STR/DEX)": "shield_str_dex_armour",
    "Shield (STR/INT)": "shield_str_int_armour",
    "Body Armour (DEX)": "body_armour_dex_armour",
    "Body Armour (DEX/INT)": "body_armour_dex_int_armour",
    "Body Armour (INT)": "body_armour_int_armour",
    "Body Armour (STR)": "body_armour_str_armour",
    "Body Armour (STR/DEX)": "body_armour_str_dex_armour",
    "Body Armour (STR/INT)": "body_armour_str_int_armour",
    "Boots (DEX)": "boot_dex_armour",
    "Boots (DEX/INT)": "boot_dex_int_armour",
    "Boots (INT)": "boot_int_armour",
    "Boots (STR)": "boot_str_armour",
    "Boots (STR/DEX)": "boot_str_dex_armour",
    "Boots (STR/INT)": "boot_str_int_armour"
}
def fetch_json(url):
    headers = {
        "sec-ch-ua": '"Not A(Brand";v="8", "Chromium";v="132", "Google Chrome";v="132"',
        "sec-ch-ua-mobile": "?0",
        "sec-ch-ua-platform": '"Windows"',
    }
    response = requests.get(url, headers=headers)

    if not response.ok:
        raise Exception(f"Failed to fetch data from {url}: HTTP {response.status_code}")

    raw_text = response.text.strip()

    if url.endswith("poec_data.json?v=1736871389"):
        raw_text = raw_text.lstrip("poecd=").strip()
    elif url.endswith("poec_lang.us.json?v=1736871389"):
        raw_text = raw_text.lstrip("poecl=").strip()

    return json.loads(raw_text)

def parse_base_groups(poec_lang):
    base_groups = {}
    for base_id, base_name in poec_lang["base"].items():
        base_groups[base_id] = {
            "base_group": custom_base_groups[base_name] if base_name in custom_base_groups else base_name,
            "items": [],
            "prefixes": [],
            "suffixes": []
        }
    return base_groups

def parse_items(poec_data, poec_lang, base_groups):
    for item in poec_data["bitems"]["seq"]:
        base_id = item["id_base"]
        if base_id in base_groups:
            base_groups[base_id]["items"].append(poec_lang["bitem"].get(item["id_bitem"], "Unknown Item"))

def parse_bases_and_groups(poec_data, poec_lang, base_groups):
    for base in poec_data["bases"]["seq"]:
        base_id = base["id_base"]
        if base_id in base_groups:
            base_groups[base_id]["base_type"] = base.get("base_type", "unknown")
            base_groups[base_id]["bgroup"] = poec_lang["bgroup"].get(base["id_bgroup"], "Unknown Group")

def parse_modifiers_and_tiers(poec_data, poec_lang, base_groups):
    for base_id, modifier_ids in poec_data["basemods"].items():
        if base_id in base_groups:
            base_group = base_groups[base_id]

            for modifier_id in modifier_ids:
                modifier_data = next((m for m in poec_data["modifiers"]["seq"] if m["id_modifier"] == modifier_id), None)
                if modifier_data:
                    modifier_name = modifier_data["name_modifier"]
                    affix_type = modifier_data["affix"]
                    mod_groups = json.loads(modifier_data["modgroups"])
                    mod_groups = [modgroup.lower().strip() for modgroup in mod_groups]

                    tiers = poec_data["tiers"].get(modifier_id, {})
                    tier_list = []

                    for base_type, tier_values in tiers.items():
                        if base_type == base_id:
                            for tier in tier_values:
                                is_float_affix = False
                                nvalues = json.loads(tier["nvalues"])
                                for value in nvalues:
                                    if isinstance(value, list):
                                        if isinstance(value[0], float) or isinstance(value[1], float):
                                            is_float_affix = True
                                            break
                                    else:
                                        if isinstance(value, float):
                                            is_float_affix = True
                                            break
                                            
                                                 
                            for tier in tier_values:
                                try:
                                    ilvl = int(tier["ilvl"])
                                    weighting = int(tier["weighting"])
                                    nvalues = json.loads(tier["nvalues"])

                                    if weighting > 1:  # Exclude invalid tiers
                                        tier_data = {
                                            "tier": f"T{len(tier_values) - tier_values.index(tier)}",
                                            "ilvl": ilvl,
                                            "weighting": weighting,
                                            "values": []
                                        }
                                        if isinstance(nvalues, list):
                                            for value in nvalues:
                                                tier_data["float"] = False
                                                if isinstance(value, list):
                                                    if is_float_affix:
                                                        value[0] = parse_value(value[0])
                                                        value[1] = parse_value(value[1])
                                                        tier_data["float"] = True
                                                        
                                                    tier_data["values"].append({
                                                        "Min": value[0], 
                                                        "Max": value[1]
                                                    })
                                                else:
                                                    if is_float_affix:
                                                        value = parse_value(value)
                                                        
                                                    tier_data["values"].append({
                                                        "Min": value,
                                                        "Max": value
                                                    })
                                        tier_list.append(tier_data)
                                except (ValueError, json.JSONDecodeError):
                                    continue

                    affix = {
                        "description": modifier_name,
                        "mod_groups": mod_groups,
                        "tiers": tier_list
                    }

                    if affix_type == "prefix":
                        base_group["prefixes"].append(affix)
                    elif affix_type == "suffix":
                        base_group["suffixes"].append(affix)

# scale up value to a factor of 60 to make it integers
def parse_value(value):
    return int(float(value) * 60)
    
    # try:
    #     if value.is_float():
    #         return int(value) * 60
    #     
    #     return value
    # except ValueError:
    #     return value
    
def calculate_percentages(base_groups):
    for base_group in base_groups.values():
        total_prefix_weight = sum(
            tier["weighting"] for prefix in base_group.get("prefixes", []) for tier in prefix["tiers"]
        )
        total_suffix_weight = sum(
            tier["weighting"] for suffix in base_group.get("suffixes", []) for tier in suffix["tiers"]
        )
        total_base_weight = total_prefix_weight + total_suffix_weight

        for affix_type, total_weight, affix_percent_field in [
            ("prefixes", total_prefix_weight, "prefix_percent"),
            ("suffixes", total_suffix_weight, "suffix_percent"),
        ]:
            for affix in base_group.get(affix_type, []):
                for tier in affix["tiers"]:
                    tier_weight = tier.get("weighting", 0)

                    affix_percent = round((tier_weight / total_weight) * 100, 4) if total_weight > 0 else 0
                    weight_percent = round((tier_weight / total_base_weight) * 100, 4) if total_base_weight > 0 else 0

                    tier["affix_percent"] = affix_percent
                    tier["weight_percent"] = weight_percent

def consolidate_data(poec_data_url, poec_lang_url, output_path):
    poec_data = fetch_json(poec_data_url)
    poec_lang = fetch_json(poec_lang_url)

    base_groups = parse_base_groups(poec_lang)
    parse_items(poec_data, poec_lang, base_groups)
    parse_bases_and_groups(poec_data, poec_lang, base_groups)
    parse_modifiers_and_tiers(poec_data, poec_lang, base_groups)
    calculate_percentages(base_groups)

    consolidated_data = {"base_groups": list(base_groups.values())}

    with open(output_path, "w", encoding="utf-8") as file:
        json.dump(consolidated_data, file, indent=4, ensure_ascii=False)

    print(f"Consolidated JSON saved to {output_path}")

poec_data_url = "https://www.craftofexile.com/json/poe2/main/poec_data.json?v=1736871389"
poec_lang_url = "https://www.craftofexile.com/json/poe2/lang/poec_lang.us.json?v=1736871389"
output_path = "poe.json"
consolidate_data(poec_data_url, poec_lang_url, output_path)
