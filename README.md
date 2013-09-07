# QCAutocomplete.NET

## Introduction

QCAutocomplete.NET is a web-service for generating suggestions to auto-complete C# code. It is provided in its raw and messy form. It will not build until tidied up to support a generic REST request. 

## How to Build

1. Get NRefactory
  Get NRefactory from https://github.com/icsharpcode/NRefactory and follow the "How to compile" instructions in the README 
  to get it built (along with the Mono.Cecil and IKVM dependencies).
2. Get QCCodingServices.NET
  NOTE: Once you do this, the following 4 directories should be siblings of one another:
  * cecil
  * ikvm
  * NRefactory
  * QCCodingServices.NET
3. QCCodingServices.NET presently assumes you will have Nuget installed (to simplify dependency resolution of ServiceStack, JSON.net, etc.)
4. Open QCCodingServices.NET.sln in your favorite .NET IDE and build.

## Usage

Code is pulled from the database and compiled on the autocomplete machine by project and user Id. From here the cursor position is used to determine the autocomplete suggestions which are parsed into a useful string of the type, and returned via JSON encoding to the requester.

## Plans for 2013

 - Remove references to QuantConnect file system
 - Modify the code base to accept REST code submissions
 - Cache the partially compiled code per user web session.
 - Extend to return the compile errors and warnings.
 
## Credit

Originally contributed by Paul Miller (http://www.linkedin.com/pub/paul-miller/9/559/a69)

## License

Provided with the Apache 2.0 license.

## About QuantConnect

QuantConnect is seeking to democratize algorithmic trading through providing powerful tools and free financial data. With our online IDE engineers can design strategies in C#, and backtest them across 15 years of free high resolution financial data. Feel free to reach out to the QC Team -- contact@quantconnect.com
