# DacConfigManager
Read all persistent variables (primitive types and string) from a Beckhoff TwinCAT target, and write it to an xml file. 
Or read values from an xml file, then write to the corresponding variables in a Beckhoff TwinCAT target

The intended work flow is:
1. read persistent variables from the target
2. edit the values in the xml file
3. write value back to the target

Requires .NET 6.0 or higher

Command line parameters:
read/write AmsNetID Port file

file is required for write operation, and optional for read operation.

Examples on how to use the program:
1. To read persistent variables from the target
    DacConfigManager.exe read "127.0.0.1.1.1" 851 confFile.xml
2. To write variables to the target
    DacConfigManager.exe write "127.0.0.1.1.1" 851 confFile.xml
