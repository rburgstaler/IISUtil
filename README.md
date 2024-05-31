IISUtil
=======

Simple command line app meant to be called from scripts for IIS special config and ACME certe fetching.

# Instructions
In order to get ACME wildcard cert fetches working properly with this app.  You will need to do some special DNS setup.  Read the instructions from `ACMEClientLib.git\README.md` to get this setup.


# Exaple usage


* Install the *.pfx cert into the Windows Certificate Store
  ```
  IISUtilCmd.exe /InstallCertPFXFileName "D:\Debug\MyCert.pfx" /InstallCertPFXPassword "MyPassword" /InstallCertStore WebHosting
  ```
* In IIS, update all certs that have a corresponding binding match to the DNSQuery wild card DNS.  ie. *.commonname.com would match test.commonname.com and hello.commonname.com but not sub1.sub2.commonname.com.
  ```
  IISUtilCmd.exe /UpdateBindingCert "WebHosting\jljkadSHA1Match" /DNSQuery "*.commonname.com"
  ```
* Get information about the *.pem file or *.pfx file.  File extension will be used to determine the type.
  ```
  IISUtil.exe /GetCertInfoFileName D:\Debug\MyCert.pfx /GetCertInfoPassword MyPassword
  ```
