# City Names
Casual city name generator using a basic markov generator which can take variable-length strings as context. These generators are grouped by biome, so that it generates desert city names based on the names of real-life cities in deserts, for example.

# Refocus
This branch refocuses on the original goals, because i got too caught up in meta stuff previously.

## Goal
citynames is an implementation of a city name generator, which takes information about a city's location (i.e. biome, distance from coast, elevation) and outputs a plausible name for a city with those parameters. Any parameter can be omitted from the query. The intention is to use this (via a compatibility layer) to generate names for Minecraft cities.

## Methodology
The refocused model will use a Markov character generator, as that has been successful in previous implementations. It will first generate a prior model, which takes the previous *`n`* characters from a string and outputs the probabilities of each possible succeeding character, with no additional assumptions. It will then add or subtract from this prior model given the query parameters. For example, if one or more biomes are specified, there will be different probabilities of succeeding characters, and the model will be adjusted toward these values. The use of the prior is intended to give more diverse output for biomes where there is not a lot of data, for example tundra. Similarly, given a distance from the coast, it will find the marginal probability when considering the distance.

# Sources
- Biome data from the [RESOLVE Ecoregions and Biomes](https://www.arcgis.com/home/item.html?id=37ea320eebb647c6838c23f72abae5ef) dataset
- City names and coordinates from [Wikidata](https://w.wiki/8$XZ)

# Credits
- [Alexis Cooper](https://github.com/AACooper1): consultation about linguistics stuff
- [Connor Shea](https://github.com/connorshea): Wikidata help, in theory
- [OtherwiseJunk](https://github.com/OtherwiseJunk): code review