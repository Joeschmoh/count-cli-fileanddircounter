    ***********************************************************************
    ** Count - A File & Directory counter with free trivial information! **
    **                            Version 2.3                            **
    **     Copyright (C) 2001,2008,2009,2010,2011,2017 Derick Snyder     **
    ***********************************************************************
    **                                                                   **
    **                         Count Readme.txt                          **
    **                                                                   **
    ***********************************************************************

Introduction:
-------------
Count is a simple command-line utility that will count up the space taken up by
all the files and folders from any point file system. It has several options to
control how the output is displayed. 

There are other ways to get the information that Count provides, but I've never
found their output helpful. One of the advantages of writing my own utility is
it works exactly how I want it to. And Count does exactly that, I use it at
home and at work all the time to figure out where all my data is and how much
space it's taking up. I hope you find it as useful as I have.

Count was written in C# and currently only runs on Windows with .NET 2.0 or
higher installed. 

Installation:
-------------
Count does not come with an installer, it will run standalone on a Windows PC
as long as the .NET runtime version 4.5 or higher is installed. On Windows
Vista / 7 machines this is already done for you. However, on Windows XP this
can be easily downloaded and installed. Just use your favorite search engine
and enter ".NET runtime download". I recommend downloading this directly from
Microsoft.

There are 3 files used for running Count - Count.exe, Count.exe.manifest, and
Count.exe.config. Realistically, you only need Count.exe for it to work. Copy
this file anywhere on your local PC hard drive and execute it from a
command-line.

I keep a specific folder on my PC for utilities like this and add the folder to
my default PATH environment variable. That allows me to run the utility from
any location on the command-line, and is a technique similar to having a /bin
folder in your home folder on a Linux/Unix system.

As a command-line utility, it runs from the command prompt (some call this a
DOS box), or from a PowerShell window. All of this should work whether you have
administrative privileges or not, however, without admin privileges you can
only setup Count to run from your user account and not for an entire machine.
And of course, Count will not be able to gather information on folders that you
do not have access. Count will, however, gracefully skip these folders and
indicate it as an "error" found during the scan.

If all the above installation information makes no sense, then you should
consider whether this utility is suited for your needs. 

Options:
--------
All switches use a forward slash ("/") before the switch, and are case
insensitive. The root folder can be placed at any position in the parameters,
but can only be given once. Count will not parse multiple root folder listings. 

If you give a bad option, Count is polite in explaining what is wrong.

Current switches are shown in the below table:

Switch   Description
/T    Totals only - displays only a summary of the statistics and does not
      display individual line item information about each folder.

/F=x  File filter - only counts files that match the given file filter. The
      default is *.*. All directories will be scanned regardless of the file
      filter. An example is /F=*.jpg to count only JPEG files.

/L=x  Directory levels - displays information on directories that are no deeper
      than this value. The default is 1, showing only the first set of folders
      in a given directory. Setting this to 0 is the same as /T.

/S=x  Sort order - This can be "N" to sort by name, or "S" to sort by size.
      Size means the total nested folder size (i.e. the folder and all it's
      children). Append an "R" to the option to reverse sort. Example /S=NR
      sorts by name from Z to A.

/N    Normal ASCII characters - uses ASCII characters when drawing lines to
      line up the sub folder levels. Default is to use extended ASCII 
      characters. However this doesn't display correctly in all editors and 
      printers.

/J    Skip junction points - skips any junction points found (also called
      reparse points) in the file system. These can be found in NTFS volumes
      and are used like hard or soft links in a Linux/Unix file system.

/O=x  Output to file - sends the output of Count to the designated file.
      Default is to display to stdout. Functionally, this is the similar to
      using a redirect on the command-line. Errors are outputted to stderr and
      will not be affected by this switch.

/OW=x Output line width - sets the width of a single output line. This can be
      used with or without the /O= switch. By default Count senses the console
      width when a console (i.e. stdout) and sets its width appropriately. When
      outputting to a file, Count defaults to 80 characters. This switch can be
      used to override the default in either case. However, the value is never
      allowed to go below 80 or above 250 characters regardless how the width
      is set.

/V    Version - displays version information, i.e. the command-line equivalent
      of Help | About.

/?    Help - displays a simple summary of the command-line options.

Common Examples:
----------------
My most common use is no parameters. It will default to count in the current
folder, display one level of sub folders, and sort by size (smallest first).
Some other common uses are:

Count D:\ /L=2 /J 
Display 2 levels of sub folders and skip any junction points. If you use
junction points in NTFS, this limits everything to the current volume.

Count /S=SR
Sort files by size in reverse order - this puts the largest folders at the top. 

