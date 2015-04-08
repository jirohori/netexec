# netexec
Automatically exported from code.google.com/p/netexec

netexec – Remote Command Execution

The command shell is a method of directly communicating with a remote system via an instruction, or command-line interface. Existing remote command execution tools besides being difficult to set up, require client software to be installed on the remote systems that you wish to access.
netexec allows you to execute a command on a remote machine without physically logging in to that machine. Full interactivity for console applications is provided. No client software installation is required.
This tool helps system administrators perform housekeeping tasks and helps security auditors to execute programs on remote systems as the direct access to those are not always possible.

netexec's uses include

      launching command shells to remote systems
      remote copy capabilities to specified directories
      enabling tools and applications to display information about remote systems
      enabling upload and download of files at runtime

Installation

Just copy netexec onto your executable path. Executing "netexec" with no command line options displays usage syntax.

Usage

The syntax is straightforward and easy to learn, making remote command-line administration and security auditing much more efficient.

    Usage:            netexec [options..] < program > [arguments]

    <computer>        Remote computer name or IP address. This is a mandatory 
                      parameter.

    -u <user>         Username for logging on remote computer. Logs in with 
                      current credentials, if not supplied.

    -p <password>     Password for logging on remote computer. Prompts for 
                      password, if not supplied.

    -dir <directory>  Set remote computer's working directory.. The default is 
                      %SystemRoot%\system32\

    -cp               First copy the specified program on the remote machine. The application     
                      must be in the system’s current directory

    -i                Allows the remote program to interact with the desktop

    -upload <file>    Upload a file to the remote computer's working directory

    -download <file>  Download a file from the remote computer's working directory

    -nowait           Do not wait for the program to complete

    -script <file>    Record an entire session to a file

    -e <cmd>          To execute this program from the remote machine in the remote computer's working directory

    -? / -h           Displays the help screen

    -args <arguments> Supply arguments of the program.

    -shellUse         %COMSPEC% to run the shell. In the absence of a program, this is the default behaviour.

Menu at runtime

By pressing the Ctrl+C key combination, the runtime menu will appear as shown below

    Ctrl+U:Upload file
    Ctrl+D:Download file
    Ctrl+X:Cancel
    Ctrl+C:Terminate process

You can enclose applications that have spaces in their name with quotation marks
e.g. 

    netexec 10.10.8.7 "c:\long name\application.exe".

Input is only passed to the remote system when you press the Enter key. Typing Ctrl-C terminates the remote process. Alternatively, you could also write the above command in the following manner:

    netexec 10.10.8.7 application.exe -dir "c:\long name"

If you fail to specify a user name, the remote process runs with the credentials of the logged-in user. Note that the password is transmitted in clear text to the remote system. Arguments supplied to netexec are case sensitive. netexec sets %SystemRoot%\system32\ as the default directory on the remote computer.

Examples

To launch an interactive command prompt on 10.10.8.7 using administrator credentials:

    netexec 10.10.8.7 -u administrator cmd

To upload the file a.exe on the remote system, with the directory on the remote system set to c:\myprogram\, type

    netexec 10.10.8.7 -u administrator a.exe -dir c:\myprogram\

NOTE: Ensure that the file a.exe exists on the remote system
To copy and execute the application myexe.exe on a remote system without leaving a copy of the application on the remote machine,

    netexec 10.10.8.7 -u administrator -cp myexe.exe

To connect to a remote host by the name "foo" with the currently logged in user's credentials and obtain a shell,

    netexec foo
