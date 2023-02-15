# Bloomberg Server API Demo Client
This is a simple C# console app to demo the use of the Bloomberg Server API on Microsoft Windows.

This may also work with desktop API if you have a Bloomberg terminal.

If you don't have a Bloomberg Terminal, you'll need four things to get this working:

- [ ] A C# compiler and Microsoft net6.0 (or greater) installed.
- [ ] The Bloomberg API libraries (Bloomberg.Blpapi) from the Bloomberg website. [^1]

- [ ] A Bloomberg SAPI license (and assorted hardware infrastructure) and a Bloomberg username and UUID.[^2]
- [ ] The hostname or IP address of your Bloomberg server (or *localhost* if you _do_ have a Bloomberg terminal).

[^1]: https://bcms.bloomberg.com/BLPAPI-Generic/blpapi_dotnet_3.19.2.1.zip
[^2]: https://www.bloomberg.com/professional/support/api-library/