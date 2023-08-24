# QDebug
Testing phase for planned project to do the following:
Connect to PLCs from multiple different vendors, main target will be Siemens and CoDeSyS type PLCs
Use their respective OPCUA server to read pointers to plc area using a DWORD size object 1st word db number, 2nd word byte offset with cutoff at 14th bit to use 15th bit(last) in DWORD as an update bit
On true of the last bit it will then read the memory area of the pointer into the server and cache into one of the following DBs:
- MongoDB main focus
- Sql will come later

Then you will be able to read this cached from the db on connect to server, on update it will send to connected clients

Example communication:
PLC -> OPCUA -> Server with configured connection to OPCUA -> Client
Client modification -> Server -> S7Net/respective lib -> PLC