Count C:\ /S=SR /L=10 /J /N /O=MyOutputFile.txt /OW=120
I do this for dumping lots of information into a file for browsing. It will
sort by size in reverse order, and display up to 10 levels of sub folders. It
skips all junction points which can be useful on Drive C because there are
certain standard junction points used in the OS that can cause infinite loops.
All the output goes to my text file and it will be formatted to 120 columns.
This is useful since many text editors and wide screen monitors can handle well
over 80 columns. Finally, "/N" uses standard ASCII characters rather than the
extended character set. Depending on your text editor, extended characters may
not show up correctly.

A helpful hint - On Windows Vista or Windows 7, run Count as an administrator
when you scan an entire drive. This give Count permission to parse more folders
giving you better information, particularly on primary OS drives.

Short History:
--------------
The first public release of Count is version 2.2 in 2011. However, I initially
wrote Count back in the early 90's in Turbo Pascal. To be honest, I don't
remember when I wrote it and it may go back even into late 80's. I needed to
figure out why my hard drive was full on my DOS machine and this utility did
that. I consider this to be my version 1. 

In 2001, I ported Count to Delphi and created version 2.0, also adding in some
various options and having to upgrade all it's counting variables to 64-bit.
It's funny how when I first wrote the utility I used 32-bit variables and I
could only dream they would overflow. By 2001 this was very limiting and with
64-bit variables I could finally handle really large hard drives that were
becoming common. 

This also led me to add in "human readable" output sizes because who really
cares if you are using 2,382,488,590,731 bytes in a folder? 2.17 TiB
(terabytes) is sufficient for me.

In 2008 I rewrote Count in C#. This was for several reasons. First, it was a
learning exercise in C# since my background is Pascal and C/C++. Second, it was
a chance to re-write the "counter" portion to store it's statistics for later
display rather than just dump the information as it found it. 

Back in the DOS days, PCs didn't have the memory to gather all the data on the
hard drive. So dumping the data to the screen as it was generated significantly
reduced the memory requirement. It was also easier to write.

With the information gathered in one step and displayed in another step I could
finally add another key feature to Count - sorting. I also laugh that it took
me 20 years to add a key feature that makes Count's output much more useful. 
In particular, sorting by size is very useful because large folders congregate
to the top or bottom. It took me only a few times testing this feature to make
sorting by size a default option. I use this feature frequently to answer
questions such as "why is my hard drive full?" 

The Count 2.2 release doesn't add any new features. I did it to post the
utility and source code publicly. I added the GPLv3 license, appropriate
copyright and other identifying information appropriate in a "production"
release.

Version 2.2 makes this a freely available application to any and all that want
to use it (assuming you comply with the terms of GPLv3 of course). Since I only
ever used previous versions privately, I will not release the source code or
binaries for these versions.

Licensing, Copyright, & Contact Information:
--------------------------------------------
Count version 2.x is Copyright 2001,2008,2009,2010,2011 by Derick Snyder.

You can contact me at "derickos at hotmail.com", replacing "at" with the @
symbol for e-mail.

Count is licensed under the GNU General Public License (GPL) version 3.

Count is free software: you can redistribute it and/or modify it under the
terms of the GNU General Public License as published by the Free Software
Foundation, either version 3 of the License, or (at your option) any later
version.

Count is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.  See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with
Count, generally in the file "LICENSE.TXT".  If not, see
<http://www.gnu.org/licenses/>. 

Platforms:
----------
Count is currently a Windows only program, running on Windows XP and higher.

Specifically tested variants are:
   *  Windows XP SP3 (32-bit)
   *  Windows 7 (64-bit)
   *  Windows Server 2003 R2 SP2 (32-bit)
   *  Windows Server 2008 R2

The Future:
-----------
I have several ideas of things to add for the future, and I'd love feedback on
more improvements. Some of these items are:

   *  Make count work on Linux via Mono, ideally as the same executable for
      either OS. It uses standard CLR calls so it should be an easy port except
      that it uses a Windows specific API to get the amount of free space on a
      drive. Also, I'm not sure properly handles "\" vs "/" in file names.
   *  More sorting options? Maybe, but I haven't had a need for more than size
      or name.
   *  A GUI version? An interesting possibility, but I'm not sure how useful
      that is.
   *  Filtered output such as only showing folders above or below a given size,
      only showing empty folders, or folders with files larger than a certain
      size.
   *  Hiding system folders from output. Currently Count shows any folder it
      finds and doesn't care if the folder is hidden, system, or otherwise. But
      some folders like the Recycle Bin or System Volume Information really
      aren't very useful to look at.
   *  Multiple file filters such as *.jpg,*.gif,*.png,*.tiff
