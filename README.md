# Introduction 
FilterPolishZ is a tool that simplifies the development and management of .filter (Path of Exile filters) files. It has been specifically designed as a personal automation assistant and has a very niche usecase. The codebase however (with some deveopment knowledge) can be adjusted fairly easily to generate other filters.

# Features
TODO: Guide users through getting your code up and running on their own system. In this section you can talk about:
1.	Parses .filter files and creates an internal representation
2.  Rebuilds and improves the readability .filter files.
3.	Performs automatic table of content adjustments
4.	Performs automatic cleaning of .filter files
5.  Works with filter-tags to create sub-strictnesses and sub-styles of the filter.
6.  Uses the poe.ninja API to gather economy data
7.  Performs eoconomy-based auto-tiering for uniques, divination cars, fossils, shaper/elder basetypes.
8.  Improves the auto-tiering by enriching and data-mining information
9.  Provides a comfortable UI to do tiering, tag-modification an other operations
10. Assists in CI/CD operations
11. Anonates economy data with "aspects" to incease tiering accuracy.

# Build and Test

( section is work in progress )

1) Building a .NET environment with WPF (such as visual studio 2019)
2) To start you also need a seedfilter and aspectfiles.
- You can get the seedfilter in my filter-repository under addtional files ( https://github.com/NeverSinkDev/NeverSink-Filter/tree/master/ADDITIONAL-FILES/SeedFilter )
- The aspect files can be found here ( https://github.com/NeverSinkDev/Filter-ItemEconomyAspects )
3) You'll have to adjust the folders in the project cofiguration, that links to the seed filter and aspect files.

# This project relies on the following nugget libraries/packages:

- Material Design for XAML
- Fody

# Special thanks:

Tobnac
Patreon Supporters ( https://www.patreon.com/Neversink )
