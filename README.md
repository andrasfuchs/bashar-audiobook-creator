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
bashar-audiobook-creator "BasharAudiobook2013.m4b"

## Screenshots
![image](https://user-images.githubusercontent.com/910321/158618532-3280c513-76e4-4681-9c21-ce14c3530ad2.png)

## Timecodes file structure
Timecodes define the chapters in the audiobook. Every chapter must be defined as a segment of an audio file. The start and the stop timecodes must follow the "HH:mm:ss" format.

These chapter definitions are stored in a CSV file that has the following columns: `Index`, `IsIncluded`, `Filename`, `StartTime`, `StopTime`, `Chapter`.

## Recommended audiobook player
I recommend to use the [Smart AudioBook Player](https://play.google.com/store/apps/details?id=ak.alizandro.smartaudiobookplayer).

## Contribute
If you like this project, please consider contributing. If you found a bug or have an idea [create a new issue](https://github.com/andrasfuchs/bashar-audiobook-creator/issues/new). The best way to improve this tool is to create a pull request that I can review and merge. If you need help with forking this repo or creating a pull request with your changes, let me know.
