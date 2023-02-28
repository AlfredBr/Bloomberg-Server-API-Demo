# Bloomberg API Demo Client
This is a simple C# console app [^1] to demonstrate the use of the **Bloomberg Server API** on Microsoft Windows.

![screenshot](https://github.com/AlfredBr/Bloomberg-Server-API-Demo/blob/main/Bloomberg%20Demo%20App.png)

Note: this will also work with **Bloomberg Desktop API** if you have a Bloomberg terminal.

## Requirements

To compile and run this, you'll need:

- [ ] A Windows PC with Microsoft net6.0 (or greater) installed.
- [ ] The Bloomberg API libraries (Bloomberg.Blpapi) from the Bloomberg website. [^2]

At this point you should be able to get _Demo Mode_ working with fake data.  To get real Bloomberg data, you'll need:

- [ ] A Bloomberg SAPI license (and assorted hardware infrastructure) and a Bloomberg username and UUID.[^3]
- [ ] The hostname or IP address of your Bloomberg server (or *localhost* if you _do_ have Bloomberg terminal software).

[^1]: https://github.com/gui-cs/Terminal.Gui
[^2]: https://bcms.bloomberg.com/BLPAPI-Generic/blpapi_dotnet_3.19.2.1.zip
[^3]: https://www.bloomberg.com/professional/support/api-library/
