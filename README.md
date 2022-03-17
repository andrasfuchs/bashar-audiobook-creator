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
I recommend to use the [Smart AudioBook Player](https://play.google.com/store/apps/details?id=ak.alizandro.smartaudiobookplayer). It's easy to use, reads the file metadata correctly and it remembers where you left last time.

Audiobook selection and player screens:

![image](https://user-images.githubusercontent.com/910321/158899513-55aff073-2d10-4b01-a717-7608ad83b3b4.png)
![image](https://user-images.githubusercontent.com/910321/158899733-32472cb4-5d89-4596-a0d1-221cf996f65d.png)

File and chapter selection screens:

![image](https://user-images.githubusercontent.com/910321/158899416-402f6c74-ff37-4461-99dd-2a4a591d5ec4.png)
![image](https://user-images.githubusercontent.com/910321/158899695-86be826c-4f14-475a-92dd-3e82bf914210.png)


## Compiled audiobooks
There are a few audiobooks that were created with this tools which are available on IPFS (on the interplanetary file system).
 * Bashar Monologes 2020.m4b: [IPFS link](https://bafybeibduxpkseqg7t4ovrbtpcgj3m4s5psjafjjnf57xgbo72l5zjymfy.ipns.dweb.link/)
 * Bashar Monologes 2021.m4b: [IPFS link](https://bafybeie4pn5xjt3z3r5lg7h537ydxtwy3lwmm5c5f5dijjeneggidph3xi.ipns.dweb.link/)

## Contribute
If you like this project, please consider contributing. 
The easiest way to start is to edit the timecodes [in our CSV file](https://github.com/andrasfuchs/bashar-audiobook-creator/blob/main/audiobook_timecodes.csv), indicating where the monologe starts and ends in a given audio file.
If you find a bug or have an idea [create a new issue](https://github.com/andrasfuchs/bashar-audiobook-creator/issues/new). The best way to improve this tool is to create a pull request that I can review and merge. If you need help with forking this repo or creating a pull request with your changes, let me know.
