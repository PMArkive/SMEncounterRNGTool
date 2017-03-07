# SMEncounterRNGTool
A tool for Pokemon Sun & Moon RNG (including stationary and wild Pokemon). <br>
Great thanks to:<br>
@quan_dra: Whom I borrowed some code from<br>
@Real96: Who give me good suggestions and help me test the tool<br>
And ALL Pokemon RNG Researcher<br>

## Following Pokemon are supported:
- Catchable Legendary Pokemon in SM: Tapus, UBs, Cosmog, Solgaleo, Lunala, Zygarde
- Event Pokemon via Mystery Gift: Magearna, Munchlax, Machamp, lunar shiny Magikarp etc.
- Pokemon given by NPC: Type:Null, Porygon, Aerodactyl
- Any Wild Pokemon appear in SM
- Fossil Pokemon: Cranidos, Tirtouga, Archen, Shieldon
- Crabrawlers underneath Berry Trees
- SM Starters: _PokeCalcNTR Only, select `<Fossil>`, change `# of NPCs` to 5. (This function will not be added)_

## Usage
### With emtimer/EonTimer
1. Select the `Pokemon` you would like to RNG.
2. Find your `Seed` via the clock hands sequence on Continue Screen
3. Set up the `Filters` and click `Search`
4. Set your favorite spread as target frame, put the frame number in the second box of `Timing Range`.
5. Put your current frame into the first box of `Timing Range`. (The tool will automatically set this value when you are using the `Tool Kit` Tab)
6. Click `Calc` and get the timer setting, set the timer. 
7. Press A at the Continue Screen or press B to exit QR Scan, start the timer at the same time.
8. Start the dialogue until the [final screen](#final-screen-when-pressing-a), wait for the timer.
9. When the timer is done, click A at the same time, receive or battle with the Pokemon.
10. If the Pokemon is not the one you want, calibrate the Pre-Timer and try again

### With PokeCalcNTR
 1. Select the `Pokemon` you would like to RNG.
 2. Start the game with PokeCalcNTR, click A until you get to the [final screen](#final-screen-when-pressing-a).
 3. Pause at a `Safe Frame` (first only check `Safe F Only` to find safe frame zone)
 4. Put your current frame number in the first box of `Frame Range` and check `Create Timeline`
 5. Click `Search`, you will see the game will follow timeline the tool created.
 6. Now all the frames in the timeline are without `?` marks, they're all SAFE and ACCESSIBLE!!
 7. Just change the `Filters` and make another search as usual.
 Warning: Do NOT change the `Starting Frame` of timeline when you are following it.

## Final screen when pressing A
Usually it's the last 'A' before the battle starts, or the special BLACK dialogue box.<br>
(All button pressing and animation time delays were calibrated. Especially for those who are using PokeCalcNTR.)
- Tapus: Tapus' cry screen before the battle starts
- Zygarde: "Zygarde has gone into a Poke Ball!"
- Solgaleo/Lunala: No dialogue, stand in front of it
- UBs & wild Pokemon: "Use this item" (Honey). If you would like to find the timing of entering bag, please add 6*(NPC+1) to `Correction`.
- Type:Null, Porygon, Aerodactyl, Magearna, fossil Pokemon and event Pokemon: "You received xxx!"
- Crabrawler: "There was a Pokemon feeding on the Berries and it leaped out at you!"
- Starters: "Having accepted on another, you'll surely be friends for life"

## Marks under Blink Column
- ★: One person on the map will blink soon, a warning for nearby frames.
- ?: The spread might be affected by the history of NPC status so it's unsafe. 
- -: The safe frames can be 100% predicted.

## Some Parameters:
- [UB Rate(%)](parameters/UB Rate.md)
- [Pre-Honey Correction](parameters/Pre-Honey Correction.md)
- [Event Pokemon Setting](parameters/Event Pokemon Setting.md)