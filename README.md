# SMEncounterRNGTool  [![Build status](https://ci.appveyor.com/api/projects/status/hv29i210qixas6kw?svg=true)](https://ci.appveyor.com/project/wwwwwwzx/smencounterrngtool)

**I will no longer be working on this repo. Checkout [3DSRNGTool](https://github.com/wwwwwwzx/3DSRNGTool) for a faster and more functional Tool.**

A tool for Pokemon Sun & Moon RNG (including ALL types of stationary and wild Pokemon). All button pressing and animation time delays were calibrated.  
Great thanks to  
**quan_dra** for original code  
**Real96** for good suggestions and testing the tool  
**Kaphotics** for discussion  
**RNGReporter** for multi-selection control  
**PKHex** for Pokemon personal table and wc7 format  
And ALL Pokemon RNG Researcher.

## Following Pokemon are supported:
- Catchable Legendary Pokemon in SM: Tapus, UBs, Cosmog, Solgaleo, Lunala, Zygarde
- Event Pokemon via Mystery Gift: Magearna, Munchlax, Machamp, lunar shiny Magikarp etc.
- Pokemon given by NPC: Type:Null, Porygon, Aerodactyl
- Any Wild Pokemon appear in SM
- Fossil Pokemon: Cranidos, Tirtouga, Archen, Shieldon
- Crabrawlers underneath Berry Trees
- Gift Eevee egg from Nursery helpers
- Main RNG eggs. i.e. eggs without shiny charm or Masuda Method
- Exeggutor attacking Lillie
- SM Starters: _PokeCalcNTR Only_
- Pokemon Caught by fishing (Unstable)
- SOS: _PokeCalcNTR Only (Unstable)_

## Usage
### With emtimer/EonTimer
1. Select the `Pokemon` you would like to RNG.  
  For Solgaleo/Lunala, you need to test the NPCs number first.  
  For wild pokemon, use the blank setting of `Pokemon`, select your `GameVersion`, `Location` and `Species`.
2. Find your `Seed` via the clock hands sequence on Continue Screen
3. Set up the `Filters` and click `Search`
4. Set your favorite spread as target frame, put the frame number in the second box of `Timing Range`.
5. Put your current frame into the first box of `Timing Range`. (The tool will automatically set this value if you are using the `Tool Kit` Tab)
6. Click `Calc` and get the timer setting, set the timer. 
7. Press A at the Continue Screen or press B to exit QR Scan, start the timer at the same time.
8. Start the dialogue until the [final screen](#final-screen-when-pressing-a), wait for the timer.
9. When the timer is done, click A at the same time, receive or battle with the Pokemon.
10. If the Pokemon is not the one you want, find the frame you actually hit, put it in the `Time Calculator`, calibrate the Pre-Timer and try again  
  ***Tips:*** For better results, please keep `"?" Frame Refinement` checked while searching.

### With PokeCalcNTR
1. Select the `Pokemon` you would like to RNG.
2. Start the game with PokeCalcNTR, click A until you get to the [final screen](#final-screen-when-pressing-a).
3. Pause at a `Safe Frame` (first only check `Safe F Only` to find safe frame zone)
4. Put your current frame number in the first box of `Frame Range` and check `Create Timeline`
5. If you  you are using honey and would like to find the timing of entering bag, please check `EnterBagTime`.
6. Click `Search`, you will see the game will follow timeline the tool created.
7. Now all the frames in the timeline are without `?` marks, they're all SAFE and ACCESSIBLE!!
8. Just change the `Filters` and make another search as usual.  
    ***Warning:*** Do NOT change the `Starting Frame` of timeline when you are following it, unless you set another `Safe Frame` ( "-" frames with `Create Timeline` unchecked) of your previous timeline as the `Starting Frame`.

## Final screen when pressing A
Usually it's the last 'A' before the battle starts, or the special BLACK dialogue box. 

- Tapus: Tapus' cry screen before the battle starts
- Zygarde: "Zygarde has gone into a Poke Ball!"
- Solgaleo/Lunala: No dialogue, stand in front of it
- UBs & wild Pokemon: "Use this item" (Honey) / Press A and enter the bag from X menu. 
- Type:Null, Porygon, Aerodactyl, Magearna, fossil Pokemon and event Pokemon: "You received xxx!"
- Crabrawler: "There was a Pokemon feeding on the Berries and it leaped out at you!"
- Gift Eevee egg: "You received Egg!". 
- Lillie: "Ahhh! What is that, xxx?!"
- Main RNG egg: "But you want the Egg your Pokemon was holding. right?" ( Check `-`->`Stationary`->`Main RNG Egg` )
- Starters: "Having accepted on another, you'll surely be friends for life".
- Fishing: When you get something on your hook (Unstable)
- SOS: When you "Use" Adrenaline Orbs (Unstable)

## Marks under Blink Column
- ★: One person on the map will blink soon, a warning for nearby frames.
- -: The safe frames can be 100% predicted.
### No NPC:
- 5: This frame will survive for 5/30s
- 30: This frame will survive for 1.0s
- 36: This frame will survive for 1.2s
### Multiple NPCs:
- ?: The spread might be affected by the history of NPC blink status so it's unsafe.  
  Using `"?" Frame Refinement` will get better prediction.

## Useful References
- [UB Rate(%) and No NPC Spots](parameters/UB%20Rate%20and%20Spots.md) 
- [Pre-Honey Correction](parameters/Pre-Honey%20Correction.md)
- [Event Pokemon Setting](parameters/Event%20Pokemon%20Setting.md) 
- [Gen VII Events Contribution Thread](https://projectpokemon.org/forums/forums/topic/39400-gen-vii-events-contribution-thread/) 
