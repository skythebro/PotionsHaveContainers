# 🫗 Potions Have Containers - Gloomrot Update
forked from [phillipsOG/C2ierce](https://github.com/phillipsOG/PotionsHaveContainers) and updated to , now also updated to 1.0.

## Potions and flasks will now return their empty counter-part containers. What were we doing before, eating them?
* Works with all potions, flasks/Canteens.
* NEW Blood bottle and blood merlot will now return a blood bottle.
* Because of the abilitygroup of the water bottle and canteen being the same they can both only give back one or the other so I added a config option to choose which one you want.
* The best option would be to not use a water canteen and use the water bottles instead. (it is ofcourse your choice :D)
* 
If there is a non water related potion I have missed be sure to open an issue on [github](https://github.com/skythebro/PotionsHaveContainers/issues).

## Installation
Place the _PotionsHaveContainers.dll_ file inside of (Vrising Server)\BepInEx\plugins folder.

## Configurable Values
```ini
[Config]

## Enable or Disable PotionsHaveContainers
# Setting type: Boolean
# Default value: true
Enable Mod = true

## Always give canteens(false) or always give bottles(true) for both water containers.
# Setting type: Boolean
# Default value: false
Give Bottles = false

```

## Developer & credits
<details>

### V rising modding [Discord](https://discord.gg/XY5bNtNm4w)
### Current Developer
- `skythebro/skyKDG` - Also known as realsky on discord

### Original Creator & Developers
- `phillipsOG`
- `C2ierce`

</details>