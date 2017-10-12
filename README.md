See An [Gurux](http://www.gurux.org/ "Gurux") for an overview.

Join the Gurux Community or follow [@Gurux](https://twitter.com/guruxorg "@Gurux") for project updates.

Gurux.DLMS library is a high-performance .NET component that helps you to read you DLMS/COSEM compatible electricity, gas or water meters. We have try to make component so easy to use that you do not need understand protocol at all.

For more info check out [Gurux.DLMS](http://www.gurux.fi/index.php?q=Gurux.DLMS "Gurux.DLMS").

We are updating documentation on Gurux web page. 

Read should read [DLMS/COSEM FAQ](http://www.gurux.org/index.php?q=DLMSCOSEMFAQ) first to get started. Read Instructions for making your own [meter reading application](http://www.gurux.org/index.php?q=DLMSIntro) or build own 
DLMS/COSEM [meter/simulator/proxy](http://www.gurux.org/index.php?q=OwnDLMSMeter).

If you have problems you can ask your questions in Gurux [Forum](http://www.gurux.org/forum).


Gurux DLMS Conformance Test Tool
=========================== 
Purpose of Gurux DLMS Conformance Test Tool is that you can verify is your meter really DLMS compatible. There are a lot of new meter manufacturers who claims that their meters are supporting DLMS protocol. This has been a problem, because there is no tester for meter byers who wants to compare meters. Manufacturer can say that meters have passes DLMS Certificate tests. Using this tool you can verify is it true. In default It runs basic test that verifies is your supporting basic DLMS functionality. You can read more from this from DLMS Breen Book: 4.1.8.4 Mandatory contents of a COSEM logical device. It also reads all data from the meter and verifies that data structures are correct. Gurux Conformance Test Tool can't check is content of data correct. Tool can't know is value of active or reactive energy correct. When you start the application, it will show dialog where you need to set correct settings. 
After that press OK button and test starts. As a result Gurux DLMS Conformance Test Tool will generate a [report](http://www.gurux.fi/files/Gurux.Conformance.Test.Report.htm). 

Command line parameters
=========================== 
You can also give command line parameters for the app. You can give example meter ip address and port number and input xml file or folder if you want to execute several messages. Connection to the meter is closed after each file is executed. If you want to ask several objecs, you can add them to the one file. 
You can read data from Gurux example server using HDLC framing and Logical name referencing like this: 
Gurux.Conformance.Test -h localhost -p 4061 

Parameters are: 
-h host name or IP address.
-p port number or name (Example: 1000).
-S Serial port settings (Example: COM1:9600:8None1).
-i IEC is a start protocol.
-a Authentication (None, Low, High).
-P Password for authentication.
-c Client address. (Default: 16).
-s Server address. (Default: 1).
-n Server address as serial number.
-r [sn, sn] Short name or Logical Name (default) referencing is used.
-w WRAPPER profile is used. HDLC is default. 
-t [Error, Warning, Info, Verbose] Trace messages.
Example: 
Gurux.Conformance.Test with TCP/IP connection: 
Gurux.Conformance.Test -r LN -c 16 -s 1 -h [Meter IP Address] -p [Meter Port No] 
Gurux.Conformance.Test using serial port connection: 
Gurux.Conformance.Test -r SN -c 16 -s 1 -S COM1:9600:8None1 -i 
Gurux.Conformance.Test -S COM1:9600:8None1 -c 16 -s 1 -a Low -P [password] 

COSEM objects
=========================== 
All COSEM objects are described as XML. This helps you to understand structure of COSEM objects and you can easily add own COSEM objects if needed. Strucure is like this:

```csharp
<?xml version="1.0" encoding="utf-8"?>
<Messages>
  <GetRequest>
    <GetRequestNormal>
      <InvokeIdAndPriority Value="193" />
      <AttributeDescriptor>
        <!--DATA-->
        <ClassId Value="1" />
        <InstanceId Value="00002A0000FF" />
        <AttributeId Value="2" />
      </AttributeDescriptor>
    </GetRequestNormal>
  </GetRequest>
  <GetResponse>
    <GetResponseNormal>
      <InvokeIdAndPriority Value="193" />
      <Result>
        <Data>
          <None Value="*" />
        </Data>
      </Result>
    </GetResponseNormal>
  </GetResponse>
</Messages>
``` 

In reply packet you can describe that content of data can be anything by giving * as a value. Like this: 
```csharp
<UInt8 Value="*" />
``` 

If data type is ignored set data type to none like this: 
```csharp
<None Value="*" />
``` 