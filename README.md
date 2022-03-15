# Bashar Audiobook Creator
Creates an audiobook based on Bashar audio files.

This command line tool does the following steps:
* Enumerates all audio files in a given folder
* Looks up the monologe start and end times in an Excel table
* Cuts the audio file according the defined times
* Removes the silence from its start and end
* Applies loudness normalization
* Compiles all processed files into a m4b
* Adds metadata like a thumbnail and chapter names to the m4b

## Usage
bashar-audiobook-creator "c:\Bashar\Audio\2013" "BasharAudiobook2013.m4b"
