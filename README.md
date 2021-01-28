# .NET 5 certificate authorization bug reproduce kit

Code that is required to reproduce the issue https://github.com/dotnet/runtime/issues/47580

# Required tools

* .NET Core SDK 3.1
* .NET SDK 5.0, 5.0.1, or 5.0.2
* Go 1.13+

# Required OS
Debian Buster or Ubuntu 20.04. The problem is not reproducible on Windows

# Certificate information
Example certificate is issued by Let's Encrypt for one time free domain. It will expire, so in order to reproduce the bug after certificate expiration one need to change system clock to January 28 2021.

# Steps to reproduce the bug
## Buggy behaviour
* Install the required tools
* Compile and run the server
```
cd Server
go run server.go
```
* Open the other terminal tab and run the client
```
cd Client/CertificateAuthBug
dotnet run --project CertificateAuthBug.csproj
```
* You should see that first two requests are successful, then the following two requests are throwing exception, and the last one if successful
## Clearing SSL cache
* Run the same client with any additional console argument to clear SSL cache before third attempt:
```
dotnet run --project CertificateAuthBug.csproj any_argument
```
* All five requests will run just fine.
## Checking .NET Core 3.1
* Edit file CertificateAuthBug.csproj and change line:
```
<TargetFramework>net5.0</TargetFramework>
```
with:
```
<TargetFramework>netcoreapp3.1</TargetFramework>
```
* Run the client again without additional arguments
* All five requests will run just fine.
## Change server CA settings
* Uncomment the following line in server.go:
```
//ClientCAs: caCertPool,
```
* Run the server and then the client (when targeting .NET 5 without additional arguments).
* All five requests will run just fine.
